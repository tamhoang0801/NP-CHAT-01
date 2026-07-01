namespace ChatApp
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel      pnlLeft;
        private System.Windows.Forms.Panel      pnlRight;
        private System.Windows.Forms.Label      lblAppTitle;
        private System.Windows.Forms.Label      lblAppSubtitle;
        private System.Windows.Forms.Label      lblWelcome;
        private System.Windows.Forms.Label      lblUsernameHint;
        private System.Windows.Forms.Label      lblIPHint;
        private System.Windows.Forms.Label      lblPortHint;
        private System.Windows.Forms.TextBox    txtUsername;
        private System.Windows.Forms.TextBox    txtIP;
        private System.Windows.Forms.TextBox    txtPort;
        private System.Windows.Forms.Button     btnLogin;
        private System.Windows.Forms.Label      lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlLeft = new Panel();
            lblAppTitle = new Label();
            lblAppSubtitle = new Label();
            pnlRight = new Panel();
            btnRegister = new Button();
            lblWelcome = new Label();
            lblUsernameHint = new Label();
            txtUsername = new TextBox();
            lblIPHint = new Label();
            txtIP = new TextBox();
            lblPortHint = new Label();
            txtPort = new TextBox();
            btnLogin = new Button();
            lblStatus = new Label();
            pnlLeft.SuspendLayout();
            pnlRight.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.FromArgb(230, 235, 245);
            pnlLeft.Controls.Add(lblAppTitle);
            pnlLeft.Controls.Add(lblAppSubtitle);
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(230, 420);
            pnlLeft.TabIndex = 0;
            // 
            // lblAppTitle
            // 
            lblAppTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblAppTitle.ForeColor = Color.FromArgb(80, 100, 150);
            lblAppTitle.Location = new Point(10, 120);
            lblAppTitle.Name = "lblAppTitle";
            lblAppTitle.Size = new Size(210, 70);
            lblAppTitle.TabIndex = 0;
            lblAppTitle.Text = "Chat";
            lblAppTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblAppSubtitle
            // 
            lblAppSubtitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblAppSubtitle.ForeColor = Color.FromArgb(70, 90, 130);
            lblAppSubtitle.Location = new Point(10, 200);
            lblAppSubtitle.Name = "lblAppSubtitle";
            lblAppSubtitle.Size = new Size(210, 70);
            lblAppSubtitle.TabIndex = 1;
            lblAppSubtitle.Text = "TCP Chat App\nKet Noi - Chia Se - Trao Doi";
            lblAppSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.White;
            pnlRight.Controls.Add(btnRegister);
            pnlRight.Controls.Add(lblWelcome);
            pnlRight.Controls.Add(lblUsernameHint);
            pnlRight.Controls.Add(txtUsername);
            pnlRight.Controls.Add(lblIPHint);
            pnlRight.Controls.Add(txtIP);
            pnlRight.Controls.Add(lblPortHint);
            pnlRight.Controls.Add(txtPort);
            pnlRight.Controls.Add(btnLogin);
            pnlRight.Controls.Add(lblStatus);
            pnlRight.Location = new Point(230, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(390, 420);
            pnlRight.TabIndex = 1;
            // 
            // btnRegister
            // 
            btnRegister.BackColor = Color.FromArgb(224, 224, 224);
            btnRegister.Cursor = Cursors.Hand;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRegister.ForeColor = Color.FromArgb(50, 50, 50);
            btnRegister.Location = new Point(202, 250);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(146, 35);
            btnRegister.TabIndex = 9;
            btnRegister.Text = "DANG KY";
            btnRegister.UseVisualStyleBackColor = false;
            btnRegister.Click += button1_Click;
            // 
            // lblWelcome
            // 
            lblWelcome.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(50, 50, 50);
            lblWelcome.Location = new Point(30, 40);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(330, 45);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Dang Nhap";
            // 
            // lblUsernameHint
            // 
            lblUsernameHint.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblUsernameHint.ForeColor = Color.FromArgb(120, 120, 120);
            lblUsernameHint.Location = new Point(30, 108);
            lblUsernameHint.Name = "lblUsernameHint";
            lblUsernameHint.Size = new Size(330, 18);
            lblUsernameHint.TabIndex = 1;
            lblUsernameHint.Text = "TEN NGUOI DUNG";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(30, 128);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(330, 24);
            txtUsername.TabIndex = 2;
            txtUsername.Enter += TextBox_Enter;
            txtUsername.KeyDown += txtInput_KeyDown;
            txtUsername.Leave += TextBox_Leave;
            // 
            // lblIPHint
            // 
            lblIPHint.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblIPHint.ForeColor = Color.FromArgb(120, 120, 120);
            lblIPHint.Location = new Point(30, 174);
            lblIPHint.Name = "lblIPHint";
            lblIPHint.Size = new Size(180, 18);
            lblIPHint.TabIndex = 3;
            lblIPHint.Text = "DIA CHI IP SERVER";
            // 
            // txtIP
            // 
            txtIP.Location = new Point(30, 194);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(161, 24);
            txtIP.TabIndex = 4;
            txtIP.Enter += TextBox_Enter;
            txtIP.KeyDown += txtInput_KeyDown;
            txtIP.Leave += TextBox_Leave;
            // 
            // lblPortHint
            // 
            lblPortHint.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblPortHint.ForeColor = Color.FromArgb(120, 120, 120);
            lblPortHint.Location = new Point(228, 174);
            lblPortHint.Name = "lblPortHint";
            lblPortHint.Size = new Size(120, 18);
            lblPortHint.TabIndex = 5;
            lblPortHint.Text = "PORT";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(197, 194);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(163, 24);
            txtPort.TabIndex = 6;
            txtPort.Enter += TextBox_Enter;
            txtPort.KeyDown += txtInput_KeyDown;
            txtPort.Leave += TextBox_Leave;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(224, 224, 224);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.FlatAppearance.BorderColor = Color.Black;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogin.ForeColor = Color.FromArgb(50, 50, 50);
            btnLogin.Location = new Point(40, 250);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(146, 35);
            btnLogin.TabIndex = 9;
            btnLogin.Text = "DANG NHAP";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 9.5F);
            lblStatus.ForeColor = Color.FromArgb(130, 130, 130);
            lblStatus.Location = new Point(30, 302);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(330, 24);
            lblStatus.TabIndex = 8;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FrmLogin
            // 
            BackColor = Color.FromArgb(245, 245, 245);
            ClientSize = new Size(604, 381);
            Controls.Add(pnlLeft);
            Controls.Add(pnlRight);
            Font = new Font("Segoe UI", 9.5F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FrmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chat App - Dang Nhap";
            Load += FrmLogin_Load;
            pnlLeft.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            pnlRight.PerformLayout();
            ResumeLayout(false);
        }

        private static void StyleTxt(System.Windows.Forms.TextBox tb)
        {
            tb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            tb.BackColor   = System.Drawing.Color.White;
            tb.ForeColor   = System.Drawing.Color.FromArgb(50, 50, 50);
            tb.Font        = new System.Drawing.Font("Segoe UI", 10.5f);
        }

        private Button btnRegister;
    }
}
