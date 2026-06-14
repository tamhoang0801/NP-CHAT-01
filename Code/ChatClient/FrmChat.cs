using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class    FrmChat : Form
    {
        private readonly string _username;
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;

        public FrmChat(string username, TcpClient client)
        {
            _username = username;
            _client = client;
            _stream = _client.GetStream();
            InitializeComponent();

            this.pnlTop.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(204, 204, 204), 1),
                    0, this.pnlTop.Height - 1, this.pnlTop.Width, this.pnlTop.Height - 1);
            };

            this.btnLogout.MouseEnter += (s, e) => this.btnLogout.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnLogout.MouseLeave += (s, e) => this.btnLogout.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.pnlLeft.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(204, 204, 204), 1),
                    this.pnlLeft.Width - 1, 0, this.pnlLeft.Width - 1, this.pnlLeft.Height);
            };

            this.pnlFileBar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(204, 204, 204), 1),
                    0, 0, this.pnlFileBar.Width, 0);
            };

            this.btnSendImage.MouseEnter += (s, e) => this.btnSendImage.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnSendImage.MouseLeave += (s, e) => this.btnSendImage.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.btnSendVideo.MouseEnter += (s, e) => this.btnSendVideo.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnSendVideo.MouseLeave += (s, e) => this.btnSendVideo.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.pnlInput.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(204, 204, 204), 1),
                    0, 0, this.pnlInput.Width, 0);
            };

            this.btnSend.MouseEnter += (s, e) => this.btnSend.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnSend.MouseLeave += (s, e) => this.btnSend.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.FormClosed += (sender, e) => Environment.Exit(0);

        }

        private void FrmChat_Load(object sender, EventArgs e)
        {
            Text = "Chat Application - " + _username;
            lblCurrentUser.Text = "User: " + _username;

            AppendSystemMessage("Chào mừng " + _username + " đã kết nối vào phòng!");

            // Bật luồng lắng nghe liên tục
            _receiveThread = new Thread(ReceiveMessage);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }


        private void ReceiveMessage()
        {
            Socket socket = _client.Client;
            while (_stream != null)
            {
                try
                {

                    byte[] peekBuffer = new byte[1];
                    int peekBytes = socket.Receive(peekBuffer, 1, SocketFlags.Peek);
                    if (peekBytes == 0) break;

                    byte firstByte = peekBuffer[0];

                    // NẾU LÀ FILE (0x10: Ảnh, 0x11: Video)
                    if (firstByte == 0x10 || firstByte == 0x11)
                    {
                        // Rút 9 byte phần đầu (1 byte loại file + 8 byte kích thước)
                        byte[] header = new byte[9];
                        int headerRead = socket.Receive(header, 9, SocketFlags.None);
                        if (headerRead == 0) break;

                        // Tính toán dung lượng file
                        long dataSize = BitConverter.ToInt64(header, 1);
                        byte[] payload = new byte[dataSize];
                        int totalReceived = 0;

                        // Tải từ từ cho đến khi đủ dung lượng
                        while (totalReceived < dataSize)
                        {
                            int read = socket.Receive(payload, totalReceived, (int)(dataSize - totalReceived), SocketFlags.None);
                            if (read == 0) break;
                            totalReceived += read;
                        }

                        // Lưu file vào máy
                        string type = firstByte == 0x10 ? "Ảnh" : "Video";
                        string ext = firstByte == 0x10 ? ".jpg" : ".mp4";
                        string fileName = $"Nhan_{type}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                        string savePath = Path.Combine(Application.StartupPath, fileName);
                        File.WriteAllBytes(savePath, payload);

                        AppendSystemMessage($"[Hệ thống] Bạn nhận được 1 {type}. Đã lưu tại: {savePath}");
                    }

                    // NẾU LÀ CHỮ (TEXT)
                    else
                    {
                        byte[] buffer = new byte[2048];
                        int bytesRead = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                        if (bytesRead == 0) break;

                        string rawData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        string[] tokens = rawData.Split('|');
                        string command = tokens[0];

                        if (command == "BROADCAST" && tokens.Length > 1)
                        {
                            string messageContent = tokens[1];
                            string time = DateTime.Now.ToString("HH:mm");

                            // Nếu là tin nhắn của hệ thống
                            if (messageContent.StartsWith("[Hệ thống]"))
                            {
                                AppendSystemMessage(messageContent);
                            }
                            else
                            {
                                string senderName = "Người khác";

                                if (messageContent.Contains(": "))
                                {
                                    int colonIndex = messageContent.IndexOf(": ");

                                    senderName = messageContent.Substring(0, colonIndex);


                                    messageContent = messageContent.Substring(colonIndex + 2);
                                }

                                AppendOtherMessage(senderName, messageContent, time);
                            }
                        }
                        else if (command == "UPDATE_ONLINE" && tokens.Length > 1)
                        {
                            string[] users = tokens[1].Split(',');
                            UpdateOnlineUsers(users);
                        }
                    }
                }
                catch { break; }
            }
        }


        private void SendTextMessage()
        {
            string content = txtMessage.Text.Trim();
            if (content == "") return;

            string time = DateTime.Now.ToString("HH:mm");
            AppendMyMessage(_username, content, time);

            try
            {
                string msg = $"MSG|{_username}|{content}";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                _stream.Write(data, 0, data.Length);
            }
            catch { MessageBox.Show("Lỗi gửi tin nhắn!"); }

            txtMessage.Clear();
            txtMessage.Focus();
        }

        private void btnSend_Click(object sender, EventArgs e) => SendTextMessage();

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendTextMessage();
            }
        }


        private void SendFile(byte typeCode, string filter)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = filter })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Đọc file thành Byte
                        byte[] fileData = File.ReadAllBytes(ofd.FileName);

                        // 1 byte mã loại + 8 byte kích thước
                        byte[] header = new byte[9];
                        header[0] = typeCode;
                        byte[] sizeBytes = BitConverter.GetBytes((long)fileData.Length);
                        Array.Copy(sizeBytes, 0, header, 1, 8);

                        // Bắn lên Server
                        _stream.Write(header, 0, 9);
                        _stream.Write(fileData, 0, fileData.Length);

                        string typeStr = typeCode == 0x10 ? "Ảnh" : "Video";
                        string time = DateTime.Now.ToString("HH:mm");
                        AppendMyMessage(_username, $"[Bạn đã gửi 1 tệp {typeStr}]", time);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi gửi file: " + ex.Message);
                    }
                }
            }
        }

        private void btnSendImage_Click(object sender, EventArgs e)
        {
            SendFile(0x10, "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp");
        }

        private void btnSendVideo_Click(object sender, EventArgs e)
        {
            SendFile(0x11, "Video Files|*.mp4;*.avi;*.mkv");
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                string msg = $"LOGOUT|{_username}";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                _stream.Write(data, 0, data.Length);
            }
            catch { }
            Environment.Exit(0);
        }


        public void UpdateOnlineUsers(string[] onlineList)
        {
            if (lstOnlineUsers.InvokeRequired)
            {
                lstOnlineUsers.Invoke(new Action(() => UpdateOnlineUsers(onlineList)));
                return;
            }
            lstOnlineUsers.Items.Clear();
            foreach (string user in onlineList)
                lstOnlineUsers.Items.Add("Online - " + user);
            lblOnlineCount.Text = "Online: " + onlineList.Length;
        }

        public void AppendMyMessage(string name, string content, string time)
        {
            if (rtbChat.InvokeRequired)
            {
                rtbChat.Invoke(new Action(() => AppendMyMessage(name, content, time)));
                return;
            }
            AppendMessage(time, name, content, Color.FromArgb(40, 100, 180));
        }

        public void AppendOtherMessage(string name, string content, string time)
        {
            if (rtbChat.InvokeRequired)
            {
                rtbChat.Invoke(new Action(() => AppendOtherMessage(name, content, time)));
                return;
            }
            AppendMessage(time, name, content, Color.FromArgb(160, 90, 30));
        }

        public void AppendSystemMessage(string message)
        {
            if (rtbChat.InvokeRequired)
            {
                rtbChat.Invoke(new Action(() => AppendSystemMessage(message)));
                return;
            }
            AppendColoredText(message + "\n", Color.FromArgb(130, 130, 130), italic: true);
            rtbChat.ScrollToCaret();
        }

        private void AppendMessage(string time, string name, string content, Color nameColor)
        {
            AppendColoredText("[" + time + "] ", Color.FromArgb(150, 150, 150));
            AppendColoredText(name + ": ", nameColor, bold: true);
            AppendColoredText(content + "\n", Color.FromArgb(50, 50, 50));
            rtbChat.ScrollToCaret();
        }

        private void AppendColoredText(string text, Color color, bool bold = false, bool italic = false)
        {
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.SelectionColor = color;
            FontStyle style = FontStyle.Regular;
            if (bold) style |= FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;
            rtbChat.SelectionFont = new Font(rtbChat.Font, style);
            rtbChat.AppendText(text);
            rtbChat.SelectionColor = rtbChat.ForeColor;
        }
    }
}