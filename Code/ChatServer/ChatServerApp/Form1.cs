using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatServerApp
{
    public partial class Form1 : Form
    {
        private TcpListener server;
        private Thread listenThread;
        private bool isRunning = false;
        private ConcurrentDictionary<string, TcpClient> onlineUsers = new ConcurrentDictionary<string, TcpClient>();
        private ConcurrentDictionary<string, string> userAvatars = new ConcurrentDictionary<string, string>(); // tên_người_dùng -> tên_file_avatar
        private string avatarFolder;

        public Form1()
        {
            InitializeComponent();
            // Tạo thư mục lưu avatar trên server
            avatarFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ServerAvatars");
            if (!System.IO.Directory.Exists(avatarFolder))
            {
                System.IO.Directory.CreateDirectory(avatarFolder);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(txtPort.Text);
                server = new TcpListener(IPAddress.Any, port);
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
            }
        }

        private void ListenForClients()
        {
            try
            {
                server.Start();
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
            Socket socket = client.Client; // Lấy Socket gốc để dùng tính năng Peek (Nhìn lén)
            string currentUsername = "";

            try
            {
                while (isRunning)
                {
                    // 1. NHÌN LÉN 1 BYTE ĐẦU TIÊN (Để biết là Gửi File hay Gửi Chữ)
                    byte[] peekBuffer = new byte[1];
                    int peekBytes = socket.Receive(peekBuffer, 1, SocketFlags.Peek);
                    if (peekBytes == 0) break; // Client ngắt kết nối

                    byte firstByte = peekBuffer[0];

                    // --- LUỒNG 1: XỬ LÝ NHẬN FILE (0x10: Ảnh, 0x11: Video) ---
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
                    // --- LUỒNG 3: XỬ LÝ NHẬN AVATAR (0x12) ---
                    else if (firstByte == 0x12)
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
                            if (read == 0) throw new Exception("Mất kết nối khi nhận avatar");
                            totalReceived += read;
                        }

                        // Lưu avatar
                        if (!string.IsNullOrEmpty(currentUsername))
                        {
                            string avatarFile = System.IO.Path.Combine(avatarFolder, $"{currentUsername}.jpg");
                            System.IO.File.WriteAllBytes(avatarFile, payload);
                            userAvatars.AddOrUpdate(currentUsername, $"{currentUsername}.jpg", (k, v) => $"{currentUsername}.jpg");
                            LogMessage($"[AVATAR] {currentUsername} đã cập nhật avatar.");
                        }
                    }
                    // --- LUỒNG 2: XỬ LÝ NHẬN CHỮ (TEXT) ---
                    else
                    {
                        byte[] buffer = new byte[2048];
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
                                    currentUsername = parts[1];
                                    if (onlineUsers.TryAdd(currentUsername, client))
                                    {
                                        LogMessage($"[ĐĂNG NHẬP] {currentUsername} đã vào phòng.");
                                        UpdateStatusUI();

                                        // Send existing online users + avatars to this new client
                                        SendExistingUsersToClient(client);

                                        // Gửi danh sách Online mới nhất cho tất cả Client
                                        string userList = string.Join(",", onlineUsers.Keys);
                                        BroadcastString($"UPDATE_ONLINE|{userList}");

                                        // Broadcast thông báo USER_ONLINE
                                        BroadcastString($"USER_ONLINE|{currentUsername}");

                                        BroadcastString($"BROADCAST|[Hệ thống] {currentUsername} đã vào phòng!");
                                    }
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
                    userAvatars.TryRemove(currentUsername, out _);
                    LogMessage($"[NGẮT KẾT NỐI] {currentUsername} đã rời đi.");

                    // Broadcast USER_OFFLINE notification
                    string userList = string.Join(",", onlineUsers.Keys);
                    BroadcastString($"UPDATE_ONLINE|{userList}");
                    BroadcastString($"USER_OFFLINE|{currentUsername}");

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
                byte[] data = Encoding.UTF8.GetBytes(rawMessage);
                targetClient.GetStream().Write(data, 0, data.Length);
            }
            catch { }
        }

        // Gửi danh sách người dùng hiện tại và avatar của họ cho client mới kết nối
        private void SendExistingUsersToClient(TcpClient newClient)
        {
            try
            {
                NetworkStream stream = newClient.GetStream();
                foreach (var user in onlineUsers)
                {
                    string username = user.Key;
                    string message = $"USER_ONLINE|{username}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    // Gửi avatar nếu có
                    if (userAvatars.TryGetValue(username, out string avatarFile))
                    {
                        string avatarPath = System.IO.Path.Combine(avatarFolder, avatarFile);
                        if (System.IO.File.Exists(avatarPath))
                        {
                            byte[] avatarData = System.IO.File.ReadAllBytes(avatarPath);
                            byte[] header = new byte[9];
                            header[0] = 0x12; // Mã avatar
                            byte[] sizeBytes = BitConverter.GetBytes((long)avatarData.Length);
                            Array.Copy(sizeBytes, 0, header, 1, 8);

                            stream.Write(header, 0, 9);
                            stream.Write(avatarData, 0, avatarData.Length);
                        }
                    }
                }
            }
            catch { }
        }

        private void BroadcastString(string rawMessage, string excludeUsername = "")
        {
            byte[] data = Encoding.UTF8.GetBytes(rawMessage);
            foreach (var user in onlineUsers)
            {
                if (user.Key != excludeUsername)
                {
                    try { user.Value.GetStream().Write(data, 0, data.Length); }
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
                user.Value.Close();
            }
            onlineUsers.Clear();
            server?.Stop();

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