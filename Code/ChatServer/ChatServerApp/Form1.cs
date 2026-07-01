using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ChatServerApp
{
    public partial class Form1 : Form
    {
        private TcpListener server;
        private Thread listenThread;
        private bool isRunning = false;
        private ConcurrentDictionary<string, TcpClient> onlineUsers = new ConcurrentDictionary<string, TcpClient>();
        private ConcurrentDictionary<string, string> userAvatars = new ConcurrentDictionary<string, string>();

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
            catch (SocketException) { }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            Socket socket = client.Client;
            string currentUsername = "";

            try
            {
                while (isRunning)
                {
                    // NHÌN LÉN 1 BYTE ĐẦU TIÊN
                    byte[] peekBuffer = new byte[1];
                    int peekBytes = socket.Receive(peekBuffer, 1, SocketFlags.Peek);
                    if (peekBytes == 0) break;

                    byte firstByte = peekBuffer[0];

                    // XỬ LÝ NHẬN FILE (0x10: Ảnh, 0x11: Video) 
                    if (firstByte == 0x10 || firstByte == 0x11)
                    {
                        byte[] header = new byte[9];
                        int headerRead = socket.Receive(header, 9, SocketFlags.None);
                        if (headerRead == 0) break;
                        long dataSize = BitConverter.ToInt64(header, 1);

                        byte[] payload = new byte[dataSize];
                        int totalReceived = 0;

                        while (totalReceived < dataSize)
                        {
                            int read = socket.Receive(payload, totalReceived, (int)(dataSize - totalReceived), SocketFlags.None);
                            if (read == 0) throw new Exception("Mất kết nối khi nhận file");
                            totalReceived += read;
                        }

                        // Gộp lại và Broadcast cho mọi người
                        byte[] fullPacket = new byte[9 + dataSize];
                        Array.Copy(header, 0, fullPacket, 0, 9);
                        Array.Copy(payload, 0, fullPacket, 9, dataSize);

                        BroadcastBytes(fullPacket, currentUsername);
                        LogMessage($"[FILE] {currentUsername} vừa gửi 1 tệp tin.");
                    }
                    // XỬ LÝ FILE RIÊNG (0x20: Ảnh, 0x21: Video)
                    else if (firstByte == 0x20 || firstByte == 0x21)
                    {
                        byte[] header = new byte[9];
                        if (!ReceiveAll(socket, header, 9)) break;
                        long dataSize = BitConverter.ToInt64(header, 1);

                        byte[] lenByte = new byte[1];
                        if (!ReceiveAll(socket, lenByte, 1)) break;
                        int recvLen = lenByte[0];
                        byte[] recvBytes = new byte[recvLen];
                        if (!ReceiveAll(socket, recvBytes, recvLen)) break;
                        string receiver = Encoding.UTF8.GetString(recvBytes);

                        byte[] payload = new byte[dataSize];
                        if (!ReceiveAll(socket, payload, (int)dataSize)) break;

                        byte fwdMarker = (firstByte == 0x20) ? (byte)0x22 : (byte)0x23;
                        byte[] senderBytes = Encoding.UTF8.GetBytes(currentUsername);
                        byte[] fwd = new byte[10 + senderBytes.Length + (int)dataSize];
                        fwd[0] = fwdMarker;
                        Array.Copy(BitConverter.GetBytes((long)dataSize), 0, fwd, 1, 8);
                        fwd[9] = (byte)senderBytes.Length;
                        Array.Copy(senderBytes, 0, fwd, 10, senderBytes.Length);
                        Array.Copy(payload, 0, fwd, 10 + senderBytes.Length, (int)dataSize);

                        if (onlineUsers.TryGetValue(receiver, out TcpClient targetClient))
                        {
                            SendBytesToOne(targetClient, fwd);
                            LogMessage($"[FILE RIÊNG] {currentUsername} → {receiver} ({dataSize} bytes)");
                        }
                        else if (onlineUsers.TryGetValue(currentUsername, out TcpClient sc))
                        {
                            SendToOne(sc, $"WARNING|{receiver} hiện không online!");
                        }
                    }
                    // XỬ LÝ NHẬN   CHỮ (TEXT) 
                    else
                    {
                        byte[] buffer = new byte[512 * 1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string incomingData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        string[] parts = incomingData.Split('|');

                        if (parts.Length == 0) continue;
                        string command = parts[0];

                        switch (command)
                        {
                            case "LOGIN":
                                if (parts.Length >= 2)
                                {
                                    string requestedName = parts[1].Trim().Replace("\n", "");

                                    if (CheckUserExists(requestedName))
                                    {
                                        if (onlineUsers.TryAdd(requestedName, client))
                                        {
                                            currentUsername = requestedName;
                                            string myAvatarBase64 = LoadAvatarFromDatabase(currentUsername);
                                            userAvatars[currentUsername] = myAvatarBase64;

                                            byte[] successData = Encoding.UTF8.GetBytes("SUCCESS|OK\n");
                                            stream.Write(successData, 0, successData.Length);
                                            Thread.Sleep(100);

                                            LogMessage($"[ĐĂNG NHẬP] {currentUsername} đã vào phòng.");
                                            UpdateStatusUI();

                                            string userList = string.Join(",", onlineUsers.Keys);
                                            BroadcastString($"UPDATE_ONLINE|{userList}");
                                            Thread.Sleep(50);
                                            BroadcastString($"BROADCAST|[Hệ thống] {currentUsername} đã vào phòng!");
                                            Thread.Sleep(50);

                                            SendStoredAvatarsToClient(client);
                                            Thread.Sleep(50);

                                            string updateMsg = AvatarMessageHelper.BuildUpdateMessage(currentUsername, myAvatarBase64);
                                            BroadcastString(updateMsg, currentUsername);
                                        }
                                        else
                                        {
                                            LogMessage($"[TỪ CHỐI] {requestedName} đăng nhập trùng tên.");
                                            byte[] errorData = Encoding.UTF8.GetBytes("ERROR|Tên đăng nhập đã tồn tại\n");
                                            stream.Write(errorData, 0, errorData.Length);
                                            client.Close();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        LogMessage($"[TỪ CHỐI] {requestedName} chưa đăng ký tài khoản.");
                                        byte[] errorData = Encoding.UTF8.GetBytes("ERROR|Tài khoản chưa tồn tại. Vui lòng đăng ký trước!\n");
                                        stream.Write(errorData, 0, errorData.Length);
                                        client.Close();
                                        return;
                                    }
                                }
                                break;

                            case "PRIVATE":
                                if (parts.Length >= 4)
                                {
                                    string pSender = parts[1];
                                    string pReceiver = parts[2];
                                    string pContent = parts[3];
                                    LogMessage($"[RIÊNG] {pSender} -> {pReceiver}: {pContent}");

                                    if (onlineUsers.TryGetValue(pReceiver, out TcpClient targetClient))
                                    {
                                        SendToOne(targetClient, $"PRIVATE|{pSender}|{pContent}");
                                    }
                                    else
                                    {
                                        if (onlineUsers.TryGetValue(pSender, out TcpClient senderClient))
                                            SendToOne(senderClient, $"WARNING|{pReceiver} hiện không online!");
                                    }
                                }
                                break;

                            case "REGISTER":
                                if (parts.Length >= 2)
                                {
                                    string regName = parts[1].Trim().Replace("\n", "");

                                    if (CheckUserExists(regName))
                                    {
                                        byte[] errorData = Encoding.UTF8.GetBytes("ERROR|Tên đăng nhập đã có người sử dụng!\n");
                                        stream.Write(errorData, 0, errorData.Length);
                                    }
                                    else
                                    {
                                        // Tạo mới vào DB
                                        if (CreateNewUser(regName))
                                        {
                                            byte[] successData = Encoding.UTF8.GetBytes("REGISTER_OK\n");
                                            stream.Write(successData, 0, successData.Length);
                                            LogMessage($"[ĐĂNG KÝ] Đã tạo tài khoản mới: {regName}");
                                        }
                                    }

                                    client.Close();
                                    return;
                                }
                                break;

                            case "MSG":
                                if (parts.Length >= 3)
                                {
                                    string sender = parts[1];
                                    string content = parts[2];
                                    LogMessage($"[CHAT] {sender}: {content}");
                                    BroadcastString($"BROADCAST|{sender}: {content}", currentUsername);
                                }
                                break;
                            
                            case "REPLY":
                                if (parts.Length >= 4)
                                {
                                    string rSender = parts[1];
                                    string rQuoted = parts[2];
                                    string rContent = parts[3];
                                    LogMessage($"[REPLY] {rSender} trả lời một tin nhắn.");
                                    BroadcastString($"RMSG|{rSender}|{rQuoted}|{rContent}", currentUsername);
                                }
                                break;

                            case "FWD":
                                if (parts.Length >= 3)
                                {
                                    string fSender = parts[1];
                                    string fContent = parts[2];
                                    LogMessage($"[FORWARD] {fSender} chuyển tiếp một tin nhắn.");
                                    BroadcastString($"FMSG|{fSender}|{fContent}", currentUsername);
                                }
                                break;

                            case "AVATAR":
                                HandleAvatarUpload(stream, incomingData);
                                break;

                            case "LOGOUT":
                                throw new Exception("Thoát");
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (!string.IsNullOrEmpty(currentUsername))
                {
                    onlineUsers.TryRemove(currentUsername, out _);
                    LogMessage($"[NGẮT KẾT NỐI] {currentUsername} đã rời đi.");

                    // Cập nhật lại danh sách Online khi có người thoát
                    string userList = string.Join(",", onlineUsers.Keys);
                    BroadcastString($"UPDATE_ONLINE|{userList}");
                    Thread.Sleep(60);
                    BroadcastString($"BROADCAST|[Hệ thống] {currentUsername} đã thoát!");
                }
                client.Close();
                UpdateStatusUI();
            }
        }

        private void SendToOne(TcpClient targetClient, string rawMessage)
        {
            try
            {
                if (!rawMessage.EndsWith("\n")) rawMessage += "\n";

                byte[] data = Encoding.UTF8.GetBytes(rawMessage);
                targetClient.GetStream().Write(data, 0, data.Length);
            }
            catch { }
        }

        private void SendBytesToOne(TcpClient targetClient, byte[] data)
        {
            try
            {
                var s = targetClient.GetStream();
                lock (s) { s.Write(data, 0, data.Length); }
            }
            catch { }
        }

        private bool ReceiveAll(System.Net.Sockets.Socket socket, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int r = socket.Receive(buffer, total, count - total, System.Net.Sockets.SocketFlags.None);
                if (r == 0) return false;
                total += r;
            }
            return true;
        }

        private void BroadcastString(string rawMessage, string excludeUsername = "")
        {
            if (!rawMessage.EndsWith("\n")) rawMessage += "\n";

            byte[] data = Encoding.UTF8.GetBytes(rawMessage);
            foreach (var user in onlineUsers)
            {
                if (user.Key != excludeUsername)
                {
                    try
                    {

                        NetworkStream targetStream = user.Value.GetStream();

                        lock (targetStream)
                        {
                            targetStream.Write(data, 0, data.Length);
                        }
                    }
                    catch { }
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!isRunning) return;
            isRunning = false;

            foreach (var user in onlineUsers)
            {
                try
                {
                    byte[] endMsg = Encoding.UTF8.GetBytes("BROADCAST|[Hệ thống] Server đã đóng cửa!|");
                    user.Value.GetStream().Write(endMsg, 0, endMsg.Length);

                    user.Value.Close();
                }
                catch { }
            }
            onlineUsers.Clear();
            try { server?.Stop(); } catch { }

            LogMessage("[HỆ THỐNG] Server đã dừng.");
            UpdateStatusUI();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            txtPort.Enabled = true;
        }

        private bool CheckUserExists(string username)
        {
            string connStr = "Server=127.0.0.1; Port=3366; Database=ChatAppDB; Uid=root; Pwd=;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM TblUsers WHERE Username = @user";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("[LỖI DATABASE]: " + ex.Message);
                    return false;
                }
            }
        }

        private bool CreateNewUser(string username)
        {
            string connStr = "Server=127.0.0.1; Port=3366; Database=ChatAppDB; Uid=root; Pwd=;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO TblUsers (Username) VALUES (@user)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("[LỖI DATABASE]: " + ex.Message);
                    return false;
                }
            }
        }

        private string LoadAvatarFromDatabase(string username)
        {
            string connStr = "Server=127.0.0.1; Port=3366; Database=ChatAppDB; Uid=root; Pwd=;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Avatar FROM TblUsers WHERE Username = @user";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            if (result is byte[] imageBytes)
                                return Convert.ToBase64String(imageBytes);
                            else if (result is string base64Str)
                                return base64Str;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"[LỖI TẢI AVATAR TỪ DB]: {ex.Message}");
                }
            }
            return "";
        }

        private void HandleAvatarUpload(NetworkStream stream, string partialData)
        {
            string fullMessage = AvatarMessageHelper.ReadCompleteAvatarMessage(stream, partialData);
            if (!AvatarMessageHelper.TryParse(fullMessage, out string username, out string base64))
                return;

            try
            {
                string connStr = "Server=127.0.0.1; Port=3366; Database=ChatAppDB; Uid=root; Pwd=;";
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "UPDATE TblUsers SET Avatar = @avt WHERE Username = @user";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (string.IsNullOrEmpty(base64))
                        {
                            cmd.Parameters.AddWithValue("@avt", DBNull.Value);
                        }
                        else
                        {
                            byte[] imageBytes = Convert.FromBase64String(base64);
                            cmd.Parameters.AddWithValue("@avt", imageBytes);
                        }

                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.ExecuteNonQuery();
                    }
                }

                userAvatars[username] = base64;
                LogMessage($"[AVATAR] {username} da cap nhat avatar.");

                string updateMessage = AvatarMessageHelper.BuildUpdateMessage(username, base64);
                BroadcastString(updateMessage);
            }
            catch (Exception ex)
            {
                LogMessage($"[LỖI LƯU AVATAR DATABASE]: {ex.Message}");

                if (ex.Message.Contains("max_allowed_packet"))
                {
                    if (onlineUsers.TryGetValue(username, out TcpClient targetClient))
                    {
                        SendToOne(targetClient, "WARNING|Kích thước ảnh quá lớn! Vui lòng chọn ảnh dung lượng nhỏ hơn.");
                    }
                }

            }
        }

        private void SendStoredAvatarsToClient(TcpClient client)
        {
            foreach (var entry in userAvatars)
            {
                // Chỉ gửi nếu tài khoản đó thực sự có ảnh
                if (!string.IsNullOrEmpty(entry.Value))
                {
                    string updateMessage = AvatarMessageHelper.BuildUpdateMessage(entry.Key, entry.Value);
                    SendToOne(client, updateMessage);

                    Thread.Sleep(100);
                }
            }
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
                lblStatus.Text = $"Số người online: {onlineUsers.Count}";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnStop_Click(this, EventArgs.Empty);
        }


        private void BroadcastBytes(byte[] data, string excludeUsername = "")
        {
            foreach (var user in onlineUsers)
            {
                if (user.Key != excludeUsername)
                {
                    try { user.Value.GetStream().Write(data, 0, data.Length); }
                    catch { }
                }
            }
        }
    }
}
