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





            // ===========================
            // HEADER
            // ===========================


            pnlTop.Dock = DockStyle.Top;

            pnlTop.Height = 50;

            pnlTop.BackColor =
                Color.White;


            lblCurrentUser.Text =
                "👤 User";


            lblCurrentUser.Location =
                new Point(25,15);


            lblCurrentUser.AutoSize = true;


            lblCurrentUser.Font =
                new Font(
                    "Segoe UI",
                    16,
                    FontStyle.Bold);



            lblStatus.Text =
                "● Online";


            lblStatus.Location =
                new Point(30,48);


            lblStatus.ForeColor =
                Color.Green;


            lblStatus.Font =
                new Font(
                    "Segoe UI",
                    10);




            btnLogout.Text =
                "Đăng xuất";


            btnLogout.Dock =
                DockStyle.Right;


            btnLogout.Width =
                130;


            btnLogout.FlatStyle =
                FlatStyle.Flat;


            btnLogout.FlatAppearance.BorderSize = 0;


            btnLogout.BackColor =
                Color.FromArgb(
                    255,
                    80,
                    90);


            btnLogout.ForeColor =
                Color.White;


            btnLogout.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);


            btnLogout.Click +=
                btnLogout_Click;




            pnlTop.Controls.Add(lblCurrentUser);
            pnlTop.Controls.Add(lblStatus);
            pnlTop.Controls.Add(btnLogout);









            // ===========================
            // SIDEBAR
            // ===========================


            pnlLeft.Dock =
                DockStyle.Left;


            pnlLeft.Width =
                260;


            pnlLeft.BackColor =
                Color.FromArgb(
                    25,
                    35,
                    55);




            lblOnlineTitle.Text =
                "💬Thành viên";

            
            lblOnlineTitle.Location =
                new Point(25,25);
            lblOnlineTitle.Size = new Size(200, 30);

            lblOnlineTitle.ForeColor =
                Color.White;


            lblOnlineTitle.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);




            lblOnlineCount.Text =
                "Online: 0";


            lblOnlineCount.Location =
                new Point(25,60);


            lblOnlineCount.ForeColor =
                Color.LightGreen;




            lstOnlineUsers.Location =
                new Point(15,100);


            lstOnlineUsers.Size =
                new Size(
                    230,
                    550);



            lstOnlineUsers.BackColor =
                Color.FromArgb(
                    35,
                    45,
                    70);



            lstOnlineUsers.ForeColor =
                Color.White;


            lstOnlineUsers.BorderStyle =
                BorderStyle.None;


            lstOnlineUsers.Font =
                new Font(
                    "Segoe UI",
                    11);




            pnlLeft.Controls.Add(lblOnlineTitle);
            pnlLeft.Controls.Add(lblOnlineCount);
            pnlLeft.Controls.Add(lstOnlineUsers);









            // ===========================
            // CHAT AREA
            // ===========================


            pnlCenter.Dock =
                DockStyle.Fill;


            pnlCenter.BackColor =
                Color.FromArgb(
                    240,
                    242,
                    247);




            rtbChat.Dock =
                DockStyle.Fill;



            rtbChat.ReadOnly = true;


            rtbChat.BorderStyle =
                BorderStyle.None;



            rtbChat.Font =
                new Font(
                    "Segoe UI",
                    12);



            rtbChat.BackColor =
                Color.FromArgb(
                    240,
                    242,
                    247);






            // ===========================
            // INPUT
            // ===========================


            pnlInput.Dock =
                DockStyle.Bottom;


            pnlInput.Height =
                75;


            pnlInput.BackColor =
                Color.White;



            txtMessage.Location =
                new Point(20,15);



            txtMessage.Size =
                new Size(
                    650,
                    45);



            txtMessage.Font =
                new Font(
                    "Segoe UI",
                    20);



            txtMessage.BorderStyle =
                BorderStyle.FixedSingle;



            txtMessage.KeyDown +=
                txtMessage_KeyDown;

            btnEmoji.Text =
                "😊";


            btnEmoji.Location =
                new Point(690,15);



            btnEmoji.Size =
                new Size(
                    45,
                    45);

            btnEmoji.Font =
                new Font(
                    "Segoe UI Emoji",
                    18,
                    FontStyle.Regular);
            btnEmoji.ForeColor = 
                Color.OrangeRed;




            btnSend.Text =
                " Gửi ➤";


            btnSend.Location =
                new Point(750,15);



            btnSend.Size =
                new Size(
                    100,
                    45);



            btnSend.BackColor =
                Color.FromArgb(
                    0,
                    132,
                    255);



            btnSend.ForeColor =
                Color.White;



            btnSend.FlatStyle =
                FlatStyle.Flat;


            btnSend.FlatAppearance.BorderSize = 0;



            btnSend.Font =
                new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold);



            btnSend.Click +=
                btnSend_Click;





            pnlInput.Controls.Add(txtMessage);

            pnlInput.Controls.Add(btnEmoji);

            pnlInput.Controls.Add(btnSend);









            // ===========================
            // FILE BAR
            // ===========================



            pnlFileBar.Dock =
                DockStyle.Bottom;


            pnlFileBar.Height =
                45;


            pnlFileBar.BackColor =
                Color.White;


            btnSendImage.Text =
                "📷 Ảnh";


            btnSendImage.Location =
                new Point(20,5);

            btnSendImage.Size = new Size(90,35);

            btnSendImage.Click +=
                btnSendImage_Click;




            btnSendVideo.Text =
                "🎥 Video";

            btnSendVideo.Size = new Size(90,35);
            
            btnSendVideo.Location =
                new Point(150,5);


            btnSendVideo.Click +=
                btnSendVideo_Click;





            pnlFileBar.Controls.Add(btnSendImage);

            pnlFileBar.Controls.Add(btnSendVideo);

            pnlFileBar.Controls.Add(lblFileName);






            // ADD CONTROL


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



            ResumeLayout(false);

        }

    }
}