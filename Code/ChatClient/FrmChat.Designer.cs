namespace ChatApp
{
    partial class FrmChat
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel       pnlTop;
        private System.Windows.Forms.Panel       pnlLeft;
        private System.Windows.Forms.Panel       pnlCenter;
        private System.Windows.Forms.Panel       pnlInput;
        private System.Windows.Forms.Panel       pnlFileBar;
        private System.Windows.Forms.Label       lblCurrentUser;
        private System.Windows.Forms.Button      btnLogout;
        private System.Windows.Forms.Label       lblOnlineTitle;
        private System.Windows.Forms.Label       lblOnlineCount;
        private System.Windows.Forms.ListBox     lstOnlineUsers;
        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.TextBox     txtMessage;
        private System.Windows.Forms.Button      btnSend;
        private System.Windows.Forms.Button      btnSendImage;
        private System.Windows.Forms.Button      btnSendVideo;
        private System.Windows.Forms.Label       lblFileName;
        private System.Windows.Forms.ProgressBar progressBar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlTop         = new System.Windows.Forms.Panel();
            this.pnlLeft        = new System.Windows.Forms.Panel();
            this.pnlCenter      = new System.Windows.Forms.Panel();
            this.pnlInput       = new System.Windows.Forms.Panel();
            this.pnlFileBar     = new System.Windows.Forms.Panel();
            this.lblCurrentUser = new System.Windows.Forms.Label();
            this.btnLogout      = new System.Windows.Forms.Button();
            this.lblOnlineTitle = new System.Windows.Forms.Label();
            this.lblOnlineCount = new System.Windows.Forms.Label();
            this.lstOnlineUsers = new System.Windows.Forms.ListBox();
            this.rtbChat        = new System.Windows.Forms.RichTextBox();
            this.txtMessage     = new System.Windows.Forms.TextBox();
            this.btnSend        = new System.Windows.Forms.Button();
            this.btnSendImage   = new System.Windows.Forms.Button();
            this.btnSendVideo   = new System.Windows.Forms.Button();
            this.lblFileName    = new System.Windows.Forms.Label();
            this.progressBar    = new System.Windows.Forms.ProgressBar();

            this.Text            = "Chat App";
            this.Size            = new System.Drawing.Size(1000, 660);
            this.MinimumSize     = new System.Drawing.Size(860, 540);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor       = System.Drawing.Color.FromArgb(245, 245, 245);
            this.Font            = new System.Drawing.Font("Segoe UI", 9.5f);
            this.Load           += new System.EventHandler(this.FrmChat_Load);

            this.pnlTop.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Height    = 52;
            this.pnlTop.BackColor = System.Drawing.Color.White;
            this.pnlTop.Paint    += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(204, 204, 204), 1),
                    0, this.pnlTop.Height - 1, this.pnlTop.Width, this.pnlTop.Height - 1);
            };

            this.lblCurrentUser.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblCurrentUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCurrentUser.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.lblCurrentUser.Font      = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
            this.lblCurrentUser.Padding   = new System.Windows.Forms.Padding(16, 0, 0, 0);

            this.btnLogout.Text      = "Dang xuat";
            this.btnLogout.Size      = new System.Drawing.Size(110, 34);
            this.btnLogout.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnLogout.FlatAppearance.BorderSize  = 1;
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            this.btnLogout.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnLogout.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.btnLogout.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.Click    += new System.EventHandler(this.btnLogout_Click);
            this.btnLogout.MouseEnter += (s, e) => this.btnLogout.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnLogout.MouseLeave += (s, e) => this.btnLogout.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.pnlTop.Controls.Add(this.lblCurrentUser);
            this.pnlTop.Controls.Add(this.btnLogout);

            this.pnlLeft.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width     = 200;
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Paint    += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(204, 204, 204), 1),
                    this.pnlLeft.Width - 1, 0, this.pnlLeft.Width - 1, this.pnlLeft.Height);
            };

            this.lblOnlineTitle.Text      = "NGUOI DUNG";
            this.lblOnlineTitle.Font      = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold);
            this.lblOnlineTitle.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
            this.lblOnlineTitle.Size      = new System.Drawing.Size(120, 18);
            this.lblOnlineTitle.Location  = new System.Drawing.Point(12, 12);

            this.lblOnlineCount.Text      = "Online: 0";
            this.lblOnlineCount.Font      = new System.Drawing.Font("Segoe UI", 7.5f);
            this.lblOnlineCount.ForeColor = System.Drawing.Color.FromArgb(80, 160, 80);
            this.lblOnlineCount.Size      = new System.Drawing.Size(70, 18);
            this.lblOnlineCount.Location  = new System.Drawing.Point(125, 12);

            this.lstOnlineUsers.Location    = new System.Drawing.Point(0, 36);
            this.lstOnlineUsers.Size        = new System.Drawing.Size(200, 520);
            this.lstOnlineUsers.Anchor      = System.Windows.Forms.AnchorStyles.Top
                                            | System.Windows.Forms.AnchorStyles.Bottom
                                            | System.Windows.Forms.AnchorStyles.Left
                                            | System.Windows.Forms.AnchorStyles.Right;
            this.lstOnlineUsers.BackColor   = System.Drawing.Color.White;
            this.lstOnlineUsers.ForeColor   = System.Drawing.Color.FromArgb(60, 60, 60);
            this.lstOnlineUsers.Font        = new System.Drawing.Font("Segoe UI", 10f);
            this.lstOnlineUsers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstOnlineUsers.ItemHeight  = 30;

            this.pnlLeft.Controls.Add(this.lblOnlineTitle);
            this.pnlLeft.Controls.Add(this.lblOnlineCount);
            this.pnlLeft.Controls.Add(this.lstOnlineUsers);

            this.pnlFileBar.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFileBar.Height    = 44;
            this.pnlFileBar.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            this.pnlFileBar.Paint    += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(204, 204, 204), 1),
                    0, 0, this.pnlFileBar.Width, 0);
            };

            this.btnSendImage.Text      = "Gui Anh";
            this.btnSendImage.Size      = new System.Drawing.Size(88, 34);
            this.btnSendImage.Location  = new System.Drawing.Point(8, 4);
            this.btnSendImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendImage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnSendImage.FlatAppearance.BorderSize  = 1;
            this.btnSendImage.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            this.btnSendImage.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.btnSendImage.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.btnSendImage.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSendImage.Click    += new System.EventHandler(this.btnSendImage_Click);
            this.btnSendImage.MouseEnter += (s, e) => this.btnSendImage.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnSendImage.MouseLeave += (s, e) => this.btnSendImage.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.btnSendVideo.Text      = "Gui Video";
            this.btnSendVideo.Size      = new System.Drawing.Size(95, 34);
            this.btnSendVideo.Location  = new System.Drawing.Point(104, 4);
            this.btnSendVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendVideo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnSendVideo.FlatAppearance.BorderSize  = 1;
            this.btnSendVideo.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            this.btnSendVideo.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.btnSendVideo.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.btnSendVideo.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSendVideo.Click    += new System.EventHandler(this.btnSendVideo_Click);
            this.btnSendVideo.MouseEnter += (s, e) => this.btnSendVideo.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnSendVideo.MouseLeave += (s, e) => this.btnSendVideo.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.lblFileName.Text      = "";
            this.lblFileName.Size      = new System.Drawing.Size(300, 34);
            this.lblFileName.Location  = new System.Drawing.Point(210, 4);
            this.lblFileName.ForeColor = System.Drawing.Color.FromArgb(80, 140, 80);
            this.lblFileName.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic);
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.progressBar.Size     = new System.Drawing.Size(200, 10);
            this.progressBar.Location = new System.Drawing.Point(520, 14);
            this.progressBar.Visible  = false;
            this.progressBar.Style    = System.Windows.Forms.ProgressBarStyle.Continuous;

            this.pnlFileBar.Controls.Add(this.btnSendImage);
            this.pnlFileBar.Controls.Add(this.btnSendVideo);
            this.pnlFileBar.Controls.Add(this.lblFileName);
            this.pnlFileBar.Controls.Add(this.progressBar);

            this.pnlInput.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInput.Height    = 56;
            this.pnlInput.BackColor = System.Drawing.Color.White;
            this.pnlInput.Padding   = new System.Windows.Forms.Padding(8);
            this.pnlInput.Paint    += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(204, 204, 204), 1),
                    0, 0, this.pnlInput.Width, 0);
            };

            this.txtMessage.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.BackColor   = System.Drawing.Color.White;
            this.txtMessage.ForeColor   = System.Drawing.Color.FromArgb(50, 50, 50);
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMessage.Font        = new System.Drawing.Font("Segoe UI", 11f);
            this.txtMessage.KeyDown    += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);

            this.btnSend.Text      = "Gui";
            this.btnSend.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnSend.Width     = 90;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnSend.FlatAppearance.BorderSize  = 1;
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            this.btnSend.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnSend.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this.btnSend.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSend.Click    += new System.EventHandler(this.btnSend_Click);
            this.btnSend.MouseEnter += (s, e) => this.btnSend.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnSend.MouseLeave += (s, e) => this.btnSend.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.pnlInput.Controls.Add(this.txtMessage);
            this.pnlInput.Controls.Add(this.btnSend);

            this.rtbChat.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.rtbChat.BackColor   = System.Drawing.Color.FromArgb(250, 250, 250);
            this.rtbChat.ForeColor   = System.Drawing.Color.FromArgb(50, 50, 50);
            this.rtbChat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbChat.Font        = new System.Drawing.Font("Consolas", 10.5f);
            this.rtbChat.ReadOnly    = true;
            this.rtbChat.ScrollBars  = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbChat.WordWrap    = true;

            this.pnlCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCenter.Controls.Add(this.rtbChat);
            this.pnlCenter.Controls.Add(this.pnlInput);
            this.pnlCenter.Controls.Add(this.pnlFileBar);

            this.Controls.Add(this.pnlCenter);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlTop);
        }
    }
}
