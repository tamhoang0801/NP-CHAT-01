using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class FrmChat : Form
    {
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB limit for received files
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".avi", ".mkv" };
        private readonly string _username;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private Thread? _receiveThread;
        private readonly ChatManager _chatManager;
        private readonly OnlineUserManager _onlineUserManager = new OnlineUserManager();
        private readonly ToolTip _userToolTip = new ToolTip();
        private int _autoSaveCounter = 0;
        private const int AutoSaveInterval = 10;
        // Cached GDI objects to prevent handle leak
        private Font? _cachedFontBold;
        private Font? _cachedFontRegular;
        private Font? _cachedFontBoldItalic;
        private Font? _cachedFontUnderline;

        private Font GetFontBold()
        {
            if (_cachedFontBold == null || _cachedFontBold.FontFamily != rtbChat.Font.FontFamily || _cachedFontBold.Size != rtbChat.Font.Size)
            {
                _cachedFontBold?.Dispose();
                _cachedFontBold = new Font(rtbChat.Font, FontStyle.Bold);
            }
            return _cachedFontBold;
        }

        private Font GetFontItalic()
        {
            if (_cachedFontRegular == null || _cachedFontRegular.FontFamily != rtbChat.Font.FontFamily || _cachedFontRegular.Size != rtbChat.Font.Size)
            {
                _cachedFontRegular?.Dispose();
                _cachedFontRegular = new Font(rtbChat.Font, FontStyle.Italic);
            }
            return _cachedFontRegular;
        }

        private Font GetFontBoldItalic()
        {
            if (_cachedFontBoldItalic == null || _cachedFontBoldItalic.FontFamily != rtbChat.Font.FontFamily || _cachedFontBoldItalic.Size != rtbChat.Font.Size)
            {
                _cachedFontBoldItalic?.Dispose();
                _cachedFontBoldItalic = new Font(rtbChat.Font, FontStyle.Bold | FontStyle.Italic);
            }
            return _cachedFontBoldItalic;
        }

        private Font GetFontUnderline()
        {
            if (_cachedFontUnderline == null || _cachedFontUnderline.FontFamily != rtbChat.Font.FontFamily || _cachedFontUnderline.Size != rtbChat.Font.Size)
            {
                _cachedFontUnderline?.Dispose();
                _cachedFontUnderline = new Font(rtbChat.Font, FontStyle.Underline);
            }
            return _cachedFontUnderline;
        }

        public FrmChat(string username, TcpClient client)
        {
            _username = username;
            _client = client;
            _stream = _client.GetStream();
            try { _client.ReceiveTimeout = 0; _client.SendTimeout = 0; } catch { }
            _chatManager = new ChatManager();
            _chatManager.SetUsername(_username);
            InitializeComponent();

            this.pnlTop.Paint += (s, e) =>
            {
                using (var p = new Pen(Color.FromArgb(204, 204, 204), 1))
                    e.Graphics.DrawLine(p, 0, this.pnlTop.Height - 1, this.pnlTop.Width, this.pnlTop.Height - 1);
            };

            this.btnLogout.MouseEnter += (s, e) => this.btnLogout.BackColor = Color.FromArgb(210, 210, 210);
            this.btnLogout.MouseLeave += (s, e) => this.btnLogout.BackColor = Color.FromArgb(224, 224, 224);

            this.pnlLeft.Paint += (s, e) =>
            {
                using (var p = new Pen(Color.FromArgb(204, 204, 204), 1))
                    e.Graphics.DrawLine(p, this.pnlLeft.Width - 1, 0, this.pnlLeft.Width - 1, this.pnlLeft.Height);
            };

            this.pnlFileBar.Paint += (s, e) =>
            {
                using (var p = new Pen(Color.FromArgb(204, 204, 204), 1))
                    e.Graphics.DrawLine(p, 0, 0, this.pnlFileBar.Width, 0);
            };

            this.btnSendImage.MouseEnter += (s, e) => this.btnSendImage.BackColor = Color.FromArgb(210, 210, 210);
            this.btnSendImage.MouseLeave += (s, e) => this.btnSendImage.BackColor = Color.FromArgb(224, 224, 224);

            this.btnSendVideo.MouseEnter += (s, e) => this.btnSendVideo.BackColor = Color.FromArgb(210, 210, 210);
            this.btnSendVideo.MouseLeave += (s, e) => this.btnSendVideo.BackColor = Color.FromArgb(224, 224, 224);

            this.pnlInput.Paint += (s, e) =>
            {
                using (var p = new Pen(Color.FromArgb(204, 204, 204), 1))
                    e.Graphics.DrawLine(p, 0, 0, this.pnlInput.Width, 0);
            };

            this.btnSend.MouseEnter += (s, e) => this.btnSend.BackColor = Color.FromArgb(210, 210, 210);
            this.btnSend.MouseLeave += (s, e) => this.btnSend.BackColor = Color.FromArgb(224, 224, 224);

            this.FormClosing += FrmChat_FormClosing;

            // Wire ChatManager events
            _chatManager.SessionListChanged += OnSessionListChanged;
            _chatManager.ActiveSessionChanged += OnActiveSessionChanged;

            // Wire emoji button
            this.btnEmoji.Click += btnEmoji_Click;

            // Wire session list selection
            this.lstChatSessions.SelectedIndexChanged += lstChatSessions_SelectedIndexChanged;

            // Wire double-click on online users list to open private chat
            this.lstOnlineUsers.MouseDoubleClick += lstOnlineUsers_MouseDoubleClick;
            // Wire mouse move for tooltip showing user info
            this.lstOnlineUsers.MouseMove += lstOnlineUsers_MouseMove;

            // Wire link click for image preview
            this.rtbChat.LinkClicked += RtbChat_LinkClicked;

            // Enable owner-draw for session list to highlight unread sessions
            this.lstChatSessions.DrawMode = DrawMode.OwnerDrawFixed;
            this.lstChatSessions.DrawItem += lstChatSessions_DrawItem;
        }

        /// <summary>
        /// Custom draw for session list: highlight unread sessions with bold + color.
        /// </summary>
        private void lstChatSessions_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _chatManager.Sessions.Count) return;

            e.DrawBackground();

            var session = _chatManager.Sessions[e.Index];
            string text = lstChatSessions.Items[e.Index].ToString() ?? session.Name;

            bool isActive = session.Id == _chatManager.ActiveSession?.Id;
            bool hasUnread = session.UnreadCount > 0;

            // Background: active = light blue, unread = very light yellow
            if (isActive)
            {
                using (var b = new SolidBrush(Color.FromArgb(220, 235, 255)))
                    e.Graphics.FillRectangle(b, e.Bounds);
            }
            else if (hasUnread)
            {
                using (var b = new SolidBrush(Color.FromArgb(255, 250, 220)))
                    e.Graphics.FillRectangle(b, e.Bounds);
            }

            // Text: unread = bold + darker, active = slightly darker, normal = default
            Color textColor;
            Font drawFont;

            if (hasUnread)
            {
                textColor = Color.FromArgb(180, 60, 0);
                drawFont = new Font(lstChatSessions.Font, FontStyle.Bold);
            }
            else if (isActive)
            {
                textColor = Color.FromArgb(0, 60, 140);
                drawFont = new Font(lstChatSessions.Font, FontStyle.Regular);
            }
            else
            {
                textColor = lstChatSessions.ForeColor;
                drawFont = lstChatSessions.Font;
            }

            // Draw unread badge
            if (hasUnread && session.UnreadCount > 0)
            {
                string badge = $" {session.UnreadCount} ";
                SizeF badgeSize = e.Graphics.MeasureString(badge, drawFont);
                float badgeX = e.Bounds.Right - badgeSize.Width - 8;
                float badgeY = e.Bounds.Y + (e.Bounds.Height - badgeSize.Height) / 2;

                using (var b = new SolidBrush(Color.FromArgb(220, 60, 60)))
                {
                    RectangleF badgeRect = new RectangleF(badgeX, badgeY, badgeSize.Width, badgeSize.Height);
                    e.Graphics.FillEllipse(b, badgeRect);
                }
                e.Graphics.DrawString(badge, drawFont, Brushes.White, badgeX, badgeY);

                // Draw text with padding for badge
                float textMaxWidth = badgeX - e.Bounds.X - 4;
                if (textMaxWidth > 0)
                {
                    using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter })
                    using (var b = new SolidBrush(textColor))
                        e.Graphics.DrawString(text, drawFont, b,
                            new RectangleF(e.Bounds.X + 2, e.Bounds.Y, textMaxWidth, e.Bounds.Height), sf);
                }
            }
            else
            {
                using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter })
                using (var b = new SolidBrush(textColor))
                    e.Graphics.DrawString(text, drawFont, b,
                        new RectangleF(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height), sf);
            }

            if (drawFont != lstChatSessions.Font)
            {
                drawFont.Dispose();
            }

            e.DrawFocusRectangle();
        }

        private void AutoSaveHistory()
        {
            _autoSaveCounter++;
            if (_autoSaveCounter >= AutoSaveInterval)
            {
                _autoSaveCounter = 0;
                try
                {
                    SaveCurrentRtfToActiveSession();
                    _chatManager.SaveHistory();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[AUTO SAVE] {ex.Message}");
                }
            }
        }

        private void FrmChat_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                SaveCurrentRtfToActiveSession();
                _chatManager.SaveHistory();

                var stream = _stream;
                if (_client != null && _client.Connected && stream != null)
                {
                    string msg = $"LOGOUT|{_username}";
                    byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
                    lock (stream)
                    {
                        if (stream.CanWrite)
                            stream.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LOGOUT] {ex.Message}");
            }
            // Dispose cached GDI objects
            _cachedFontBold?.Dispose();
            _cachedFontRegular?.Dispose();
            _cachedFontBoldItalic?.Dispose();
            _cachedFontUnderline?.Dispose();
            Cleanup();
        }

        private void Cleanup()
        {
            try { _stream?.Close(); } catch (Exception ex) { Debug.WriteLine($"[CLEANUP] stream: {ex.Message}"); }
            try { _stream?.Dispose(); } catch (Exception ex) { Debug.WriteLine($"[CLEANUP] stream dispose: {ex.Message}"); }
            try { _client?.Close(); } catch (Exception ex) { Debug.WriteLine($"[CLEANUP] client: {ex.Message}"); }
            try { _client?.Dispose(); } catch (Exception ex) { Debug.WriteLine($"[CLEANUP] client dispose: {ex.Message}"); }
            _stream = null;
            _client = null;
        }

        private bool IsConnected()
        {
            try
            {
                return _client != null && _client.Connected
                    && _stream != null && _stream.CanWrite;
            }
            catch
            {
                return false;
            }
        }

        private void FrmChat_Load(object sender, EventArgs e)
        {
            Text = "Chat Application - " + _username;
            lblCurrentUser.Text = "User: " + _username;

            bool historyLoaded = _chatManager.LoadHistory();
            if (historyLoaded)
                AppendSystemMessage("Đã khôi phục lịch sử chat.");

            RefreshSessionList();
            AppendSystemMessage("Chào mừng " + _username + " đã kết nối vào phòng!");

            _receiveThread = new Thread(ReceiveMessage);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }

        private void DisplayImageInline(string imagePath, string caption)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, string>(DisplayImageInline), imagePath, caption);
                return;
            }

            try
            {
                if (!File.Exists(imagePath)) return;

                // Chèn caption + link đến file ảnh, không dùng Clipboard/Image.FromFile để tránh khóa file
                AppendColoredText(caption + "\n", Color.FromArgb(0, 120, 0), italic: true);
                AppendLinkText(imagePath);
                rtbChat.AppendText("\n");
                rtbChat.ScrollToCaret();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DISPLAY IMAGE] {ex.Message}");
                AppendColoredText($"[Hệ thống] Ảnh đã lưu: {imagePath}\n", Color.FromArgb(0, 100, 180));
                rtbChat.ScrollToCaret();
            }
        }

        private void RtbChat_LinkClicked(object? sender, LinkClickedEventArgs e)
        {
            string? link = e.LinkText;
            string? ext = Path.GetExtension(link)?.ToLowerInvariant();
            if (ext != null && AllowedImageExtensions.Contains(ext) && File.Exists(link))
            {
                try { Process.Start(new ProcessStartInfo { FileName = link, UseShellExecute = true }); }
                catch (Exception ex) { Debug.WriteLine($"[OPEN IMAGE] {ex.Message}"); }
            }
        }

        private void RefreshSessionList()
        {
            lstChatSessions.Items.Clear();
            foreach (var session in _chatManager.Sessions)
            {
                // Chỉ dùng tên session; unread badge được DrawItem vẽ riêng
                lstChatSessions.Items.Add(session.Name);
            }

            var activeForSelect = _chatManager.ActiveSession;
            if (activeForSelect != null)
            {
                int index = -1;
                for (int i = 0; i < _chatManager.Sessions.Count; i++)
                {
                    if (_chatManager.Sessions[i].Id == activeForSelect.Id) { index = i; break; }
                }
                if (index >= 0 && index < lstChatSessions.Items.Count)
                    lstChatSessions.SelectedIndex = index;
            }
        }

        private void OnSessionListChanged()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(OnSessionListChanged)); return; }
            RefreshSessionList();
        }

        private void OnActiveSessionChanged(ChatSession session)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => OnActiveSessionChanged(session))); return; }

            SaveCurrentRtfToActiveSession();

            if (!string.IsNullOrEmpty(session.RtfContent))
            {
                try { rtbChat.Rtf = session.RtfContent; }
                catch { session.RtfContent = null; }
            }

            if (string.IsNullOrEmpty(session.RtfContent))
            {
                rtbChat.Clear();
                foreach (string msg in session.Messages)
                    RenderMessageFromHistory(msg);
            }

            RefreshSessionList();
            this.Text = "Chat Application - " + _username + " [" + session.Name + "]";
            AutoScrollToEnd();
        }

        private void RenderMessageFromHistory(string msg)
        {
            if (msg.StartsWith("[Hệ thống]"))
            {
                AppendSystemMessage(msg.TrimEnd('\n'));
            }
            else if (msg.Contains(" -> "))
            {
                int timeEnd = msg.IndexOf(']');
                if (timeEnd > 0) AppendColoredText(msg, Color.FromArgb(80, 0, 100));
                else AppendColoredText(msg, Color.FromArgb(80, 0, 100));
            }
            else
            {
                int timeEnd = msg.IndexOf(']');
                if (timeEnd > 0)
                {
                    string time = msg.Substring(1, timeEnd - 1);
                    string rest = msg.Substring(timeEnd + 1).Trim();
                    int colonIndex = rest.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        string name = rest.Substring(0, colonIndex).Trim();
                        string content = rest.Substring(colonIndex + 1).Trim();
                        AppendMessage(time, name, content, Color.FromArgb(40, 100, 180));
                        return;
                    }
                }
                AppendColoredText(msg, Color.FromArgb(50, 50, 50));
            }
        }

        private void SaveCurrentRtfToActiveSession()
        {
            if (_chatManager.ActiveSession != null)
                _chatManager.ActiveSession.RtfContent = rtbChat.Rtf;
        }

        private void lstChatSessions_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstChatSessions.SelectedIndex < 0) return;
            if (lstChatSessions.SelectedIndex >= _chatManager.Sessions.Count) return;

            var selected = _chatManager.Sessions[lstChatSessions.SelectedIndex];
            if (selected.Id != _chatManager.ActiveSession?.Id)
                _chatManager.SetActiveSession(selected.Id);
        }

        private void btnNewChat_Click(object? sender, EventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(
                "Nhập tên phòng chat mới:", "Tạo phòng mới",
                "Phòng chat " + (_chatManager.Sessions.Count + 1));
            if (!string.IsNullOrWhiteSpace(name))
                _chatManager.CreateSession(name.Trim());
        }

        private void btnRenameChat_Click(object? sender, EventArgs e)
        {
            if (_chatManager.ActiveSession == null) return;
            string newName = Microsoft.VisualBasic.Interaction.InputBox(
                "Nhập tên mới cho phòng chat:", "Đổi tên phòng",
                _chatManager.ActiveSession.Name);
            if (!string.IsNullOrWhiteSpace(newName))
                _chatManager.RenameSession(_chatManager.ActiveSession.Id, newName.Trim());
        }

        private void btnDeleteChat_Click(object? sender, EventArgs e)
        {
            if (_chatManager.ActiveSession == null) return;
            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa phòng '" + _chatManager.ActiveSession.Name + "'?",
                "Xóa phòng chat", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                _chatManager.DeleteSession(_chatManager.ActiveSession.Id);
        }

        private void lstOnlineUsers_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            int index = lstOnlineUsers.IndexFromPoint(e.Location);
            if (index < 0) return;
            string item = lstOnlineUsers.Items[index].ToString() ?? "";
            if (item.StartsWith("Online - ") && item.Length > "Online - ".Length)
                OpenPrivateChatWith(item.Substring("Online - ".Length));
        }

        private void OpenPrivateChatWith(string partnerUsername)
        {
            if (string.IsNullOrWhiteSpace(partnerUsername) || partnerUsername == _username) return;
            var session = _chatManager.GetOrCreatePrivateSession(partnerUsername, _username);
            _chatManager.SetActiveSession(session.Id);
        }

        private void btnEmoji_Click(object? sender, EventArgs e)
        {
            var emojiPicker = new EmojiPickerForm(txtMessage);
            emojiPicker.Location = new Point(
                this.Location.X + this.pnlInput.Left + this.btnEmoji.Left,
                this.Location.Y + this.pnlInput.Top + this.btnEmoji.Top - emojiPicker.Height);
            var screen = Screen.FromControl(this).WorkingArea;
            if (emojiPicker.Location.X + emojiPicker.Width > screen.Right)
                emojiPicker.Location = new Point(screen.Right - emojiPicker.Width, emojiPicker.Location.Y);
            if (emojiPicker.Location.Y < screen.Top)
                emojiPicker.Location = new Point(emojiPicker.Location.X, screen.Top);
            emojiPicker.ShowDialog();
        }

        private ChatSession GetPublicSession()
        {
            var pub = _chatManager.Sessions.FirstOrDefault(s => !s.IsPrivate);
            if (pub != null) return pub;
            // Nếu không còn session public nào, tự tạo lại "Phòng chat chính"
            pub = _chatManager.CreateSession("Phòng chat chính");
            return pub;
        }

        private bool IsActiveSessionPublic()
        {
            return _chatManager.ActiveSession != null && !_chatManager.ActiveSession.IsPrivate;
        }

        private void ProcessTextLine(string line)
        {
            // Split với limit để '|' trong nội dung không bị mất
            // PRIVATE|sender|content -> 3 parts; content có thể chứa '|'
            // BROADCAST|msg -> 2 parts; msg có thể chứa '|'
            // FILE_ANNOUNCE|sender -> 2 parts
            string[] tokens = line.Split(new[] { '|' }, 3);
            if (tokens.Length == 0) return;
            string command = tokens[0];

            if (command == "SERVER_SHUTDOWN")
            {
                HandleDisconnect("Server đã đóng cửa!");
                return;
            }

            if (command == "PRIVATE" && tokens.Length >= 3)
            {
                string sender = tokens[1];
                string content = tokens[2];
                string time = DateTime.Now.ToString("HH:mm");

                var session = _chatManager.GetOrCreatePrivateSessionSilent(sender);
                session.AddMessage($"[{time}] {sender}: {content}\n");

                if (_chatManager.ActiveSession != null && _chatManager.ActiveSession.Id == session.Id)
                {
                    AppendPrivateChatLine(time, sender, _username, content, isSender: false);
                    AutoScrollToEnd();
                }
                else
                {
                    session.UnreadCount++;
                    RefreshSessionList();
                    ShowNotification($"[PRIVATE] {sender}: {content}", sender);
                }
                AutoSaveHistory();
            }
            else if (command == "PRIVATE_ECHO" && tokens.Length >= 3)
            {
                string receiverPartner = tokens[1];
                string content = tokens[2];
                string time = DateTime.Now.ToString("HH:mm");
                var session = _chatManager.GetOrCreatePrivateSessionSilent(receiverPartner);
                session.AddMessage($"[{time}] {_username} -> {receiverPartner}: {content}\n");
                AutoSaveHistory();
            }
            else if (command == "DELIVERED" && tokens.Length >= 3)
            {
                string toUser = tokens[1];
                string fromUser = tokens[2];
                string statusLine = $"✓✓ {fromUser} đã nhận";
                AppendColoredText($"[Hệ thống] {statusLine}\n", Color.FromArgb(0, 140, 60), italic: true);
                AutoScrollToEnd();
                var session = _chatManager.GetOrCreatePrivateSessionSilent(fromUser);
                session.AddMessage($"[Hệ thống] {statusLine}\n");
                AutoSaveHistory();
            }
            else if (command == "RECIPIENT_NOT_FOUND" && tokens.Length >= 2)
            {
                string notFoundUser = tokens[1];
                string errorMsg = $"Người dùng '{notFoundUser}' không tồn tại hoặc đã offline.";
                AppendColoredText($"[Hệ thống] {errorMsg}\n", Color.FromArgb(200, 60, 60), italic: true);
                AutoScrollToEnd();
                if (_chatManager.ActiveSession != null && _chatManager.ActiveSession.IsPrivate)
                {
                    string partnerName = _chatManager.ActiveSession.PrivateChatPartner ?? notFoundUser;
                    var session = _chatManager.GetOrCreatePrivateSessionSilent(partnerName);
                    session.AddMessage($"[Hệ thống] {errorMsg}\n");
                }
                AutoSaveHistory();
            }
            else if (command == "BROADCAST" && tokens.Length > 1)
            {
                string messageContent = tokens[1];

                if (messageContent.StartsWith("[Hệ thống]"))
                {
                    AddPublicMessage(messageContent);
                    if (IsActiveSessionPublic()) { AppendSystemMessage(messageContent); AutoScrollToEnd(); }
                }
                else
                {
                    int colonIndex = messageContent.IndexOf(':');
                    if (colonIndex > 0 && messageContent.Contains(": "))
                    {
                        string senderName = messageContent.Substring(0, colonIndex).Trim();
                        string actualContent = messageContent.Substring(colonIndex + 1).Trim();
                        string time = DateTime.Now.ToString("HH:mm");
                        string formattedMsg = $"[{time}] {senderName}: {actualContent}\n";
                        AddPublicMessage(formattedMsg);
                        if (IsActiveSessionPublic()) { AppendOtherMessage(senderName, actualContent, time); AutoScrollToEnd(); }
                    }
                }
                AutoSaveHistory();
            }
            else if (command == "UPDATE_ONLINE" && tokens.Length > 1)
            {
                UpdateOnlineUsers(tokens[1]);
            }
            else if (command == "FILE_ANNOUNCE" && tokens.Length >= 2)
            {
                string announceContent = tokens[1];
                if (announceContent.Contains(" -> "))
                {
                    var parts = announceContent.Split(new[] { " -> " }, StringSplitOptions.None);
                    string senderName = parts[0].Trim();
                    string msg = $"[Hệ thống] {senderName} đã gửi 1 file riêng.";
                    var session = _chatManager.GetOrCreatePrivateSessionSilent(senderName);
                    session.AddMessage(msg + "\n");
                    if (_chatManager.ActiveSession?.Id == session.Id) { AppendSystemMessage(msg); AutoScrollToEnd(); }
                    else { session.UnreadCount++; RefreshSessionList(); ShowNotification($"[PRIVATE] {senderName} đã gửi 1 file.", senderName); }
                }
                else
                {
                    string senderName = announceContent;
                    string msg = $"[Hệ thống] {senderName} đã gửi 1 file.";
                    AddPublicMessage(msg + "\n");
                    if (IsActiveSessionPublic()) { AppendSystemMessage(msg); AutoScrollToEnd(); }
                }
                AutoSaveHistory();
            }
        }

        private void ReceiveMessage()
        {
            var client = _client;
            if (client == null) return;
            Socket socket = client.Client;

            while (_stream is NetworkStream stream)
            {
                try
                {
                    byte[] peekBuffer = new byte[1];
                    int peekBytes;
                    try { peekBytes = socket.Receive(peekBuffer, 1, SocketFlags.Peek); }
                    catch (SocketException) { HandleDisconnect("Mất kết nối với Server."); break; }

                    if (peekBytes == 0) { HandleDisconnect("Server ngừng hoạt động."); break; }

                    byte firstByte = peekBuffer[0];

                    if (firstByte == 0x10 || firstByte == 0x11)
                    {
                        ReceiveFileData(socket);
                    }
                    else
                    {
                        string? line = ChatHelper.ReadLineUtf8(stream);
                        if (line == null) { HandleDisconnect("Mất kết nối với Server."); break; }
                        if (string.IsNullOrEmpty(line)) continue;

                        var capturedLine = line;
                        if (!this.IsDisposed && this.IsHandleCreated)
                            this.BeginInvoke(new Action<string>(ProcessTextLine), capturedLine);
                    }
                }
                catch (Exception ex)
                {
                    if (this.IsDisposed || !this.IsHandleCreated) return;
                    Debug.WriteLine($"[RECEIVE ERROR] {ex.Message}");
                    HandleDisconnect("Đường truyền bị đứt!");
                    return;
                }
            }
        }

        private void ShowNotification(string message, string? senderName = null)
        {
            if (this.InvokeRequired) { this.Invoke(new Action<string, string?>(ShowNotification), message, senderName); return; }

            string originalText = "Chat Application - " + _username;
            if (_chatManager.ActiveSession != null) originalText += " [" + _chatManager.ActiveSession.Name + "]";

            string shortMsg = senderName != null ? $"📩 {senderName}" : "📩 Tin nhắn mới";
            this.Text = $"{shortMsg} - {originalText}";

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000;
            timer.Tick += (s, e) => { this.Text = originalText; timer.Stop(); timer.Dispose(); };
            timer.Start();
        }

        private void AutoScrollToEnd()
        {
            if (rtbChat.InvokeRequired) { rtbChat.Invoke(new Action(AutoScrollToEnd)); return; }
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.ScrollToCaret();
        }

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

        private void ReceiveFileData(Socket socket)
        {
            try
            {
                byte[] header = new byte[9];
                if (!ReceiveExact(socket, header, 9)) { HandleDisconnect("Mất kết nối khi đang nhận file."); return; }

                byte firstByte = header[0];
                long dataSize = BitConverter.ToInt64(header, 1);

                if (dataSize <= 0 || dataSize > MaxFileSize)
                {
                    string typeName = firstByte == 0x10 ? "ảnh" : "video";
                    string errMsg = $"[Hệ thống] Tệp {typeName} quá lớn (>{MaxFileSize / 1024 / 1024}MB) hoặc không hợp lệ - đã từ chối.";
                    AppendSystemMessage(errMsg);
                    if (_chatManager.ActiveSession != null) _chatManager.ActiveSession.AddMessage(errMsg + "\n");
                    byte[] discard = new byte[Math.Min(dataSize, 8192)];
                    long totalDiscarded = 0;
                    while (totalDiscarded < dataSize)
                    {
                        int toRead = (int)Math.Min(dataSize - totalDiscarded, discard.Length);
                        int read = socket.Receive(discard, 0, toRead, SocketFlags.None);
                        if (read == 0) break;
                        totalDiscarded += read;
                    }
                    return;
                }

                byte[] payload = new byte[dataSize];
                int totalReceived = 0;
                while (totalReceived < dataSize)
                {
                    int read = socket.Receive(payload, totalReceived, (int)(dataSize - totalReceived), SocketFlags.None);
                    if (read == 0) break;
                    totalReceived += read;
                }

                bool isImage = firstByte == 0x10;
                string type = isImage ? "Images" : "Videos";
                string ext = isImage ? ".jpg" : ".mp4";
                string fileName = $"{_username}_{type}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                string saveDir = Path.Combine(Application.StartupPath, "ReceivedFiles", type);
                Directory.CreateDirectory(saveDir);
                string savePath = Path.Combine(saveDir, fileName);
                File.WriteAllBytes(savePath, payload);

                var capturedPath = savePath;
                var capturedName = fileName;
                var capturedPayloadLen = payload.Length;
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        if (isImage)
                        {
                            string imgCaption = $"[Hệ thống] Ảnh đã nhận - {capturedName}";
                            DisplayImageInline(capturedPath, imgCaption);
                            string sysMsg = imgCaption + $" ({capturedPayloadLen / 1024}KB)";
                            var activeImg = _chatManager.ActiveSession;
                            if (activeImg != null && activeImg.IsPrivate)
                            { activeImg.AddMessage(sysMsg + "\n"); activeImg.RtfContent = null; }
                            else { AddPublicMessage(sysMsg); }
                        }
                        else
                        {
                            string sysMsg = $"[Hệ thống] Bạn nhận được 1 Video ({capturedPayloadLen / 1024}KB). Đã lưu tại: {capturedPath}";
                            var activeVid = _chatManager.ActiveSession;
                            if (activeVid != null && activeVid.IsPrivate)
                            {
                                activeVid.AddMessage(sysMsg + "\n");
                                activeVid.RtfContent = null;
                                AppendSystemMessage($"[Hệ thống] Bạn nhận được 1 Video ({capturedPayloadLen / 1024}KB).");
                                AppendLinkText(capturedPath);
                                rtbChat.AppendText("\n");
                                AutoScrollToEnd();
                            }
                            else
                            {
                                AddPublicMessage(sysMsg);
                                if (IsActiveSessionPublic())
                                {
                                    AppendSystemMessage($"[Hệ thống] Bạn nhận được 1 Video ({capturedPayloadLen / 1024}KB).");
                                    AppendLinkText(capturedPath);
                                    rtbChat.AppendText("\n");
                                    AutoScrollToEnd();
                                }
                            }
                        }
                        AutoSaveHistory();
                    }));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RECEIVE FILE] {ex.Message}");
                AppendSystemMessage("[Hệ thống] Lỗi khi nhận file.");
            }
        }

        private void AppendLinkText(string url)
        {
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.SelectionColor = Color.FromArgb(0, 80, 200);
            rtbChat.SelectionFont = GetFontUnderline();
            rtbChat.AppendText(url);
            rtbChat.SelectionColor = rtbChat.ForeColor;
            rtbChat.SelectionFont = rtbChat.Font;
        }

        private void AddPublicMessage(string message)
        {
            var publicSession = GetPublicSession();
            publicSession.AddMessage(message);
            SaveCurrentRtfToActiveSession();
            publicSession.RtfContent = null;
        }

        private void HandleDisconnect(string reason)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (this.InvokeRequired) { this.Invoke(new MethodInvoker(delegate { HandleDisconnect(reason); })); return; }

            _chatManager.SaveHistory();
            DialogResult res = MessageBox.Show(reason + Environment.NewLine + "Ứng dụng sẽ đóng.",
                "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (res == DialogResult.OK) this.Close();
        }

        private void SendTextMessage()
        {
            string content = txtMessage.Text.Trim();

            if (string.IsNullOrEmpty(content)) return;

            if (!IsConnected())
            {
                AppendSystemMessage("[Hệ thống] Mất kết nối đến Server. Không thể gửi tin nhắn.");
                AutoScrollToEnd();
                return;
            }

            string time = DateTime.Now.ToString("HH:mm");

            try
            {
                if (_chatManager.ActiveSession != null && _chatManager.ActiveSession.IsPrivate)
                {
                    var receiver = _chatManager.ActiveSession.PrivateChatPartner;
                    if (string.IsNullOrWhiteSpace(receiver))
                    {
                        AppendSystemMessage("[Hệ thống] Không xác định được người nhận trong cuộc trò chuyện riêng.");
                        AutoScrollToEnd();
                        return;
                    }

                    AppendPrivateChatLine(time, _username, receiver, content, isSender: true);
                    AppendColoredText($"  ✓ Đã gửi\n", Color.FromArgb(140, 140, 140), italic: true);
                    AutoScrollToEnd();

                    string msg = $"PRIVATE|{_username}|{ChatHelper.Sanitize(receiver)}|{ChatHelper.Sanitize(content)}";
                    byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
                    { var s2 = _stream; if (s2 != null) { lock (s2) { if (s2.CanWrite) s2.Write(data, 0, data.Length); } } }
                }
                else
                {
                    AppendMyMessage(_username, content, time);
                    AppendColoredText($"  ✓ Đã gửi\n", Color.FromArgb(140, 140, 140), italic: true);
                    AutoScrollToEnd();

                    // Lưu tin public mình gửi vào public session (HIGH-7)
                    string formattedMsg = $"[{time}] {_username}: {content}\n";
                    AddPublicMessage(formattedMsg);

                    string msg = $"MSG|{_username}|{ChatHelper.Sanitize(content)}";
                    byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
                    { var s2 = _stream; if (s2 != null) { lock (s2) { if (s2.CanWrite) s2.Write(data, 0, data.Length); } } }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SEND TEXT] {ex.Message}");
                MessageBox.Show("Lỗi gửi tin nhắn! Kiểm tra kết nối.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            txtMessage.Clear();
            txtMessage.Focus();
            AutoSaveHistory();
        }

        private void btnSend_Click(object sender, EventArgs e) => SendTextMessage();

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                if (!string.IsNullOrWhiteSpace(txtMessage.Text))
                    SendTextMessage();
            }
        }

        private void SendFile(byte typeCode, string filter)
        {
            if (!IsConnected())
            {
                AppendSystemMessage("[Hệ thống] Mất kết nối đến Server. Không thể gửi file.");
                AutoScrollToEnd();
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = filter })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string filePath = ofd.FileName;
                    string fileExt = Path.GetExtension(filePath)?.ToLowerInvariant() ?? "";

                    bool validExt;
                    if (typeCode == 0x10) validExt = AllowedImageExtensions.Contains(fileExt);
                    else validExt = AllowedVideoExtensions.Contains(fileExt);

                    if (!validExt)
                    {
                        string allowed = typeCode == 0x10
                            ? string.Join(", ", AllowedImageExtensions)
                            : string.Join(", ", AllowedVideoExtensions);
                        MessageBox.Show($"Định dạng file không được hỗ trợ!\nChỉ chấp nhận: {allowed}",
                            "Lỗi định dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        byte[] fileData = File.ReadAllBytes(filePath);
                        long sizeMB = fileData.Length / (1024 * 1024);

                        if (fileData.Length > MaxFileSize)
                        {
                            MessageBox.Show($"File quá lớn! Tối đa {MaxFileSize / 1024 / 1024}MB.\nFile của bạn: {sizeMB}MB.",
                                "Lỗi kích thước", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        byte[] header = new byte[9];
                        header[0] = typeCode;
                        byte[] sizeBytes = BitConverter.GetBytes((long)fileData.Length);
                        Array.Copy(sizeBytes, 0, header, 1, 8);

                        var activeSession = _chatManager.ActiveSession;
                        string? privatePartner = (activeSession != null
                            && activeSession.IsPrivate
                            && !string.IsNullOrEmpty(activeSession.PrivateChatPartner))
                            ? activeSession.PrivateChatPartner
                            : null;

                        var stream = _stream;
                        if (stream == null) return;
                        lock (stream)
                        {
                            if (!stream.CanWrite)
                            {
                                MessageBox.Show("Mất kết nối đến Server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (privatePartner != null)
                            {
                                string receiver = privatePartner;
                                string cmd = $"SEND_FILE_PRIVATE|{receiver}|{fileData.Length}";
                                byte[] cmdData = Encoding.UTF8.GetBytes(cmd + "\n");
                                stream.Write(cmdData, 0, cmdData.Length);
                                stream.Write(header, 0, 9);
                                stream.Write(fileData, 0, fileData.Length);
                            }
                            else
                            {
                                string cmd = $"SEND_FILE_PUBLIC|{fileData.Length}";
                                byte[] cmdData = Encoding.UTF8.GetBytes(cmd + "\n");
                                stream.Write(cmdData, 0, cmdData.Length);
                                stream.Write(header, 0, 9);
                                stream.Write(fileData, 0, fileData.Length);
                            }
                        }

                        string typeStr = typeCode == 0x10 ? "Ảnh" : "Video";
                        string time = DateTime.Now.ToString("HH:mm");
                        string sizeStr = fileData.Length >= 1024 * 1024
                            ? $"{(float)fileData.Length / (1024 * 1024):F1}MB"
                            : $"{fileData.Length / 1024}KB";

                        if (activeSession != null && privatePartner != null)
                        {
                            string receiver = privatePartner;
                            string privateMsg = $"[{time}] {_username} -> {receiver}: [Bạn đã gửi 1 tệp {typeStr} - {sizeStr}]\n";
                            activeSession.AddMessage(privateMsg);
                            activeSession.RtfContent = null;
                            AppendPrivateChatLine(time, _username, receiver, $"[Bạn đã gửi 1 tệp {typeStr} ({sizeStr})]", isSender: true);
                            AutoScrollToEnd();
                        }
                        else
                        {
                            string publicMsg = $"[{time}] {_username}: [Bạn đã gửi 1 tệp {typeStr} - {sizeStr}]\n";
                            AddPublicMessage(publicMsg);
                            if (IsActiveSessionPublic())
                            {
                                AppendMyMessage(_username, $"[Bạn đã gửi 1 tệp {typeStr} ({sizeStr})]", time);
                                AutoScrollToEnd();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SEND FILE] {ex.Message}");
                        MessageBox.Show("Lỗi khi gửi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            AutoSaveHistory();
        }

        private void btnSendImage_Click(object sender, EventArgs e) { SendFile(0x10, "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp"); }
        private void btnSendVideo_Click(object sender, EventArgs e) { SendFile(0x11, "Video Files|*.mp4;*.avi;*.mkv"); }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                var stream = _stream;
                if (stream != null && stream.CanWrite)
                {
                    string msg = $"LOGOUT|{_username}";
                    byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
                    lock (stream) { stream.Write(data, 0, data.Length); }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[LOGOUT] {ex.Message}"); }
            // Ngăn FormClosing gửi LOGOUT lần 2 (duplicate)
            _stream = null;
            this.Close();
        }

        public void UpdateOnlineUsers(string rawData)
        {
            if (lstOnlineUsers.InvokeRequired) { lstOnlineUsers.Invoke(new Action(() => UpdateOnlineUsers(rawData))); return; }

            _onlineUserManager.SyncFromServer(rawData);
            lstOnlineUsers.Items.Clear();
            foreach (var userInfo in _onlineUserManager.AllUsers)
            {
                if (string.Equals(userInfo.Username, _username, StringComparison.OrdinalIgnoreCase)) continue;
                lstOnlineUsers.Items.Add($"Online - {userInfo.Username}");
            }
            lblOnlineCount.Text = $"Online: {_onlineUserManager.Count}";
        }

        private void lstOnlineUsers_MouseMove(object? sender, MouseEventArgs e)
        {
            int index = lstOnlineUsers.IndexFromPoint(e.Location);
            if (index >= 0)
            {
                string item = lstOnlineUsers.Items[index].ToString() ?? "";
                if (item.StartsWith("Online - ") && item.Length > "Online - ".Length)
                {
                    string username = item.Substring("Online - ".Length);
                    var userInfo = _onlineUserManager.GetByUsername(username);
                    if (userInfo != null)
                    {
                        string tooltipText = $"Tên: {userInfo.Username}\n"
                                           + $"Trạng thái: Online\n"
                                           + $"Đăng nhập: {userInfo.LoginTime:HH:mm:ss}";
                        _userToolTip.SetToolTip(lstOnlineUsers, tooltipText);
                    }
                }
            }
        }

        public void AppendMyMessage(string name, string content, string time)
        {
            if (rtbChat.InvokeRequired) { rtbChat.Invoke(new Action(() => AppendMyMessage(name, content, time))); return; }
            AppendMessage(time, name, content, Color.FromArgb(40, 100, 180));
        }

        public void AppendOtherMessage(string name, string content, string time)
        {
            if (rtbChat.InvokeRequired) { rtbChat.Invoke(new Action(() => AppendOtherMessage(name, content, time))); return; }
            AppendMessage(time, name, content, Color.FromArgb(160, 90, 30));
        }

        private void AppendPrivateChatLine(string time, string sender, string receiver, string content, bool isSender)
        {
            if (rtbChat.InvokeRequired) { rtbChat.Invoke(new Action(() => AppendPrivateChatLine(time, sender, receiver, content, isSender))); return; }

            string title = isSender ? $"{sender} -> {receiver}" : $"{sender}";
            AppendColoredText("[" + time + "] ", Color.FromArgb(150, 150, 150));
            AppendColoredText(title + ": ", Color.FromArgb(120, 0, 150), bold: true);
            AppendColoredText(content + "\n", Color.FromArgb(60, 0, 80));
            rtbChat.ScrollToCaret();
        }

        public void AppendSystemMessage(string message)
        {
            if (rtbChat.InvokeRequired) { rtbChat.Invoke(new Action(() => AppendSystemMessage(message))); return; }
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

            // Use cached Font to avoid GDI handle leak
            if (bold && italic)
                rtbChat.SelectionFont = GetFontBoldItalic();
            else if (bold)
                rtbChat.SelectionFont = GetFontBold();
            else if (italic)
                rtbChat.SelectionFont = GetFontItalic();
            else
                rtbChat.SelectionFont = rtbChat.Font;

            rtbChat.AppendText(text);
            rtbChat.SelectionColor = rtbChat.ForeColor;
        }
    }
}
