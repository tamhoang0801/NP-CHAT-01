namespace ChatApp
{
    partial class FrmChat
    {
        private System.ComponentModel.IContainer components = null;


        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlCenter;
        private System.Windows.Forms.Panel pnlInput;
        private System.Windows.Forms.Panel pnlFileBar;


        private System.Windows.Forms.Label lblCurrentUser;
        private System.Windows.Forms.Button btnLogout;


        private System.Windows.Forms.Label lblOnlineTitle;
        private System.Windows.Forms.Label lblOnlineCount;
        private System.Windows.Forms.ListBox lstOnlineUsers;


        private System.Windows.Forms.RichTextBox rtbChat;


        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;


        private System.Windows.Forms.Button btnSendImage;
        private System.Windows.Forms.Button btnSendVideo;

        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.ProgressBar progressBar;



        protected override void Dispose(bool disposing)
        {
            if(disposing && components!=null)
                components.Dispose();

            base.Dispose(disposing);
        }



        private void InitializeComponent()
        {


            this.pnlTop = new System.Windows.Forms.Panel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.pnlCenter = new System.Windows.Forms.Panel();
            this.pnlInput = new System.Windows.Forms.Panel();
            this.pnlFileBar = new System.Windows.Forms.Panel();



            this.lblCurrentUser = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();


            this.lblOnlineTitle = new System.Windows.Forms.Label();
            this.lblOnlineCount = new System.Windows.Forms.Label();

            this.lstOnlineUsers = new System.Windows.Forms.ListBox();


            this.rtbChat = new System.Windows.Forms.RichTextBox();


            this.txtMessage = new System.Windows.Forms.TextBox();

            this.btnSend = new System.Windows.Forms.Button();


            this.btnSendImage = new System.Windows.Forms.Button();

            this.btnSendVideo = new System.Windows.Forms.Button();


            this.lblFileName = new System.Windows.Forms.Label();

            this.progressBar = new System.Windows.Forms.ProgressBar();



            // FORM

            this.Text="Messenger Chat";

            this.Size =
            new System.Drawing.Size(1100,700);


            this.StartPosition =
            System.Windows.Forms.FormStartPosition.CenterScreen;


            this.BackColor =
            System.Drawing.Color.FromArgb(245,247,252);

            this.Font =
            new System.Drawing.Font("Segoe UI",10);



            // HEADER


            pnlTop.Dock =
            System.Windows.Forms.DockStyle.Top;


            pnlTop.Height=65;


            pnlTop.BackColor =
            System.Drawing.Color.FromArgb(70,120,220);



            lblCurrentUser.Text="User";

            lblCurrentUser.ForeColor =
            System.Drawing.Color.White;


            lblCurrentUser.Font =
            new System.Drawing.Font(
            "Segoe UI",
            14,
            System.Drawing.FontStyle.Bold);



            lblCurrentUser.Location =
            new System.Drawing.Point(25,18);


            lblCurrentUser.AutoSize=true;



            btnLogout.Text="Đăng xuất";


            btnLogout.Size =
            new System.Drawing.Size(120,40);



            btnLogout.Location =
            new System.Drawing.Point(940,12);



            btnLogout.BackColor =
            System.Drawing.Color.FromArgb(220,60,60);



            btnLogout.ForeColor =
            System.Drawing.Color.White;


            btnLogout.FlatStyle =
            System.Windows.Forms.FlatStyle.Flat;



            btnLogout.Click +=
            new System.EventHandler(
            this.btnLogout_Click);



            pnlTop.Controls.Add(lblCurrentUser);

            pnlTop.Controls.Add(btnLogout);






            // LEFT USER LIST


            pnlLeft.Dock =
            System.Windows.Forms.DockStyle.Left;


            pnlLeft.Width=240;


            pnlLeft.BackColor =
            System.Drawing.Color.White;



            lblOnlineTitle.Text="NGƯỜI ONLINE";


            lblOnlineTitle.Font =
            new System.Drawing.Font(
            "Segoe UI",
            11,
            System.Drawing.FontStyle.Bold);



            lblOnlineTitle.Location =
            new System.Drawing.Point(20,20);


            lblOnlineTitle.AutoSize=true;



            lblOnlineCount.Text="Online: 0";


            lblOnlineCount.ForeColor =
            System.Drawing.Color.Green;


            lblOnlineCount.Location =
            new System.Drawing.Point(20,50);



            lstOnlineUsers.Location =
            new System.Drawing.Point(10,90);



            lstOnlineUsers.Size =
            new System.Drawing.Size(220,500);



            lstOnlineUsers.Font =
            new System.Drawing.Font(
            "Segoe UI",
            11);



            lstOnlineUsers.BorderStyle =
            System.Windows.Forms.BorderStyle.None;




            pnlLeft.Controls.Add(lblOnlineTitle);

            pnlLeft.Controls.Add(lblOnlineCount);

            pnlLeft.Controls.Add(lstOnlineUsers);







            // CHAT AREA


            rtbChat.Dock =
            System.Windows.Forms.DockStyle.Fill;


            rtbChat.BackColor =
            System.Drawing.Color.White;



            rtbChat.BorderStyle =
            System.Windows.Forms.BorderStyle.None;


            rtbChat.Font =
            new System.Drawing.Font(
            "Segoe UI",
            11);



            rtbChat.ReadOnly=true;




            pnlCenter.Dock =
            System.Windows.Forms.DockStyle.Fill;



            pnlCenter.Controls.Add(rtbChat);

            pnlCenter.Controls.Add(pnlInput);

            pnlCenter.Controls.Add(pnlFileBar);





            // INPUT


            pnlInput.Dock =
            System.Windows.Forms.DockStyle.Bottom;


            pnlInput.Height=65;


            pnlInput.BackColor =
            System.Drawing.Color.White;



            txtMessage.Location =
            new System.Drawing.Point(20,15);


            txtMessage.Size =
            new System.Drawing.Size(750,35);



            txtMessage.Font =
            new System.Drawing.Font(
            "Segoe UI",
            11);



            btnSend.Text="Gửi";


            btnSend.Location =
            new System.Drawing.Point(800,15);



            btnSend.Size =
            new System.Drawing.Size(100,35);



            btnSend.BackColor =
            System.Drawing.Color.FromArgb(70,120,220);



            btnSend.ForeColor =
            System.Drawing.Color.White;


            btnSend.FlatStyle =
            System.Windows.Forms.FlatStyle.Flat;



            btnSend.Click +=
            new System.EventHandler(
            this.btnSend_Click);




            pnlInput.Controls.Add(txtMessage);

            pnlInput.Controls.Add(btnSend);

            // FILE BAR
            pnlFileBar.Dock =
            System.Windows.Forms.DockStyle.Bottom;


            pnlFileBar.Height=45;

            btnSendImage.Text="📷 Ảnh";


            btnSendImage.Location =
            new System.Drawing.Point(20,5);

            btnSendImage.Click +=
            new System.EventHandler(
            this.btnSendImage_Click);
            btnSendVideo.Text="🎥 Video";


            btnSendVideo.Location =
            new System.Drawing.Point(120,5);


            btnSendVideo.Click +=
            new System.EventHandler(
            this.btnSendVideo_Click);

            pnlFileBar.Controls.Add(btnSendImage);

            pnlFileBar.Controls.Add(btnSendVideo);
            this.Controls.Add(pnlCenter);

            this.Controls.Add(pnlLeft);

            this.Controls.Add(pnlTop);



            this.Load +=
            new System.EventHandler(
            this.FrmChat_Load);

        }
    }
}