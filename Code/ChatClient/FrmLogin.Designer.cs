namespace ChatApp
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Label lblTitle;

        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblPort;

        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;

        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label lblStatus;


        protected override void Dispose(bool disposing)
        {
            if(disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.lblSub = new System.Windows.Forms.Label();

            this.pnlCard = new System.Windows.Forms.Panel();

            this.lblTitle = new System.Windows.Forms.Label();

            this.lblUser = new System.Windows.Forms.Label();
            this.lblIP = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();

            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();

            this.btnLogin = new System.Windows.Forms.Button();

            this.lblStatus = new System.Windows.Forms.Label();


            // FORM
            this.Text = "Chat App - Login";
            this.Size = new System.Drawing.Size(900,550);
            this.StartPosition =
            System.Windows.Forms.FormStartPosition.CenterScreen;

            this.BackColor =
            System.Drawing.Color.FromArgb(240,245,255);

            this.Font =
            new System.Drawing.Font("Segoe UI",10);


            // LEFT PANEL

            this.pnlLeft.Dock =
            System.Windows.Forms.DockStyle.Left;

            this.pnlLeft.Width = 300;

            this.pnlLeft.BackColor =
            System.Drawing.Color.FromArgb(70,120,220);



            this.lblLogo.Text="CHAT";

            this.lblLogo.ForeColor =
            System.Drawing.Color.White;

            this.lblLogo.Font =
            new System.Drawing.Font(
            "Segoe UI",
            38,
            System.Drawing.FontStyle.Bold);


            this.lblLogo.Location =
            new System.Drawing.Point(80,150);


            this.lblLogo.AutoSize=true;



            this.lblSub.Text =
            "TCP CHAT APP\n\nKết nối - Chia sẻ - Trò chuyện";


            this.lblSub.ForeColor =
            System.Drawing.Color.White;


            this.lblSub.Font =
            new System.Drawing.Font(
            "Segoe UI",
            12,
            System.Drawing.FontStyle.Bold);



            this.lblSub.Location =
            new System.Drawing.Point(45,230);



            this.lblSub.AutoSize=true;



            this.pnlLeft.Controls.Add(lblLogo);
            this.pnlLeft.Controls.Add(lblSub);




            // CARD LOGIN

            this.pnlCard.Location =
            new System.Drawing.Point(360,50);

            this.pnlCard.Size =
            new System.Drawing.Size(450,430);


            this.pnlCard.BackColor =
            System.Drawing.Color.White;




            // TITLE

            this.lblTitle.Text =
            "ĐĂNG NHẬP";


            this.lblTitle.Font =
            new System.Drawing.Font(
            "Segoe UI",
            24,
            System.Drawing.FontStyle.Bold);


            this.lblTitle.ForeColor =
            System.Drawing.Color.FromArgb(40,80,160);


            this.lblTitle.Location =
            new System.Drawing.Point(120,40);


            this.lblTitle.AutoSize=true;




            // LABEL

            lblUser.Text="Tên người dùng";
            lblIP.Text="IP SERVER";
            lblPort.Text="PORT";


            lblUser.Location =
            new System.Drawing.Point(40,120);

            lblIP.Location =
            new System.Drawing.Point(40,200);

            lblPort.Location =
            new System.Drawing.Point(270,200);



            foreach(var l in new[]{lblUser,lblIP,lblPort})
            {
                l.ForeColor =
                System.Drawing.Color.Gray;

                l.Font =
                new System.Drawing.Font(
                "Segoe UI",
                9,
                System.Drawing.FontStyle.Bold);

                l.AutoSize=true;
            }



            // TEXTBOX

            txtUsername.Location =
            new System.Drawing.Point(40,145);

            txtUsername.Size =
            new System.Drawing.Size(360,35);



            txtIP.Location =
            new System.Drawing.Point(40,225);


            txtIP.Size =
            new System.Drawing.Size(210,35);



            txtPort.Location =
            new System.Drawing.Point(270,225);


            txtPort.Size =
            new System.Drawing.Size(130,35);



            foreach(var t in new[]{txtUsername,txtIP,txtPort})
            {
                t.Font =
                new System.Drawing.Font(
                "Segoe UI",
                11);

                t.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;
            }





            // BUTTON

            btnLogin.Text =
            "ĐĂNG NHẬP";


            btnLogin.Location =
            new System.Drawing.Point(40,300);


            btnLogin.Size =
            new System.Drawing.Size(360,50);



            btnLogin.BackColor =
            System.Drawing.Color.FromArgb(70,120,220);


            btnLogin.ForeColor =
            System.Drawing.Color.White;



            btnLogin.FlatStyle =
            System.Windows.Forms.FlatStyle.Flat;


            btnLogin.Font =
            new System.Drawing.Font(
            "Segoe UI",
            12,
            System.Drawing.FontStyle.Bold);



            btnLogin.Cursor =
            System.Windows.Forms.Cursors.Hand;


            btnLogin.Click +=
            new System.EventHandler(
            this.btnLogin_Click);





            // STATUS


            lblStatus.Text =
            "Chưa kết nối";


            lblStatus.Location =
            new System.Drawing.Point(40,370);


            lblStatus.AutoSize=true;


            lblStatus.ForeColor =
            System.Drawing.Color.Gray;




            pnlCard.Controls.Add(lblTitle);

            pnlCard.Controls.Add(lblUser);
            pnlCard.Controls.Add(lblIP);
            pnlCard.Controls.Add(lblPort);


            pnlCard.Controls.Add(txtUsername);
            pnlCard.Controls.Add(txtIP);
            pnlCard.Controls.Add(txtPort);


            pnlCard.Controls.Add(btnLogin);
            pnlCard.Controls.Add(lblStatus);




            this.Controls.Add(pnlLeft);
            this.Controls.Add(pnlCard);



            this.Load +=
            new System.EventHandler(
            this.FrmLogin_Load);
        }
    }
}