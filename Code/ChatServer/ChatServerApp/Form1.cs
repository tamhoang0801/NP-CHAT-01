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
                    // XỬ LÝ NHẬN   CHỮ (TEXT) 
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
                                    string requestedName = parts[1];

                                    if (CheckAndAutoRegisterUser(requestedName))
                                    {
                                        if (onlineUsers.TryAdd(requestedName, client))
                                        {
                                            currentUsername = requestedName;

                                            LogMessage($"[ĐĂNG NHẬP] {currentUsername} đã vào phòng.");
                                            UpdateStatusUI();

                                            // Gửi danh sách Online mới nhất cho tất cả Client
                                            string userList = string.Join(",", onlineUsers.Keys);
                                            BroadcastString($"UPDATE_ONLINE|{userList}");
                                            Thread.Sleep(50);
                                            BroadcastString($"BROADCAST|[Hệ thống] {currentUsername} đã vào phòng!");
                                        }
                                        else
                                        {
                                            LogMessage($"[TỪ CHỐI] {currentUsername} đăng nhập trùng tên.");

                                            //Gửi tb 'ERROR'về cho Client
                                            byte[] errorData = Encoding.UTF8.GetBytes("ERROR|Tên đăng nhập đã tồn tại!");
                                            stream.Write(errorData, 0, errorData.Length);
                                            
                                            client.Close();
                                            return;
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
                byte[] data = Encoding.UTF8.GetBytes(rawMessage);
                targetClient.GetStream().Write(data, 0, data.Length);
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
                    // Bắn thêm một câu chót để Client biết đường mà tự cút (Tùy chọn)
                    byte[] endMsg = Encoding.UTF8.GetBytes("BROADCAST|[Hệ thống] Server đã đóng cửa!|");
                    user.Value.GetStream().Write(endMsg, 0, endMsg.Length);

                    // Cắt đứt đường ống
                    user.Value.Close();
                }
                catch
                {
                    // Nếu thằng này lỗi rồi thì lơ đi, lôi đầu thằng tiếp theo ra đóng!
                }
            }
            onlineUsers.Clear();
            try { server?.Stop(); } catch { } // Chống sập lúc Stop Server

            LogMessage("[HỆ THỐNG] Server đã dừng.");
            UpdateStatusUI();

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            txtPort.Enabled = true;
        }

         private bool CheckAndAutoRegisterUser(string username)
         {
         string connectionString = "Server=127.0.0.1; Port=3366; Database=ChatAppDB; Uid=root; Pwd=;";
         
         using (MySqlConnection conn = new MySqlConnection(connectionString))
         {
         try
         {
             conn.Open();

             string checkQuery = "SELECT COUNT(*) FROM TblUsers WHERE Username = @user";
             using (MySqlCommand cmdCheck = new MySqlCommand(checkQuery, conn))
             {
                 cmdCheck.Parameters.AddWithValue("@user", username);
                 int userCount = Convert.ToInt32(cmdCheck.ExecuteScalar());

                 if (userCount > 0)
                 {
                     return true;
                 }
             }

             string insertQuery = "INSERT INTO TblUsers (Username) VALUES (@user)";
             using (MySqlCommand cmdInsert = new MySqlCommand(insertQuery, conn))
             {
                 cmdInsert.Parameters.AddWithValue("@user", username);

                 cmdInsert.ExecuteNonQuery();

                 LogMessage($"[HỆ THỐNG] Đã tự động tạo tài khoản mới cho: {username}");
             }
             
             return true;
         }
         catch (Exception ex)
         {
             LogMessage("[LỖI DATABASE]: " + ex.Message);
             return false;
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

        private void HandleAvatarUpload(NetworkStream stream, string partialData)
        {
            string fullMessage = AvatarMessageHelper.ReadCompleteAvatarMessage(stream, partialData);
            if (!AvatarMessageHelper.TryParse(fullMessage, out string username, out string base64))
                return;

            userAvatars[username] = base64;
            LogMessage($"[AVATAR] {username} da cap nhat avatar.");

            string updateMessage = AvatarMessageHelper.BuildUpdateMessage(username, base64);
            BroadcastString(updateMessage);
        }

        private void SendStoredAvatarsToClient(TcpClient client)
        {
            foreach (var entry in userAvatars)
            {
                string updateMessage = AvatarMessageHelper.BuildUpdateMessage(entry.Key, entry.Value);
                SendToOne(client, updateMessage);
            }
        }
    }
}
