namespace ChatApp
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null!;

        private System.Windows.Forms.Panel      pnlLeft = null!;
        private System.Windows.Forms.Panel      pnlRight = null!;
        private System.Windows.Forms.Label      lblAppTitle = null!;
        private System.Windows.Forms.Label      lblAppSubtitle = null!;
        private System.Windows.Forms.Label      lblWelcome = null!;
        private System.Windows.Forms.Label      lblUsernameHint = null!;
        private System.Windows.Forms.Label      lblIPHint = null!;
        private System.Windows.Forms.Label      lblPortHint = null!;
        private System.Windows.Forms.TextBox    txtUsername = null!;
        private System.Windows.Forms.TextBox    txtIP = null!;
        private System.Windows.Forms.TextBox    txtPort = null!;
        private System.Windows.Forms.Button     btnLogin = null!;
        private System.Windows.Forms.Label      lblStatus = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlLeft         = new System.Windows.Forms.Panel();
            this.pnlRight        = new System.Windows.Forms.Panel();
            this.lblAppTitle     = new System.Windows.Forms.Label();
            this.lblAppSubtitle  = new System.Windows.Forms.Label();
            this.lblWelcome      = new System.Windows.Forms.Label();
            this.lblUsernameHint = new System.Windows.Forms.Label();
            this.lblIPHint       = new System.Windows.Forms.Label();
            this.lblPortHint     = new System.Windows.Forms.Label();
            this.txtUsername     = new System.Windows.Forms.TextBox();
            this.txtIP           = new System.Windows.Forms.TextBox();
            this.txtPort         = new System.Windows.Forms.TextBox();
            this.btnLogin        = new System.Windows.Forms.Button();
            this.lblStatus       = new System.Windows.Forms.Label();

            this.Text            = "Chat App - Dang Nhap";
            this.Size            = new System.Drawing.Size(620, 420);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = System.Drawing.Color.FromArgb(245, 245, 245);
            this.Font            = new System.Drawing.Font("Segoe UI", 9.5f);
            this.Load           += new System.EventHandler(this.FrmLogin_Load);

            this.pnlLeft.Size      = new System.Drawing.Size(230, 420);
            this.pnlLeft.Location  = new System.Drawing.Point(0, 0);
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(230, 235, 245);

            this.lblAppTitle.Text      = "Chat";
            this.lblAppTitle.Font      = new System.Drawing.Font("Segoe UI", 26f, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = System.Drawing.Color.FromArgb(80, 100, 150);
            this.lblAppTitle.Size      = new System.Drawing.Size(210, 70);
            this.lblAppTitle.Location  = new System.Drawing.Point(10, 120);
            this.lblAppTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblAppSubtitle.Text      = "TCP Chat App\nKet Noi - Chia Se - Trao Doi";
            this.lblAppSubtitle.Font      = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
            this.lblAppSubtitle.ForeColor = System.Drawing.Color.FromArgb(70, 90, 130);
            this.lblAppSubtitle.Size      = new System.Drawing.Size(210, 70);
            this.lblAppSubtitle.Location  = new System.Drawing.Point(10, 200);
            this.lblAppSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.pnlLeft.Controls.Add(this.lblAppTitle);
            this.pnlLeft.Controls.Add(this.lblAppSubtitle);

            this.pnlRight.Size      = new System.Drawing.Size(390, 420);
            this.pnlRight.Location  = new System.Drawing.Point(230, 0);
            this.pnlRight.BackColor = System.Drawing.Color.White;

            this.lblWelcome.Text      = "Dang Nhap";
            this.lblWelcome.Font      = new System.Drawing.Font("Segoe UI", 20f, System.Drawing.FontStyle.Bold);
            this.lblWelcome.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.lblWelcome.Size      = new System.Drawing.Size(330, 45);
            this.lblWelcome.Location  = new System.Drawing.Point(30, 40);

            this.lblUsernameHint.Text      = "TEN NGUOI DUNG";
            this.lblUsernameHint.Font      = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold);
            this.lblUsernameHint.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblUsernameHint.Size      = new System.Drawing.Size(330, 18);
            this.lblUsernameHint.Location  = new System.Drawing.Point(30, 108);

            this.txtUsername.Size      = new System.Drawing.Size(330, 28);
            this.txtUsername.Location  = new System.Drawing.Point(30, 128);
            StyleTxt(this.txtUsername);
            this.txtUsername.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            this.txtUsername.Enter    += new System.EventHandler(this.TextBox_Enter);
            this.txtUsername.Leave    += new System.EventHandler(this.TextBox_Leave);

            this.lblIPHint.Text      = "DIA CHI IP SERVER";
            this.lblIPHint.Font      = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold);
            this.lblIPHint.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblIPHint.Size      = new System.Drawing.Size(180, 18);
            this.lblIPHint.Location  = new System.Drawing.Point(30, 174);

            this.txtIP.Size      = new System.Drawing.Size(185, 28);
            this.txtIP.Location  = new System.Drawing.Point(30, 194);
            StyleTxt(this.txtIP);
            this.txtIP.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            this.txtIP.Enter    += new System.EventHandler(this.TextBox_Enter);
            this.txtIP.Leave    += new System.EventHandler(this.TextBox_Leave);

            this.lblPortHint.Text      = "PORT";
            this.lblPortHint.Font      = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold);
            this.lblPortHint.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblPortHint.Size      = new System.Drawing.Size(120, 18);
            this.lblPortHint.Location  = new System.Drawing.Point(228, 174);

            this.txtPort.Size      = new System.Drawing.Size(132, 28);
            this.txtPort.Location  = new System.Drawing.Point(228, 194);
            StyleTxt(this.txtPort);
            this.txtPort.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            this.txtPort.Enter    += new System.EventHandler(this.TextBox_Enter);
            this.txtPort.Leave    += new System.EventHandler(this.TextBox_Leave);

            this.btnLogin.Text      = "DANG NHAP";
            this.btnLogin.Size      = new System.Drawing.Size(330, 40);
            this.btnLogin.Location  = new System.Drawing.Point(30, 248);
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnLogin.FlatAppearance.BorderSize  = 1;
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            this.btnLogin.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnLogin.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this.btnLogin.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.Click    += new System.EventHandler(this.btnLogin_Click);
            this.btnLogin.MouseEnter += (s, e) => this.btnLogin.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnLogin.MouseLeave += (s, e) => this.btnLogin.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);

            this.lblStatus.Size      = new System.Drawing.Size(330, 24);
            this.lblStatus.Location  = new System.Drawing.Point(30, 302);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 9.5f);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.pnlRight.Controls.Add(this.lblWelcome);
            this.pnlRight.Controls.Add(this.lblUsernameHint);
            this.pnlRight.Controls.Add(this.txtUsername);
            this.pnlRight.Controls.Add(this.lblIPHint);
            this.pnlRight.Controls.Add(this.txtIP);
            this.pnlRight.Controls.Add(this.lblPortHint);
            this.pnlRight.Controls.Add(this.txtPort);
            this.pnlRight.Controls.Add(this.btnLogin);
            this.pnlRight.Controls.Add(this.lblStatus);

            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlRight);
        }

        private static void StyleTxt(System.Windows.Forms.TextBox tb)
        {
            tb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            tb.BackColor   = System.Drawing.Color.White;
            tb.ForeColor   = System.Drawing.Color.FromArgb(50, 50, 50);
            tb.Font        = new System.Drawing.Font("Segoe UI", 10.5f);
        }
    }
}
