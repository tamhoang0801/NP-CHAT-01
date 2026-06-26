namespace ChatApp
{
    partial class FrmChat
    {
        private System.ComponentModel.IContainer components = null!;

        private System.Windows.Forms.Panel       pnlTop = null!;
        private System.Windows.Forms.Panel       pnlLeft = null!;
        private System.Windows.Forms.Panel       pnlCenter = null!;
        private System.Windows.Forms.Panel       pnlInput = null!;
        private System.Windows.Forms.Panel       pnlFileBar = null!;
        private System.Windows.Forms.Label       lblCurrentUser = null!;
        private System.Windows.Forms.Button      btnLogout = null!;
        private System.Windows.Forms.Label       lblOnlineTitle = null!;
        private System.Windows.Forms.Label       lblOnlineCount = null!;
        private System.Windows.Forms.ListBox     lstOnlineUsers = null!;
        private System.Windows.Forms.RichTextBox rtbChat = null!;
        private System.Windows.Forms.TextBox     txtMessage = null!;
        private System.Windows.Forms.Button      btnSend = null!;
        private System.Windows.Forms.Button      btnSendImage = null!;
        private System.Windows.Forms.Button      btnSendVideo = null!;
        private System.Windows.Forms.Label       lblFileName = null!;
        private System.Windows.Forms.ProgressBar progressBar = null!;
        // Multi-chat session controls
        private System.Windows.Forms.Panel       pnlSessions = null!;
        private System.Windows.Forms.Label       lblSessionTitle = null!;
        private System.Windows.Forms.ListBox     lstChatSessions = null!;
        private System.Windows.Forms.Button      btnNewChat = null!;
        private System.Windows.Forms.Button      btnRenameChat = null!;
        private System.Windows.Forms.Button      btnDeleteChat = null!;
        // Emoji picker control
        private System.Windows.Forms.Button      btnEmoji = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlTop = new Panel();
            lblCurrentUser = new Label();
            btnLogout = new Button();
            pnlLeft = new Panel();
            lblOnlineTitle = new Label();
            lblOnlineCount = new Label();
            lstOnlineUsers = new ListBox();
            pnlCenter = new Panel();
            rtbChat = new RichTextBox();
            pnlInput = new Panel();
            txtMessage = new TextBox();
            btnSend = new Button();
            pnlFileBar = new Panel();
            btnSendImage = new Button();
            btnSendVideo = new Button();
            lblFileName = new Label();
            progressBar = new ProgressBar();
            pnlSessions = new Panel();
            lblSessionTitle = new Label();
            lstChatSessions = new ListBox();
            btnNewChat = new Button();
            btnRenameChat = new Button();
            btnDeleteChat = new Button();
            btnEmoji = new Button();
            pnlTop.SuspendLayout();
            pnlLeft.SuspendLayout();
            pnlCenter.SuspendLayout();
            pnlInput.SuspendLayout();
            pnlFileBar.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.White;
            pnlTop.Controls.Add(lblCurrentUser);
            pnlTop.Controls.Add(btnLogout);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(984, 52);
            pnlTop.TabIndex = 2;
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.Dock = DockStyle.Fill;
            lblCurrentUser.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblCurrentUser.ForeColor = Color.FromArgb(50, 50, 50);
            lblCurrentUser.Location = new Point(0, 0);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Padding = new Padding(16, 0, 0, 0);
            lblCurrentUser.Size = new Size(874, 52);
            lblCurrentUser.TabIndex = 0;
            lblCurrentUser.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnLogout
            // 
            btnLogout.BackColor = Color.FromArgb(224, 224, 224);
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Dock = DockStyle.Right;
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnLogout.ForeColor = Color.FromArgb(50, 50, 50);
            btnLogout.Location = new Point(874, 0);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(110, 52);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "Dang xuat";
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.White;
            pnlLeft.Controls.Add(pnlSessions);
            pnlLeft.Controls.Add(lblOnlineTitle);
            pnlLeft.Controls.Add(lblOnlineCount);
            pnlLeft.Controls.Add(lstOnlineUsers);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 52);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(200, 569);
            pnlLeft.TabIndex = 1;
            // 
            // lblOnlineTitle
            // 
            lblOnlineTitle.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblOnlineTitle.ForeColor = Color.FromArgb(100, 100, 100);
            lblOnlineTitle.Location = new Point(12, 12);
            lblOnlineTitle.Name = "lblOnlineTitle";
            lblOnlineTitle.Size = new Size(120, 18);
            lblOnlineTitle.TabIndex = 0;
            lblOnlineTitle.Text = "NGUOI DUNG";
            // 
            // lblOnlineCount
            // 
            lblOnlineCount.Font = new Font("Segoe UI", 7.5F);
            lblOnlineCount.ForeColor = Color.FromArgb(80, 160, 80);
            lblOnlineCount.Location = new Point(125, 12);
            lblOnlineCount.Name = "lblOnlineCount";
            lblOnlineCount.Size = new Size(70, 18);
            lblOnlineCount.TabIndex = 1;
            lblOnlineCount.Text = "Online: 0";
            // 
            // lstOnlineUsers
            // 
            lstOnlineUsers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstOnlineUsers.BackColor = Color.White;
            lstOnlineUsers.BorderStyle = BorderStyle.None;
            lstOnlineUsers.Font = new Font("Segoe UI", 10F);
            lstOnlineUsers.ForeColor = Color.FromArgb(60, 60, 60);
            lstOnlineUsers.Location = new Point(0, 36);
            lstOnlineUsers.Name = "lstOnlineUsers";
            lstOnlineUsers.Size = new Size(200, 180);
            lstOnlineUsers.TabIndex = 2;
            // 
            // pnlSessions
            // 
            pnlSessions.BackColor = Color.White;
            pnlSessions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlSessions.Controls.Add(lblSessionTitle);
            pnlSessions.Controls.Add(lstChatSessions);
            pnlSessions.Controls.Add(btnNewChat);
            pnlSessions.Controls.Add(btnRenameChat);
            pnlSessions.Controls.Add(btnDeleteChat);
            pnlSessions.Location = new Point(0, 220);
            pnlSessions.Name = "pnlSessions";
            pnlSessions.Size = new Size(200, 349);
            pnlSessions.TabIndex = 3;
            // 
            // lblSessionTitle
            // 
            lblSessionTitle.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblSessionTitle.ForeColor = Color.FromArgb(100, 100, 100);
            lblSessionTitle.Location = new Point(4, 6);
            lblSessionTitle.Name = "lblSessionTitle";
            lblSessionTitle.Size = new Size(120, 18);
            lblSessionTitle.TabIndex = 0;
            lblSessionTitle.Text = "PHONG CHAT";
            // 
            // lstChatSessions
            // 
            lstChatSessions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstChatSessions.BackColor = Color.White;
            lstChatSessions.BorderStyle = BorderStyle.None;
            lstChatSessions.Font = new Font("Segoe UI", 9.5F);
            lstChatSessions.ForeColor = Color.FromArgb(60, 60, 60);
            lstChatSessions.Location = new Point(4, 28);
            lstChatSessions.Name = "lstChatSessions";
            lstChatSessions.Size = new Size(192, 270);
            lstChatSessions.TabIndex = 1;
            // 
            // btnNewChat
            // 
            btnNewChat.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNewChat.BackColor = Color.FromArgb(224, 224, 224);
            btnNewChat.Cursor = Cursors.Hand;
            btnNewChat.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnNewChat.FlatStyle = FlatStyle.Flat;
            btnNewChat.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            btnNewChat.ForeColor = Color.FromArgb(60, 60, 60);
            btnNewChat.Location = new Point(4, 302);
            btnNewChat.Name = "btnNewChat";
            btnNewChat.Size = new Size(60, 22);
            btnNewChat.TabIndex = 2;
            btnNewChat.Text = "Moi";
            btnNewChat.UseVisualStyleBackColor = false;
            btnNewChat.Click += btnNewChat_Click;
            btnNewChat.MouseEnter += (s, e) => btnNewChat.BackColor = Color.FromArgb(210, 210, 210);
            btnNewChat.MouseLeave += (s, e) => btnNewChat.BackColor = Color.FromArgb(224, 224, 224);
            // 
            // btnRenameChat
            // 
            btnRenameChat.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRenameChat.BackColor = Color.FromArgb(224, 224, 224);
            btnRenameChat.Cursor = Cursors.Hand;
            btnRenameChat.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnRenameChat.FlatStyle = FlatStyle.Flat;
            btnRenameChat.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            btnRenameChat.ForeColor = Color.FromArgb(60, 60, 60);
            btnRenameChat.Location = new Point(68, 302);
            btnRenameChat.Name = "btnRenameChat";
            btnRenameChat.Size = new Size(60, 22);
            btnRenameChat.TabIndex = 3;
            btnRenameChat.Text = "Sua";
            btnRenameChat.UseVisualStyleBackColor = false;
            btnRenameChat.Click += btnRenameChat_Click;
            btnRenameChat.MouseEnter += (s, e) => btnRenameChat.BackColor = Color.FromArgb(210, 210, 210);
            btnRenameChat.MouseLeave += (s, e) => btnRenameChat.BackColor = Color.FromArgb(224, 224, 224);
            // 
            // btnDeleteChat
            // 
            btnDeleteChat.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteChat.BackColor = Color.FromArgb(224, 224, 224);
            btnDeleteChat.Cursor = Cursors.Hand;
            btnDeleteChat.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnDeleteChat.FlatStyle = FlatStyle.Flat;
            btnDeleteChat.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            btnDeleteChat.ForeColor = Color.FromArgb(60, 60, 60);
            btnDeleteChat.Location = new Point(132, 302);
            btnDeleteChat.Name = "btnDeleteChat";
            btnDeleteChat.Size = new Size(64, 22);
            btnDeleteChat.TabIndex = 4;
            btnDeleteChat.Text = "Xoa";
            btnDeleteChat.UseVisualStyleBackColor = false;
            btnDeleteChat.Click += btnDeleteChat_Click;
            btnDeleteChat.MouseEnter += (s, e) => btnDeleteChat.BackColor = Color.FromArgb(210, 210, 210);
            btnDeleteChat.MouseLeave += (s, e) => btnDeleteChat.BackColor = Color.FromArgb(224, 224, 224);
            // 
            // pnlCenter
            // 
            pnlCenter.Controls.Add(rtbChat);
            pnlCenter.Controls.Add(pnlInput);
            pnlCenter.Controls.Add(pnlFileBar);
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Location = new Point(200, 52);
            pnlCenter.Name = "pnlCenter";
            pnlCenter.Size = new Size(784, 569);
            pnlCenter.TabIndex = 0;
            // 
            // rtbChat
            // 
            rtbChat.BackColor = Color.FromArgb(250, 250, 250);
            rtbChat.BorderStyle = BorderStyle.None;
            rtbChat.Dock = DockStyle.Fill;
            rtbChat.Font = new Font("Consolas", 10.5F);
            rtbChat.ForeColor = Color.FromArgb(50, 50, 50);
            rtbChat.Location = new Point(0, 0);
            rtbChat.Name = "rtbChat";
            rtbChat.ReadOnly = true;
            rtbChat.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbChat.Size = new Size(784, 469);
            rtbChat.TabIndex = 0;
            rtbChat.Text = "";
            // 
            // pnlInput
            // 
            pnlInput.BackColor = Color.White;
            pnlInput.Controls.Add(btnEmoji);
            pnlInput.Controls.Add(txtMessage);
            pnlInput.Controls.Add(btnSend);
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Location = new Point(0, 469);
            pnlInput.Name = "pnlInput";
            pnlInput.Padding = new Padding(8);
            pnlInput.Size = new Size(784, 56);
            pnlInput.TabIndex = 1;
            // 
            // txtMessage
            // 
            txtMessage.BackColor = Color.White;
            txtMessage.BorderStyle = BorderStyle.FixedSingle;
            txtMessage.Dock = DockStyle.Fill;
            txtMessage.Font = new Font("Segoe UI", 11F);
            txtMessage.ForeColor = Color.FromArgb(50, 50, 50);
            txtMessage.Location = new Point(8, 8);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(648, 27);
            txtMessage.TabIndex = 0;
            txtMessage.KeyDown += txtMessage_KeyDown;
            // 
            // btnEmoji
            // 
            btnEmoji.BackColor = Color.FromArgb(224, 224, 224);
            btnEmoji.Cursor = Cursors.Hand;
            btnEmoji.Dock = DockStyle.Right;
            btnEmoji.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnEmoji.FlatStyle = FlatStyle.Flat;
            btnEmoji.Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular);
            btnEmoji.ForeColor = Color.FromArgb(60, 60, 60);
            btnEmoji.Location = new Point(656, 8);
            btnEmoji.Name = "btnEmoji";
            btnEmoji.Size = new Size(30, 40);
            btnEmoji.TabIndex = 2;
            btnEmoji.Text = "😊";
            btnEmoji.UseVisualStyleBackColor = false;
            btnEmoji.MouseEnter += (s, e) => btnEmoji.BackColor = Color.FromArgb(210, 210, 210);
            btnEmoji.MouseLeave += (s, e) => btnEmoji.BackColor = Color.FromArgb(224, 224, 224);
            // 
            // btnSend
            // 
            btnSend.BackColor = Color.FromArgb(224, 224, 224);
            btnSend.Cursor = Cursors.Hand;
            btnSend.Dock = DockStyle.Right;
            btnSend.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSend.ForeColor = Color.FromArgb(50, 50, 50);
            btnSend.Location = new Point(686, 8);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(90, 40);
            btnSend.TabIndex = 1;
            btnSend.Text = "Gui";
            btnSend.UseVisualStyleBackColor = false;
            btnSend.Click += btnSend_Click;
            // 
            // pnlFileBar
            // 
            pnlFileBar.BackColor = Color.FromArgb(248, 248, 248);
            pnlFileBar.Controls.Add(btnSendImage);
            pnlFileBar.Controls.Add(btnSendVideo);
            pnlFileBar.Controls.Add(lblFileName);
            pnlFileBar.Controls.Add(progressBar);
            pnlFileBar.Dock = DockStyle.Bottom;
            pnlFileBar.Location = new Point(0, 525);
            pnlFileBar.Name = "pnlFileBar";
            pnlFileBar.Size = new Size(784, 44);
            pnlFileBar.TabIndex = 2;
            // 
            // btnSendImage
            // 
            btnSendImage.BackColor = Color.FromArgb(224, 224, 224);
            btnSendImage.Cursor = Cursors.Hand;
            btnSendImage.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnSendImage.FlatStyle = FlatStyle.Flat;
            btnSendImage.Font = new Font("Segoe UI", 9F);
            btnSendImage.ForeColor = Color.FromArgb(60, 60, 60);
            btnSendImage.Location = new Point(8, 4);
            btnSendImage.Name = "btnSendImage";
            btnSendImage.Size = new Size(88, 34);
            btnSendImage.TabIndex = 0;
            btnSendImage.Text = "Gui Anh";
            btnSendImage.UseVisualStyleBackColor = false;
            btnSendImage.Click += btnSendImage_Click;
            // 
            // btnSendVideo
            // 
            btnSendVideo.BackColor = Color.FromArgb(224, 224, 224);
            btnSendVideo.Cursor = Cursors.Hand;
            btnSendVideo.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnSendVideo.FlatStyle = FlatStyle.Flat;
            btnSendVideo.Font = new Font("Segoe UI", 9F);
            btnSendVideo.ForeColor = Color.FromArgb(60, 60, 60);
            btnSendVideo.Location = new Point(104, 4);
            btnSendVideo.Name = "btnSendVideo";
            btnSendVideo.Size = new Size(95, 34);
            btnSendVideo.TabIndex = 1;
            btnSendVideo.Text = "Gui Video";
            btnSendVideo.UseVisualStyleBackColor = false;
            btnSendVideo.Click += btnSendVideo_Click;
            // 
            // lblFileName
            // 
            lblFileName.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblFileName.ForeColor = Color.FromArgb(80, 140, 80);
            lblFileName.Location = new Point(210, 4);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(300, 34);
            lblFileName.TabIndex = 2;
            lblFileName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(520, 14);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(200, 10);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 3;
            progressBar.Visible = false;
            // 
            // FrmChat
            // 
            BackColor = Color.FromArgb(245, 245, 245);
            ClientSize = new Size(984, 621);
            Controls.Add(pnlCenter);
            Controls.Add(pnlLeft);
            Controls.Add(pnlTop);
            Font = new Font("Segoe UI", 9.5F);
            MinimumSize = new Size(860, 540);
            Name = "FrmChat";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chat App";
            Load += FrmChat_Load;
            pnlTop.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            pnlCenter.ResumeLayout(false);
            pnlInput.ResumeLayout(false);
            pnlInput.PerformLayout();
            pnlFileBar.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
