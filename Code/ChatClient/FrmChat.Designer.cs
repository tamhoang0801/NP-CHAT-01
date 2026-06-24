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
        private Button btnLogout;

        private Label lblOnlineTitle;
        private Label lblOnlineCount;

        private ListBox lstOnlineUsers;

        private RichTextBox rtbChat;

        private TextBox txtMessage;

        private Button btnSend;
        private Button btnEmoji;

        private Button btnSendImage;
        private Button btnSendVideo;

        private Label lblFileName;
        private ProgressBar progressBar;


        protected override void Dispose(bool disposing)
        {
            if(disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }



        private void InitializeComponent()
        {

            pnlTop = new Panel();
            pnlLeft = new Panel();
            pnlCenter = new Panel();

            pnlInput = new Panel();
            pnlFileBar = new Panel();


            lblCurrentUser = new Label();
            lblStatus = new Label();

            btnLogout = new Button();


            lblOnlineTitle = new Label();
            lblOnlineCount = new Label();

            lstOnlineUsers = new ListBox();


            rtbChat = new RichTextBox();


            txtMessage = new TextBox();

            btnSend = new Button();
            btnEmoji = new Button();


            btnSendImage = new Button();
            btnSendVideo = new Button();


            lblFileName = new Label();

            progressBar = new ProgressBar();



            SuspendLayout();



            // ================= TOP =================


            pnlTop.Dock =
                DockStyle.Top;

            pnlTop.Height =
                70;

            pnlTop.BackColor =
                Color.White;



            lblCurrentUser.AutoSize=false;

            lblCurrentUser.Size =
                new Size(500,35);

            lblCurrentUser.Location =
                new Point(20,8);

            lblCurrentUser.Font =
                new Font(
                "Segoe UI",
                14,
                FontStyle.Bold);



            lblStatus.AutoSize=true;

            lblStatus.Location =
                new Point(25,42);

            lblStatus.ForeColor =
                Color.Green;

            lblStatus.Text =
                "🟢 Online";



            btnLogout.Dock =
                DockStyle.Right;

            btnLogout.Width =
                120;

            btnLogout.Text =
                "Đăng xuất";

            btnLogout.BackColor =
                Color.FromArgb(255,90,90);

            btnLogout.ForeColor =
                Color.White;

            btnLogout.FlatStyle =
                FlatStyle.Flat;

            btnLogout.FlatAppearance.BorderSize=0;

            btnLogout.Click +=
                btnLogout_Click;



            pnlTop.Controls.Add(lblCurrentUser);
            pnlTop.Controls.Add(lblStatus);
            pnlTop.Controls.Add(btnLogout);





            // ================= LEFT =================



            pnlLeft.Dock =
                DockStyle.Left;


            pnlLeft.Width =
                220;


            pnlLeft.BackColor =
                Color.FromArgb(32,42,68);



            lblOnlineTitle.Text =
                "NGƯỜI DÙNG";


            lblOnlineTitle.ForeColor =
                Color.White;


            lblOnlineTitle.Font =
                new Font(
                "Segoe UI",
                11,
                FontStyle.Bold);


            lblOnlineTitle.Location =
                new Point(20,20);




            lblOnlineCount.Text =
                "Online: 0";


            lblOnlineCount.ForeColor =
                Color.LightGreen;


            lblOnlineCount.Location =
                new Point(20,50);




            lstOnlineUsers.Location =
                new Point(0,85);


            lstOnlineUsers.Size =
                new Size(
                220,
                500);


            lstOnlineUsers.BorderStyle =
                BorderStyle.None;


            lstOnlineUsers.BackColor =
                Color.FromArgb(32,42,68);


            lstOnlineUsers.ForeColor =
                Color.White;


            lstOnlineUsers.Font =
                new Font(
                "Segoe UI",
                11);




            pnlLeft.Controls.Add(lblOnlineTitle);
            pnlLeft.Controls.Add(lblOnlineCount);
            pnlLeft.Controls.Add(lstOnlineUsers);






            // ================= CHAT =================



            pnlCenter.Dock =
                DockStyle.Fill;



            rtbChat.Dock =
                DockStyle.Fill;


            rtbChat.ReadOnly=true;


            rtbChat.BorderStyle =
                BorderStyle.None;


            rtbChat.BackColor =
                Color.FromArgb(
                245,
                247,
                250);


            rtbChat.Font =
                new Font(
                "Segoe UI",
                11);




            pnlCenter.Controls.Add(rtbChat);






            // ================= INPUT =================



            pnlInput.Dock =
                DockStyle.Bottom;


            pnlInput.Height =
                60;


            pnlInput.BackColor =
                Color.White;




            txtMessage.Location =
                new Point(15,12);


            txtMessage.Size =
                new Size(
                650,
                35);


            txtMessage.Font =
                new Font(
                "Segoe UI",
                12);



            txtMessage.KeyDown +=
                txtMessage_KeyDown;





            btnEmoji.Text =
                "😊";


            btnEmoji.Location =
                new Point(680,12);


            btnEmoji.Size =
                new Size(
                45,
                35);





            btnSend.Text =
                "Gửi ➤";


            btnSend.Location =
                new Point(730,12);


            btnSend.Size =
                new Size(
                90,
                35);



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
                new Point(10,5);


            btnSendImage.Click +=
                btnSendImage_Click;





            btnSendVideo.Text =
                "🎥 Video";


            btnSendVideo.Location =
                new Point(110,5);


            btnSendVideo.Click +=
                btnSendVideo_Click;




            pnlFileBar.Controls.Add(btnSendImage);

            pnlFileBar.Controls.Add(btnSendVideo);

            pnlFileBar.Controls.Add(lblFileName);







            // QUAN TRỌNG:
            // Dock đúng thứ tự


            pnlCenter.Controls.Add(rtbChat);

            pnlCenter.Controls.Add(pnlFileBar);

            pnlCenter.Controls.Add(pnlInput);






            // FORM


            ClientSize =
                new Size(
                1200,
                750);


            MinimumSize =
                new Size(
                1000,
                600);



            Controls.Add(pnlCenter);

            Controls.Add(pnlLeft);

            Controls.Add(pnlTop);



            Text =
                "Chat Application";



            Load +=
                FrmChat_Load;



            ResumeLayout(false);

        }
    }
}