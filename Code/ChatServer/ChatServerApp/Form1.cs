using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatServerApp
{
    /// <summary>
    /// Thông tin về một file đang được truyền, để quyết định gửi public hay private.
    /// </summary>
    internal class PendingFileTransfer
    {
        public bool IsPrivate { get; set; }
        public string Sender { get; set; } = "";
        public string Receiver { get; set; } = "";
        public long Size { get; set; }
    }

    public partial class Form1 : Form
    {
        private TcpListener server;
        private Thread listenThread;
        private bool isRunning = false;
        private readonly OnlineUserManager _onlineUserManager = new OnlineUserManager();
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB limit for forwarded files
        private readonly object _writeLock = new object();

        /// <summary>
        /// Lưu trạng thái file đang chờ cho mỗi client (set khi nhận SEND_FILE_* command, đọc khi nhận file bytes).
        /// </summary>
        private readonly Dictionary<TcpClient, PendingFileTransfer> _pendingFileTransfers
            = new Dictionary<TcpClient, PendingFileTransfer>();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(txtPort.Text);
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                isRunning = true;

                listenThread = new Thread(ListenForClients);
                listenThread.IsBackground = true;
                listenThread.Start();

                LogMessage($"[HỆ THỐNG] Server khởi động trên cổng {port}");
                UpdateStatusUI();

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                txtPort.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi chạy: " + ex.Message);
                server?.Stop();
            }
        }

        private void ListenForClients()
        {
            try
            {
                while (isRunning)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
            }
            catch (SocketException ex)
            {
                if (isRunning)
                    LogMessage($"[LỖI] ListenForClients: {ex.Message}");
            }
        }

        // ----- Sanitize: loại bỏ \r và \n khỏi username và nội dung text,
        // để \n chỉ còn là dấu phân cách tin nhắn -----
        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace('\r', ' ').Replace('\n', ' ');
        }

        /// <summary>Đọc chính xác count byte từ socket, xử lý TCP fragmentation.</summary>
        private static bool ReceiveExact(Socket socket, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int n = socket.Receive(buffer, total, count - total, SocketFlags.None);
                if (n == 0) return false;
                total += n;
            }
            return true;
        }

        /// <summary>
        /// Đọc chính xác 1 dòng UTF-8 kết thúc bằng \n từ NetworkStream (tối đa 8KB).
        /// Trả về null nếu stream đóng hoặc vượt quá giới hạn.
        /// </summary>
        private static string ReadLineUtf8(NetworkStream stream)
        {
            const int maxLineBytes = 8192;
            var bytes = new System.Collections.Generic.List<byte>(maxLineBytes);
            byte[] one = new byte[1];
            while (true)
            {
                int n = stream.Read(one, 0, 1);
                if (n == 0) return null;
                if (one[0] == (byte)'\n') break;
                if (bytes.Count >= maxLineBytes) return null;
                bytes.Add(one[0]);
            }
            return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Kiểm tra username hợp lệ: không chứa ký tự đặc biệt phá vỡ giao thức.
        /// </summary>
        private static bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            return !username.Contains("|") && !username.Contains(":") && !username.Contains(",") && !username.Contains("\n") && !username.Contains("\r");
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            Socket socket = client.Client;
            string currentUsername = "";
            try { socket.ReceiveTimeout = 0; socket.SendTimeout = 10000; stream.ReadTimeout = System.Threading.Timeout.Infinite; } catch { }
       

            try
            {
                while (isRunning)
                {
                    // Peek 1 byte đầu
                    byte[] peekBuffer = new byte[1];
                    int peekBytes;
                    try
                    {
                        peekBytes = socket.Receive(peekBuffer, 1, SocketFlags.Peek);
                    }
                    catch (SocketException)
                    {
                        break; // Client đã ngắt kết nối
                    }
                    if (peekBytes == 0) break;

                    byte firstByte = peekBuffer[0];

                    if (firstByte == 0x10 || firstByte == 0x11)
                    {
                        ProcessFileTransfer(client, socket, stream, currentUsername, firstByte);
                    }
                    else
                    {
                        if (!ProcessTextCommand(client, stream, ref currentUsername))
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[LỖI] {currentUsername}: {ex.Message}");
            }
            finally
            {
                HandleClientDisconnect(client, currentUsername);
            }
        }

        /// <summary>
        /// Xử lý một command dạng text từ client, đọc từng dòng qua ReadLineUtf8.
        /// </summary>
        /// <returns>false nếu cần thoát vòng lặp (mất kết nối).</returns>
        private bool ProcessTextCommand(TcpClient client, NetworkStream stream, ref string currentUsername)
        {
            string line = ReadLineUtf8(stream);
            if (line == null) return false;
            if (string.IsNullOrEmpty(line)) return true;

            // Chỉ Split tối đa 4 phần: MSG luôn là MSG|sender|content (content có thể chứa '|')
            // Với PRIVATE: PRIVATE|sender|receiver|content (content có thể chứa '|')
            // Server tự ghi đè sender bằng currentUsername đã xác thực
            // Limit = 4: command|arg1|arg2|rest (nội dung rest có '|' không mất)
            string[] parts = line.Split(new[] { '|' }, 4);
            if (parts.Length == 0) return true;

            string command = parts[0];

            switch (command)
            {
                case "LOGIN":
                    HandleLogin(client, stream, parts, ref currentUsername);
                    break;

                case "MSG":
                    // Chỉ cho gửi MSG khi đã LOGIN, luôn dùng currentUsername làm sender
                    if (string.IsNullOrEmpty(currentUsername)) return false;
                    if (parts.Length >= 3)
                        HandlePublicMessage(currentUsername, parts[2]);
                    break;

                case "PRIVATE":
                    if (string.IsNullOrEmpty(currentUsername)) return false;
                    if (parts.Length >= 4)
                        HandlePrivateMessage(currentUsername, parts[2], parts[3]);
                    break;

                case "SEND_FILE_PUBLIC":
                    if (string.IsNullOrEmpty(currentUsername)) return false;
                    if (parts.Length >= 3 && long.TryParse(parts[2], out long pubSize))
                        SetPendingFileTransfer(client, false, currentUsername, "", pubSize);
                    break;

                case "SEND_FILE_PRIVATE":
                    if (string.IsNullOrEmpty(currentUsername)) return false;
                    if (parts.Length >= 4 && long.TryParse(parts[3], out long privSize))
                        SetPendingFileTransfer(client, true, currentUsername, parts[2], privSize);
                    break;

                case "LOGOUT":
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Xử lý đăng nhập.
        /// </summary>
        private void HandleLogin(TcpClient client, NetworkStream stream, string[] parts, ref string currentUsername)
        {
            if (parts.Length < 2) return;

            string username = Sanitize(parts[1]);

            // Kiểm tra username hợp lệ: không chứa ký tự đặc biệt
            if (!IsValidUsername(username))
            {
                LogMessage($"[TỪ CHỐI] {username} chứa ký tự không hợp lệ.");
                SendString(stream, "ERROR|Tên người dùng không được chứa ký tự | : ,");
                SafeCloseClient(client);
                return;
            }

            if (_onlineUserManager.TryAdd(username, client, out var newUserInfo))
            {
                currentUsername = username;
                LogMessage($"[ĐĂNG NHẬP] {currentUsername} đã vào phòng.");
                UpdateStatusUI();

                // Gửi LOGIN_OK
                SendString(stream, "LOGIN_OK");

                // Gửi danh sách online kèm thời gian đăng nhập
                BroadcastOnlineList();
                BroadcastText($"BROADCAST|[Hệ thống] {currentUsername} đã vào phòng!", currentUsername);
            }
            else
            {
                LogMessage($"[TỪ CHỐI] {username} đăng nhập trùng tên.");
                SendString(stream, "ERROR|Tên đăng nhập đã tồn tại!");
                SafeCloseClient(client);
            }
        }

        /// <summary>
        /// Broadcast danh sách online kèm thời gian đăng nhập: UPDATE_ONLINE|user1:HH:mm:ss,user2:HH:mm:ss
        /// </summary>
        private void BroadcastOnlineList()
        {
            var items = new List<string>();
            foreach (var u in _onlineUserManager.AllUsers)
                items.Add($"{u.Username}:{u.LoginTime:HH:mm:ss}");
            BroadcastText($"UPDATE_ONLINE|{string.Join(",", items)}");
        }

        /// <summary>
        /// Gửi tin nhắn public broadcast.
        /// </summary>
        private void HandlePublicMessage(string sender, string content)
        {
            LogMessage($"[CHAT] {sender}: {content}");
            BroadcastText($"BROADCAST|{Sanitize(sender)}: {Sanitize(content)}", sender);
        }

        /// <summary>
        /// Gửi tin nhắn private. Hỗ trợ DELIVERED ACK và RECIPIENT_NOT_FOUND.
        /// </summary>
        private void HandlePrivateMessage(string sender, string receiver, string content)
        {
            string saneSender = Sanitize(sender);
            string saneReceiver = Sanitize(receiver);
            string saneContent = Sanitize(content);

            LogMessage($"[PRIVATE] {saneSender} -> {saneReceiver}: {saneContent}");

            var targetInfo = _onlineUserManager.GetByUsername(saneReceiver);
            if (targetInfo != null)
            {
                // Gửi tin nhắn private cho người nhận
                SendString(targetInfo.Client.GetStream(), $"PRIVATE|{saneSender}|{saneContent}");

                // Gửi DELIVERED ACK cho người gửi
                var senderInfo = _onlineUserManager.GetByUsername(saneSender);
                if (senderInfo != null)
                {
                    SendString(senderInfo.Client.GetStream(), $"DELIVERED|{saneSender}|{saneReceiver}");
                }
            }
            else
            {
                // Người nhận không tồn tại hoặc đã logout
                var senderInfo = _onlineUserManager.GetByUsername(saneSender);
                if (senderInfo != null)
                {
                    SendString(senderInfo.Client.GetStream(), $"RECIPIENT_NOT_FOUND|{saneReceiver}");
                }
            }

            // Luôn echo lại cho người gửi để lưu history
            var senderEcho = _onlineUserManager.GetByUsername(saneSender);
            if (senderEcho != null)
            {
                SendString(senderEcho.Client.GetStream(), $"PRIVATE_ECHO|{saneReceiver}|{saneContent}");
            }
        }

        /// <summary>
        /// Ghi nhận trạng thái file sắp được gửi từ client.
        /// </summary>
        private void SetPendingFileTransfer(TcpClient client, bool isPrivate, string sender, string receiver, long size)
        {
            lock (_pendingFileTransfers)
            {
                _pendingFileTransfers[client] = new PendingFileTransfer
                {
                    IsPrivate = isPrivate,
                    Sender = sender,
                    Receiver = receiver,
                    Size = size
                };
            }
        }

        /// <summary>
        /// Xử lý nhận file (0x10: Ảnh, 0x11: Video).
        /// </summary>
        private void ProcessFileTransfer(TcpClient client, Socket socket, NetworkStream stream, string currentUsername, byte typeCode)
        {
            byte[] header = new byte[9];
            if (!ReceiveExact(socket, header, 9)) return;
            long dataSize = BitConverter.ToInt64(header, 1);

            // Kiểm tra kích thước file
            if (dataSize <= 0 || dataSize > MaxFileSize)
            {
                LogMessage($"[FILE] {currentUsername} gửi tệp quá lớn (>{MaxFileSize / 1024 / 1024}MB) - đã từ chối.");
                DiscardRemainingData(socket, dataSize);
                return;
            }

            // Đọc payload
            byte[] payload = new byte[dataSize];
            int totalReceived = 0;
            while (totalReceived < dataSize)
            {
                int read = socket.Receive(payload, totalReceived, (int)(dataSize - totalReceived), SocketFlags.None);
                if (read == 0) throw new Exception("Mất kết nối khi nhận file");
                totalReceived += read;
            }

            // Gộp header + payload
            byte[] fullPacket = new byte[9 + dataSize];
            Array.Copy(header, 0, fullPacket, 0, 9);
            Array.Copy(payload, 0, fullPacket, 9, dataSize);

            // Lấy thông tin pending
            PendingFileTransfer pending = null;
            lock (_pendingFileTransfers)
            {
                if (_pendingFileTransfers.TryGetValue(client, out pending))
                    _pendingFileTransfers.Remove(client);
            }

            string senderName = pending?.Sender ?? currentUsername;
            bool isPrivate = pending?.IsPrivate ?? false;
            string receiverName = pending?.Receiver ?? "";

            if (isPrivate && !string.IsNullOrEmpty(receiverName))
            {
                // File private: gửi cho người nhận + echo cho người gửi
                // Gộp announce + fullPacket vào một lần lock để atomic
                byte[] announceTarget = Encoding.UTF8.GetBytes($"FILE_ANNOUNCE|{senderName}\n");
                byte[] announceEcho = Encoding.UTF8.GetBytes($"FILE_ANNOUNCE|{senderName} -> {receiverName}\n");

                lock (_writeLock)
                {
                    var targetInfo = _onlineUserManager.GetByUsername(receiverName);
                    if (targetInfo != null)
                    {
                        try
                        {
                            NetworkStream ns = targetInfo.Client.GetStream();
                            if (ns.CanWrite)
                            {
                                ns.Write(announceTarget, 0, announceTarget.Length);
                                ns.Write(fullPacket, 0, fullPacket.Length);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"[LỖI] Gửi file private đến {receiverName}: {ex.Message}");
                        }
                    }

                    var senderInfo = _onlineUserManager.GetByUsername(senderName);
                    if (senderInfo != null && senderInfo.Client != client)
                    {
                        try
                        {
                            NetworkStream ns = senderInfo.Client.GetStream();
                            if (ns.CanWrite)
                            {
                                ns.Write(announceEcho, 0, announceEcho.Length);
                                ns.Write(fullPacket, 0, fullPacket.Length);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"[LỖI] Gửi file private echo đến {senderName}: {ex.Message}");
                        }
                    }
                }

                LogMessage($"[FILE PRIVATE] {senderName} gửi 1 tệp tin riêng cho {receiverName}.");
            }
            else
            {
                // File public: broadcast cho tất cả trừ người gửi
                BroadcastFile(fullPacket, senderName);
                LogMessage($"[FILE] {senderName} gửi 1 tệp tin.");
            }
        }

        /// <summary>
        /// Gửi file kèm announce đến tất cả users trừ người gửi.
        /// </summary>
        private void BroadcastFile(byte[] filePacket, string senderUsername)
        {
            // Announce phải kết thúc bằng \n để client nhận diện là tin text trọn vẹn
            byte[] announceData = Encoding.UTF8.GetBytes($"FILE_ANNOUNCE|{senderUsername}\n");

            lock (_writeLock)
            {
                foreach (var user in _onlineUserManager.AllUsers)
                {
                    if (user.Username == senderUsername) continue;

                    try
                    {
                        NetworkStream ns = user.Client.GetStream();
                        if (ns.CanWrite)
                        {
                            ns.Write(announceData, 0, announceData.Length);
                            ns.Write(filePacket, 0, filePacket.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"[LỖI] Gửi file đến {user.Username}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý khi client disconnect (finally block).
        /// </summary>
        private void HandleClientDisconnect(TcpClient client, string currentUsername)
        {
            // Xoá pending file transfer nếu có
            lock (_pendingFileTransfers)
                _pendingFileTransfers.Remove(client);

            if (!string.IsNullOrEmpty(currentUsername))
            {
                _onlineUserManager.TryRemove(currentUsername);
                LogMessage($"[NGẮT KẾT NỐI] {currentUsername} đã rời đi.");

                BroadcastOnlineList();
                BroadcastText($"BROADCAST|[Hệ thống] {currentUsername} đã thoát!", currentUsername);
            }

            SafeCloseClient(client);
            UpdateStatusUI();
        }

        /// <summary>
        /// Hủy dữ liệu thừa từ socket (khi file quá lớn hoặc không hợp lệ).
        /// Tránh loop vô tận: tối đa 1000 lần lặp tương ứng ~8MB discard mỗi lần gọi.
        /// </summary>
        private void DiscardRemainingData(Socket socket, long totalSize)
        {
            byte[] discard = new byte[8192];
            long totalDiscarded = 0;
            int maxIterations = 1000;
            int iterations = 0;
            while (totalDiscarded < totalSize && iterations < maxIterations)
            {
                int toRead = (int)Math.Min(totalSize - totalDiscarded, discard.Length);
                int read = socket.Receive(discard, 0, toRead, SocketFlags.None);
                if (read == 0) break;
                totalDiscarded += read;
                iterations++;
            }
        }

        /// <summary>
        /// Gửi một chuỗi text lên stream. Luôn thêm \n ở cuối để đóng khung.
        /// </summary>
        private void SendString(NetworkStream stream, string message)
        {
            if (stream == null) return;
            // Đảm bảo \n chỉ là delimiter: loại bỏ \r, \n trong nội dung
            byte[] data = Encoding.UTF8.GetBytes(Sanitize(message) + "\n");
            lock (_writeLock)
            {
                try
                {
                    if (stream.CanWrite)
                        stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    LogMessage($"[LỖI] SendString: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Ghi bytes trực tiếp lên stream (dùng cho file).
        /// </summary>
        private void WriteBytesToStream(NetworkStream stream, byte[] data)
        {
            if (stream == null) return;
            lock (_writeLock)
            {
                try
                {
                    if (stream.CanWrite)
                        stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    LogMessage($"[LỖI] WriteBytesToStream: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Broadcast text tới tất cả users, có thể loại trừ một user.
        /// Mỗi tin đều kết thúc bằng \n.
        /// </summary>
        private void BroadcastText(string message, string excludeUsername = "")
        {
            byte[] data = Encoding.UTF8.GetBytes(Sanitize(message) + "\n");
            lock (_writeLock)
            {
                foreach (var user in _onlineUserManager.AllUsers)
                {
                    if (user.Username == excludeUsername) continue;
                    if (!IsClientConnected(user.Client)) continue;

                    try
                    {
                        NetworkStream ns = user.Client.GetStream();
                        if (ns.CanWrite)
                            ns.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"[LỖI] Broadcast đến {user.Username}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Kiểm tra kết nối TCP còn sống không.
        /// </summary>
        private static bool IsClientConnected(TcpClient client)
        {
            try
            {
                return client?.Client != null
                    && client.Client.Connected
                    && !(client.Client.Poll(1000, SelectMode.SelectRead) && client.Client.Available == 0);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Đóng client an toàn, không ném exception.
        /// </summary>
        private static void SafeCloseClient(TcpClient client)
        {
            try
            {
                if (client == null) return;
                if (client.Connected)
                    client.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SafeCloseClient: {ex.Message}");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!isRunning) return;
            isRunning = false;

            foreach (var user in _onlineUserManager.AllUsers)
            {
                try
                {
                    if (IsClientConnected(user.Client))
                    {
                        // Gửi lệnh SERVER_SHUTDOWN riêng biệt thay vì lẫn trong BROADCAST
                        SendString(user.Client.GetStream(), "SERVER_SHUTDOWN");
                    }
                    SafeCloseClient(user.Client);
                }
                catch (Exception ex)
                {
                    LogMessage($"[LỖI] Đóng kết nối {user.Username}: {ex.Message}");
                }
            }

            _onlineUserManager.Clear();
            lock (_pendingFileTransfers)
                _pendingFileTransfers.Clear();

            try { server?.Stop(); }
            catch (Exception ex) { LogMessage($"[LỖI] Stop server: {ex.Message}"); }

            LogMessage("[HỆ THỐNG] Server đã dừng.");
            UpdateStatusUI();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            txtPort.Enabled = true;
        }

        private void LogMessage(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(LogMessage), message);
            }
            else
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            }
        }

        private void UpdateStatusUI()
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(UpdateStatusUI));
            }
            else
            {
                lblStatus.Text = $"Số người online: {_onlineUserManager.Count}";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnStop_Click(this, EventArgs.Empty);
        }
    }
}
