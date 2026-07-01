using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace ChatApp
{
    public partial class FrmChat : Form
    {
        private readonly string _username;
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private readonly Dictionary<string, Image> _avatarCache = new Dictionary<string, Image>();
        private Image _myAvatarImage;
        private readonly Dictionary<string, FrmPrivateChat> _privateChats = new Dictionary<string, FrmPrivateChat>();
        // ==== Reply / Forward ====
        private string _replyToText = null;
        private Panel _pnlReply;
        private Label _lblReply;
        public FrmChat(string username, TcpClient client)
        {
            _username = username;
            _client = client;
            _stream = _client.GetStream();
            InitializeComponent();

            // ==== Reply / Forward: menu chuot phai + thanh tra loi ====
            var replyMenu = new ContextMenuStrip();
            var miReply = new ToolStripMenuItem("Tra loi");
            var miForward = new ToolStripMenuItem("Chuyen tiep");
            miReply.Click += (s, e) => StartReply();
            miForward.Click += (s, e) => ForwardSelected();
            replyMenu.Items.Add(miReply);
            replyMenu.Items.Add(miForward);
            this.rtbChat.ContextMenuStrip = replyMenu;

            _pnlReply = new Panel { Dock = DockStyle.Top, Height = 26, BackColor = System.Drawing.Color.FromArgb(235, 240, 250), Visible = false };
            _lblReply = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) };
            var btnCancelReply = new Button { Text = "X", Dock = DockStyle.Right, Width = 28, FlatStyle = FlatStyle.Flat };
            btnCancelReply.Click += (s, e) => CancelReply();
            _pnlReply.Controls.Add(_lblReply);
            _pnlReply.Controls.Add(btnCancelReply);
            this.pnlInput.Controls.Add(_pnlReply);

            var btnEmoji = new Button
            {
                Text = ":)",
                Width = 40,
                Height = this.txtMessage.Height,
                Font = new System.Drawing.Font("Segoe UI Emoji", 12F),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(224, 224, 224),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            this.pnlInput.Controls.Add(btnEmoji);

            btnEmoji.Location = new System.Drawing.Point(this.btnSend.Left - btnEmoji.Width - 5, this.txtMessage.Top);
            btnEmoji.BringToFront();

            this.txtMessage.Width = btnEmoji.Left - this.txtMessage.Left - 5;

            btnEmoji.Click += (s, e) =>
            {
                var picker = new EmojiPickerForm(this.txtMessage);
                picker.StartPosition = FormStartPosition.Manual;
                picker.Location = this.PointToScreen(new System.Drawing.Point(
                    this.pnlInput.Left + btnEmoji.Left,
                    this.pnlInput.Top + btnEmoji.Top - picker.Height));
                picker.Show(this);
            };

            this.lstOnlineUsers.MouseDoubleClick += LstOnlineUsers_MouseDoubleClick;
            this.lstOnlineUsers.MouseClick += LstOnlineUsers_MouseDoubleClick;
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

            lstOnlineUsers.DrawMode = DrawMode.OwnerDrawFixed;
            lstOnlineUsers.ItemHeight = 36;
            lstOnlineUsers.DrawItem += LstOnlineUsers_DrawItem;

            this.btnChooseAvatar.MouseEnter += (s, e) => this.btnChooseAvatar.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnChooseAvatar.MouseLeave += (s, e) => this.btnChooseAvatar.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

        }

        private void FrmChat_Load(object sender, EventArgs e)
        {
            Text = "Chat Application - " + _username;
            lblCurrentUser.Text = "User: " + _username;

            AppendSystemMessage("Chao mung " + _username + " da ket noi vao phong!");

            InitializeMyDefaultAvatar();

            // Bat luong lang nghe lien tuc
            _receiveThread = new Thread(ReceiveMessage);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }

        private void LstOnlineUsers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                int index = lstOnlineUsers.IndexFromPoint(e.Location);
                if (index < 0 || index >= lstOnlineUsers.Items.Count)
                    index = lstOnlineUsers.SelectedIndex;
                if (index < 0 || index >= lstOnlineUsers.Items.Count) return;

                string item = lstOnlineUsers.Items[index].ToString();
                string partner = item.StartsWith("Online - ") ? item.Substring("Online - ".Length).Trim() : item.Trim();
                if (partner == "" || partner == _username) return;

                OpenPrivateChat(partner);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi mo chat rieng: " + ex.Message);
            }
        }

        private FrmPrivateChat OpenPrivateChat(string partner)
        {
            if (_privateChats.TryGetValue(partner, out var existing) && !existing.IsDisposed)
            {
                if (!existing.Visible) existing.Show(this);
                existing.WindowState = FormWindowState.Normal;
                existing.BringToFront();
                existing.Activate();
                return existing;
            }

            var frm = new FrmPrivateChat(_username, partner, _stream, _myAvatarImage, GetAvatarForUser(partner));
            frm.FormClosed += (s, e) => _privateChats.Remove(partner);
            _privateChats[partner] = frm;

            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Show(this);
            frm.BringToFront();
            frm.Activate();
            return frm;
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
                    if (peekBytes == 0)
                    {
                        HandleDisconnect("Server ngung hoat dong.");
                        break;
                    }

                    byte firstByte = peekBuffer[0];

                    // NEU LA FILE (0x10: Anh, 0x11: Video)
                    if (firstByte == 0x10 || firstByte == 0x11)
                    {
                        // Rut 9 byte phan dau (1 byte loai file + 8 byte kich thuoc)
                        byte[] header = new byte[9];
                        int headerRead = socket.Receive(header, 9, SocketFlags.None);
                        if (headerRead == 0)
                        {
                            HandleDisconnect("Mat ket noi khi dang nhan file.");
                            break;
                        }

                        // Tinh toan dung luong file
                        long dataSize = BitConverter.ToInt64(header, 1);
                        byte[] payload = new byte[dataSize];
                        int totalReceived = 0;

                        // Tai tu tu cho den khi du dung luong
                        while (totalReceived < dataSize)
                        {
                            int read = socket.Receive(payload, totalReceived, (int)(dataSize - totalReceived), SocketFlags.None);
                            if (read == 0) break;
                            totalReceived += read;
                        }

                        // Luu file vao may
                        string type = firstByte == 0x10 ? "Anh" : "Video";
                        string ext = firstByte == 0x10 ? ".jpg" : ".mp4";
                        string fileName = $"Nhan_{type}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                        string savePath = Path.Combine(Application.StartupPath, fileName);
                        File.WriteAllBytes(savePath, payload);

                        AppendSystemMessage($"[He thong] Ban nhan duoc 1 {type}. Da luu tai: {savePath}");
                    }
                    // NHAN FILE RIENG (0x22: Anh, 0x23: Video)
                    else if (firstByte == 0x22 || firstByte == 0x23)
                    {
                        byte[] header = new byte[9];
                        ReadExact(socket, header, 9);
                        long dataSize = BitConverter.ToInt64(header, 1);

                        byte[] lenByte = new byte[1];
                        ReadExact(socket, lenByte, 1);
                        int senderLen = lenByte[0];
                        byte[] senderBytes = new byte[senderLen];
                        ReadExact(socket, senderBytes, senderLen);
                        string sender = Encoding.UTF8.GetString(senderBytes);

                        byte[] payload = new byte[dataSize];
                        ReadExact(socket, payload, (int)dataSize);

                        string type = firstByte == 0x22 ? "Anh" : "Video";
                        string ext = firstByte == 0x22 ? ".jpg" : ".mp4";
                        string fileName = $"Rieng_{type}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                        string savePath = Path.Combine(Application.StartupPath, fileName);
                        File.WriteAllBytes(savePath, payload);

                        this.Invoke(new Action(() =>
                        {
                            var frm = OpenPrivateChat(sender);
                            frm.ReceiveMessage($"[{type}] da nhan, luu tai: {savePath}");
                        }));
                    }
                    // NEU LA CHU (TEXT)
                    else
                    {
                        byte[] buffer = new byte[2048];
                        int bytesRead = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                        if (bytesRead == 0)
                        {
                            HandleDisconnect("Mat ket noi voi Server.");
                            break;
                        }

                        string rawData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        List<string> messages = SplitStickyPackets(rawData);

                        foreach (string msg in messages)
                        {
                            ProcessSingleMessage(socket, msg);
                        }
                    }
                }
                catch
                {
                    if (this.IsDisposed || !this.IsHandleCreated) return;

                    try
                    {
                        this.Invoke(new Action(() =>
                        {
                            if (this.IsDisposed) return;

                            MessageBox.Show("Duong truyen bi dut!.", "Loi mang", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            System.Diagnostics.Process.Start(Application.ExecutablePath);
                            Environment.Exit(0);
                        }));
                    }
                    catch { }

                    return;
                }
            }
        }

        private List<string> SplitStickyPackets(string rawData)
        {
            List<string> messages = new List<string>();
            string[] prefixes = { "UPDATE_ONLINE|", "BROADCAST|", "AVATAR_UPDATE|", "ERROR|", "WARNING|", "PRIVATE|", "RMSG|", "FMSG|" };
            int currentIndex = 0;
            while (currentIndex < rawData.Length)
            {
                int nextPrefixIndex = -1;

                foreach (string prefix in prefixes)
                {
                    int index = rawData.IndexOf(prefix, currentIndex + 1);
                    if (index > 0 && (nextPrefixIndex == -1 || index < nextPrefixIndex))
                    {
                        nextPrefixIndex = index;
                    }
                }

                if (nextPrefixIndex != -1)
                {
                    string msg = rawData.Substring(currentIndex, nextPrefixIndex - currentIndex);
                    messages.Add(msg);
                    currentIndex = nextPrefixIndex;
                }
                else
                {
                    string msg = rawData.Substring(currentIndex);
                    messages.Add(msg);
                    break;
                }
            }

            return messages;
        }

        private void ProcessSingleMessage(Socket socket, string msg)
        {
            if (msg.StartsWith(AvatarHelper.AvatarUpdateCommand + "|"))
            {
                string fullAvatarMessage = AvatarHelper.ReadCompleteAvatarMessage(socket, msg);
                if (AvatarHelper.TryParseAvatarMessage(fullAvatarMessage, out string avatarUser, out string avatarBase64))
                    ApplyAvatarUpdate(avatarUser, avatarBase64);
                return;
            }

            string[] tokens = msg.Split('|');
            if (tokens.Length == 0) return;
            string command = tokens[0];

            if (command == "BROADCAST" && tokens.Length > 1)
            {
                string messageContent = tokens[1];
                string time = DateTime.Now.ToString("HH:mm");

                if (messageContent.StartsWith("[He thong]"))
                {
                    AppendSystemMessage(messageContent);
                    if (messageContent.Contains("dong cua"))
                    {
                        this.Invoke(new Action(() =>
                        {
                            System.Diagnostics.Process.Start(Application.ExecutablePath);
                            Environment.Exit(0);
                        }));
                        return;
                    }
                }
                else
                {
                    if (messageContent.Contains("dong cua"))
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Server da dong cua!.", "Mat ket noi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            Application.Restart();
                            Environment.Exit(0);
                        }));
                        return;
                    }

                    int colonIndex = messageContent.IndexOf(':');

                    if (colonIndex > 0)
                    {
                        if (messageContent.Contains(": "))
                        {
                            string senderName = messageContent.Substring(0, colonIndex).Trim();
                            string actualContent = messageContent.Substring(colonIndex + 1).Trim();

                            AppendOtherMessage(senderName, actualContent, time);
                        }
                    }
                }
            }
            else if (command == "UPDATE_ONLINE" && tokens.Length > 1)
            {
                string[] users = tokens[1].Split(',');
                UpdateOnlineUsers(users);
            }
            else if (command == "PRIVATE" && tokens.Length >= 3)
            {
                string pSender = tokens[1];
                string pContent = tokens[2];
                this.Invoke(new Action(() =>
                {
                    var frm = OpenPrivateChat(pSender);
                    frm.ReceiveMessage(pContent);
                }));
            }
            else if (command == "ERROR" && tokens.Length > 1)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(tokens[1], "Loi dang nhap", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Diagnostics.Process.Start(Application.ExecutablePath);
                    Environment.Exit(0);
                }));
            }
            else if (command == "WARNING" && tokens.Length > 1)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(tokens[1], "Thong bao he thong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }));
            }
            else if (command == "RMSG" && tokens.Length >= 4)
            {
                string sender = tokens[1];
                string quoted = Unesc(tokens[2]);
                string content = Unesc(tokens[3]);
                string time = DateTime.Now.ToString("HH:mm");
                AppendReplyMessage(sender, quoted, content, time, false);
            }
            else if (command == "FMSG" && tokens.Length >= 3)
            {
                string sender = tokens[1];
                string content = Unesc(tokens[2]);
                string time = DateTime.Now.ToString("HH:mm");
                AppendForwardMessage(sender, content, time, false);
            }
        }

        private void HandleDisconnect(string reason)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return; // Neu form da dong tu truoc thi bo qua

            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate { HandleDisconnect(reason); }));
                return;
            }

            MessageBox.Show(reason, "Loi ket noi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }

        private void SendTextMessage()
        {
            string content = txtMessage.Text.Trim();
            if (content == "") return;
            string time = DateTime.Now.ToString("HH:mm");

            try
            {
                if (_replyToText != null)
                {
                    AppendReplyMessage(_username, _replyToText, content, time, true);
                    string msg = $"REPLY|{_username}|{Esc(_replyToText)}|{Esc(content)}";
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    _stream.Write(data, 0, data.Length);
                    CancelReply();
                }
                else
                {
                    AppendMyMessage(_username, content, time);
                    string msg = $"MSG|{_username}|{content}";
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    _stream.Write(data, 0, data.Length);
                }
            }
            catch { MessageBox.Show("Loi gui tin nhan!"); }

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
                        // Doc file thanh Byte
                        byte[] fileData = File.ReadAllBytes(ofd.FileName);

                        // 1 byte ma loai + 8 byte kich thuoc
                        byte[] header = new byte[9];
                        header[0] = typeCode;
                        byte[] sizeBytes = BitConverter.GetBytes((long)fileData.Length);
                        Array.Copy(sizeBytes, 0, header, 1, 8);

                        // Gui len Server
                        _stream.Write(header, 0, 9);
                        _stream.Write(fileData, 0, fileData.Length);

                        string typeStr = typeCode == 0x10 ? "Anh" : "Video";
                        string time = DateTime.Now.ToString("HH:mm");
                        AppendMyMessage(_username, $"[Ban da gui 1 tep {typeStr}]", time);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Loi khi gui file: " + ex.Message);
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

        private void ReadExact(System.Net.Sockets.Socket socket, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int r = socket.Receive(buffer, total, count - total, System.Net.Sockets.SocketFlags.None);
                if (r == 0) throw new Exception("Mat ket noi khi nhan file rieng");
                total += r;
            }
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
            {
                string cleanUser = user.Trim();

                if (!string.IsNullOrEmpty(cleanUser))
                {
                    if (cleanUser == _username) continue;
                    lstOnlineUsers.Items.Add("Online - " + cleanUser);
                }
            }
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

        // ================== REPLY / FORWARD HELPERS ==================
        private static string Esc(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("|", "\\p").Replace("\r", "").Replace("\n", "\\n");
        }

        private static string Unesc(string s)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    char n = s[++i];
                    if (n == 'p') sb.Append('|');
                    else if (n == 'n') sb.Append('\n');
                    else if (n == '\\') sb.Append('\\');
                    else sb.Append(n);
                }
                else sb.Append(s[i]);
            }
            return sb.ToString();
        }

        private void StartReply()
        {
            string sel = rtbChat.SelectedText.Trim();
            if (string.IsNullOrEmpty(sel))
            { MessageBox.Show("Hay boi den tin nhan muon tra loi."); return; }
            _replyToText = sel;
            _lblReply.Text = "Dang tra loi: " + (sel.Length > 60 ? sel.Substring(0, 60) + "..." : sel);
            _pnlReply.Visible = true;
            txtMessage.Focus();
        }

        private void CancelReply()
        {
            _replyToText = null;
            _pnlReply.Visible = false;
            _lblReply.Text = "";
        }

        private void ForwardSelected()
        {
            string content = rtbChat.SelectedText.Trim();
            if (string.IsNullOrEmpty(content))
            { MessageBox.Show("Hay boi den noi dung can chuyen tiep."); return; }
            string time = DateTime.Now.ToString("HH:mm");
            AppendForwardMessage(_username, content, time, true);
            try
            {
                string msg = $"FWD|{_username}|{Esc(content)}";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                _stream.Write(data, 0, data.Length);
            }
            catch { MessageBox.Show("Loi khi chuyen tiep!"); }
        }

        public void AppendReplyMessage(string name, string quoted, string content, string time, bool isMine)
        {
            if (rtbChat.InvokeRequired)
            { rtbChat.Invoke(new Action(() => AppendReplyMessage(name, quoted, content, time, isMine))); return; }
            string shortQuote = quoted.Replace("\n", " ");
            if (shortQuote.Length > 70) shortQuote = shortQuote.Substring(0, 70) + "...";
            AppendColoredText("> " + shortQuote + "\n", Color.FromArgb(150, 150, 150), italic: true);
            Color nameColor = isMine ? Color.FromArgb(40, 100, 180) : Color.FromArgb(160, 90, 30);
            AppendColoredText("[" + time + "] ", Color.FromArgb(150, 150, 150));
            AppendColoredText(name + ": ", nameColor, bold: true);
            AppendColoredText(content + "\n", Color.FromArgb(50, 50, 50));
            rtbChat.ScrollToCaret();
        }

        public void AppendForwardMessage(string name, string content, string time, bool isMine)
        {
            if (rtbChat.InvokeRequired)
            { rtbChat.Invoke(new Action(() => AppendForwardMessage(name, content, time, isMine))); return; }
            AppendColoredText(">> Tin da chuyen tiep\n", Color.FromArgb(120, 120, 120), italic: true);
            Color c = isMine ? Color.FromArgb(40, 100, 180) : Color.FromArgb(160, 90, 30);
            AppendColoredText("[" + time + "] ", Color.FromArgb(150, 150, 150));
            AppendColoredText(name + ": ", c, bold: true);
            AppendColoredText(content + "\n", Color.FromArgb(50, 50, 50));
            rtbChat.ScrollToCaret();
        }
        // ============================================================

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
            FontStyle style = FontStyle.Regular;
            if (bold) style |= FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;

            Font textFont = new Font(rtbChat.Font, style);
            Font emojiFont = new Font("Segoe UI Emoji", rtbChat.Font.Size, FontStyle.Regular);

            var buffer = new StringBuilder();
            bool bufferIsEmoji = false;

            void Flush()
            {
                if (buffer.Length == 0) return;
                rtbChat.SelectionStart = rtbChat.TextLength;
                rtbChat.SelectionLength = 0;
                rtbChat.SelectionColor = color;
                rtbChat.SelectionFont = bufferIsEmoji ? emojiFont : textFont;
                rtbChat.AppendText(buffer.ToString());
                buffer.Clear();
            }

            int i = 0;
            while (i < text.Length)
            {
                int charLen = char.IsSurrogatePair(text, i) ? 2 : 1;
                int cp = charLen == 2 ? char.ConvertToUtf32(text, i) : text[i];
                bool isEmoji = IsEmojiCodePoint(cp);

                if (buffer.Length > 0 && isEmoji != bufferIsEmoji)
                    Flush();

                bufferIsEmoji = isEmoji;
                buffer.Append(text, i, charLen);
                i += charLen;
            }
            Flush();

            rtbChat.SelectionColor = rtbChat.ForeColor;
        }

        // Kiem tra 1 ky tu co phai emoji khong
        private static bool IsEmojiCodePoint(int cp)
        {
            return
                cp == 0x200D ||
                cp == 0xFE0F || cp == 0xFE0E ||
                (cp >= 0x1F000 && cp <= 0x1FAFF) ||
                (cp >= 0x1F1E6 && cp <= 0x1F1FF) ||
                (cp >= 0x2600 && cp <= 0x27BF) ||
                (cp >= 0x2B00 && cp <= 0x2BFF) ||
                (cp >= 0x2300 && cp <= 0x23FF) ||
                (cp >= 0x2190 && cp <= 0x21FF) ||
                (cp >= 0x25A0 && cp <= 0x25FF) ||
                cp == 0x203C || cp == 0x2049 ||
                cp == 0x24C2;
        }

        private void InitializeMyDefaultAvatar()
        {
            ApplyAvatarUpdate(_username, null);
        }

        private void btnChooseAvatar_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "JPEG Files|*.jpg",
                Title = "Chon avatar (.jpg)"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            if (!AvatarHelper.TryValidateAvatarFile(ofd.FileName, out string validationError))
            {
                MessageBox.Show(validationError, "Loi avatar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string base64 = AvatarHelper.ImageFileToBase64(ofd.FileName);
                string msg = AvatarHelper.BuildAvatarUploadMessage(_username, base64);
                byte[] data = Encoding.UTF8.GetBytes(msg);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi khi chon avatar: " + ex.Message);
            }
        }

        private void ApplyAvatarUpdate(string username, string base64)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ApplyAvatarUpdate(username, base64)));
                return;
            }

            try
            {
                Image avatarImage = string.IsNullOrEmpty(base64)
                    ? AvatarHelper.GetDefaultAvatar(username, 64)
                    : AvatarHelper.Base64ToImage(base64);

                SetUserAvatarCache(username, avatarImage);

                if (username == _username)
                {
                    _myAvatarImage?.Dispose();
                    _myAvatarImage = avatarImage;
                    Image oldHeaderAvatar = picAvatar.Image;
                    picAvatar.Image = AvatarHelper.ToThumbnail(_myAvatarImage, picAvatar.Width - 12);
                    oldHeaderAvatar?.Dispose();
                }

                lstOnlineUsers.Invalidate();
            }
            catch { }
        }

        private void SetUserAvatarCache(string username, Image avatarImage)
        {
            if (_avatarCache.TryGetValue(username, out Image oldImage))
            {
                if (!ReferenceEquals(oldImage, _myAvatarImage))
                    oldImage.Dispose();
            }

            _avatarCache[username] = avatarImage;
        }

        private Image GetAvatarForUser(string username)
        {
            if (_avatarCache.TryGetValue(username, out Image cached))
                return cached;

            Image defaultAvatar = AvatarHelper.GetDefaultAvatar(username, 28);
            _avatarCache[username] = defaultAvatar;
            return defaultAvatar;
        }

        private void LstOnlineUsers_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            e.DrawBackground();

            string itemText = lstOnlineUsers.Items[e.Index]?.ToString() ?? "";
            string username = itemText.StartsWith("Online - ")
                ? itemText.Substring("Online - ".Length)
                : itemText;

            Image avatar = GetAvatarForUser(username);

            // Ve avatar hinh tron
            int avatarSize = 28;
            int avatarX = e.Bounds.Left + 6;
            int avatarY = e.Bounds.Top + 4;
            using (Bitmap circleAvatar = AvatarHelper.MakeCircular(avatar, avatarSize))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawImage(circleAvatar, avatarX, avatarY, avatarSize, avatarSize);
            }

            using SolidBrush brush = new SolidBrush(e.ForeColor);
            e.Graphics.DrawString(itemText, e.Font, brush, e.Bounds.Left + 40, e.Bounds.Top + 8);

            e.DrawFocusRectangle();
        }
    }
}
