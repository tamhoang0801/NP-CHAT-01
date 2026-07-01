using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp
{
    partial class FrmChat
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlTop;
        private Panel pnlLeft;
        private Panel pnlCenter;
        private Panel pnlInput;
        private Panel pnlFileBar;

        private Label lblCurrentUser;
        private Label lblStatus;
        private Label lblOnlineTitle;
        private Label lblOnlineCount;

        private Button btnLogout;
        private Button btnSend;
        private Button btnEmoji;
        private Button btnSendImage;
        private Button btnSendVideo;
        private Button btnChooseAvatar;

        private ListBox lstOnlineUsers;
        private RichTextBox rtbChat;
        private TextBox txtMessage;

        private ProgressBar progressBar;
        private PictureBox picAvatar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = AutoScaleMode.None;

            pnlTop = new Panel();
            pnlLeft = new Panel();
            pnlCenter = new Panel();
            pnlInput = new Panel();
            pnlFileBar = new Panel();

            lblCurrentUser = new Label();
            lblStatus = new Label();
            lblOnlineTitle = new Label();
            lblOnlineCount = new Label();

            btnLogout = new Button();
            btnSend = new Button();
            btnEmoji = new Button();
            btnSendImage = new Button();
            btnSendVideo = new Button();
            btnChooseAvatar = new Button();

            lstOnlineUsers = new ListBox();
            rtbChat = new RichTextBox();
            txtMessage = new TextBox();
            progressBar = new ProgressBar();
            picAvatar = new PictureBox();

            SuspendLayout();

            // ================= TOP =================
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 80;
            pnlTop.BackColor = Color.White;

            picAvatar.Location = new Point(25, 12);
            picAvatar.Size = new Size(55, 55);
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;

            lblCurrentUser.Text = "👤 User";
            lblCurrentUser.Location = new Point(90, 15);
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.Font = new Font("Segoe UI", 16, FontStyle.Bold);

            lblStatus.Text = "● Online";
            lblStatus.Location = new Point(92, 52);
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.Green;

            btnChooseAvatar.Text = "Avatar";
            btnChooseAvatar.Dock = DockStyle.Right;
            btnChooseAvatar.Width = 150;
            btnChooseAvatar.Click += btnChooseAvatar_Click;

            btnLogout.Text = "Đăng xuất";
            btnLogout.Dock = DockStyle.Right;
            btnLogout.Width = 140;
            btnLogout.Click += btnLogout_Click;

            pnlTop.Controls.Add(picAvatar);
            pnlTop.Controls.Add(lblCurrentUser);
            pnlTop.Controls.Add(lblStatus);
            pnlTop.Controls.Add(btnChooseAvatar);
            pnlTop.Controls.Add(btnLogout);

            // ================= LEFT =================
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Width = 260;
            pnlLeft.BackColor = Color.FromArgb(25, 35, 55);

            lblOnlineTitle.Text = "💬 Thành viên";
            lblOnlineTitle.Location = new Point(25, 25);
            lblOnlineTitle.ForeColor = Color.White;
            lblOnlineTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblOnlineTitle.AutoSize = true;

            lblOnlineCount.Text = "Online: 0";
            lblOnlineCount.Location = new Point(25, 60);
            lblOnlineCount.ForeColor = Color.LightGreen;
            lblOnlineCount.AutoSize = true;

            lstOnlineUsers.Location = new Point(15, 100);
            lstOnlineUsers.Size = new Size(230, 500);
            lstOnlineUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstOnlineUsers.BackColor = Color.FromArgb(35, 45, 70);
            lstOnlineUsers.ForeColor = Color.White;
            lstOnlineUsers.BorderStyle = BorderStyle.None;
            lstOnlineUsers.Font = new Font("Segoe UI", 11);

            pnlLeft.Controls.Add(lblOnlineTitle);
            pnlLeft.Controls.Add(lblOnlineCount);
            pnlLeft.Controls.Add(lstOnlineUsers);

            // ================= CENTER =================
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.BackColor = Color.FromArgb(240, 242, 247);

            rtbChat.Dock = DockStyle.Fill;
            rtbChat.ReadOnly = true;
            rtbChat.BorderStyle = BorderStyle.None;
            rtbChat.Font = new Font("Segoe UI", 12);

            // ================= INPUT =================
             pnlInput.Dock =
                DockStyle.Bottom;

            pnlInput.Height =
                75;


            pnlInput.BackColor =
                Color.White;





            // ================= TEXT MESSAGE =================


            txtMessage.Location =
                new Point(20,15);


            txtMessage.Height =
                45;


            txtMessage.Font =
                new Font(
                    "Segoe UI",
                    12);



            txtMessage.Anchor =
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Top;



            txtMessage.KeyDown +=
                txtMessage_KeyDown;







            // ================= EMOJI =================


            btnEmoji.Text =
                "😊";


            btnEmoji.Size =
                new Size(
                    45,
                    45);



            btnEmoji.Location =
                new Point(
                    700,
                    15);



            btnEmoji.Anchor =
                AnchorStyles.Right |
                AnchorStyles.Top;



            btnEmoji.Click +=
                btnEmoji_Click;







            // ================= SEND =================


            btnSend.Text =
                "Gửi ➤";



            btnSend.Size =
                new Size(
                    100,
                    45);



            btnSend.Location =
                new Point(
                    760,
                    15);



            btnSend.Anchor =
                AnchorStyles.Right |
                AnchorStyles.Top;



            btnSend.BackColor =
                Color.FromArgb(
                    0,
                    132,
                    255);



            btnSend.ForeColor =
                Color.White;



            btnSend.FlatStyle =
                FlatStyle.Flat;



            btnSend.Click +=
                btnSend_Click;





            pnlInput.Controls.Add(txtMessage);

            pnlInput.Controls.Add(btnEmoji);

            pnlInput.Controls.Add(btnSend);











            // ================= FILE =================


            pnlFileBar.Dock =
                DockStyle.Bottom;


            pnlFileBar.Height =
                45;






            btnSendImage.Text =
                "📷 Ảnh";


            btnSendImage.Location =
                new Point(20,5);


            btnSendImage.Size =
                new Size(
                    90,
                    35);



            btnSendImage.Click +=
                btnSendImage_Click;


            btnSendVideo.Text =
                "🎥 Video";



            btnSendVideo.Location =
                new Point(150,5);



            btnSendVideo.Size =
                new Size(
                    90,
                    35);



            btnSendVideo.Click +=
                btnSendVideo_Click;



            pnlFileBar.Controls.Add(btnSendImage);

            pnlFileBar.Controls.Add(btnSendVideo);


            pnlCenter.Controls.Add(rtbChat);

            pnlCenter.Controls.Add(pnlFileBar);

            pnlCenter.Controls.Add(pnlInput);






            Controls.Add(pnlCenter);

            Controls.Add(pnlLeft);

            Controls.Add(pnlTop);






            ClientSize =
                new Size(
                    1200,
                    750);
            MinimumSize =
                new Size(
                    1000,
                    600);





            Text =
                "Modern Chat Application";





            Load +=
                FrmChat_Load;




            Resize +=
                FrmChat_Resize;





            FrmChat_Resize(
                null,
                null);




            ResumeLayout(false);

        }







        private void FrmChat_Resize(
            object? sender,
            EventArgs e)
        {

            if(pnlInput == null)
                return;



            int y = 15;



            txtMessage.Location =
                new Point(
                    20,
                    y);



            txtMessage.Width =
                pnlInput.Width - 260;

            btnEmoji.Location =
                new Point(
                    pnlInput.Width - 170,
                    y);

            btnSend.Location =
                new Point(
                    pnlInput.Width - 110,
                    y);
        }
    }
}