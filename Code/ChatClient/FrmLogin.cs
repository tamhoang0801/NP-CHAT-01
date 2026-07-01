using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            txtIP.Text = "127.0.0.1";
            txtPort.Text = "9999";
            lblStatus.Text = "Chua ket noi";
            lblStatus.ForeColor = Color.FromArgb(150, 150, 150);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string ip = txtIP.Text.Trim();
            string portText = txtPort.Text.Trim();
            int port;

            if (username == "")
            {
                ShowError("Vui long nhap ten nguoi dung!");
                txtUsername.Focus();
                return;
            }

            if (ip == "")
            {
                ShowError("Vui long nhap dia chi IP Server!");
                txtIP.Focus();
                return;
            }

            if (portText == "" || !int.TryParse(portText, out port) || port < 1 || port > 65535)
            {
                ShowError("Port khong hop le! (1-65535)");
                txtPort.Focus();
                return;
            }

            SetConnectingState(true);

            try
            {

                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                client.Connect(ip, port);
                System.Net.Sockets.NetworkStream stream = client.GetStream();

                string loginMsg = $"LOGIN|{username}";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(loginMsg);
                stream.Write(data, 0, data.Length);

                lblStatus.Text = "Đã kết nối thành công!";
                lblStatus.ForeColor = Color.FromArgb(60, 160, 80);

                FrmChat chatForm = new FrmChat(username, client);
                chatForm.Show();
                this.Hide();
            }
            catch (Exception)
            {
                ShowError("Không thể kết nối. Hãy kiểm tra xem Server đã bật chưa!");
                SetConnectingState(false);
            }
        }

        private void ShowError(string message)
        {
            lblStatus.Text = "Loi: " + message;
            lblStatus.ForeColor = Color.FromArgb(200, 60, 60);
        }

        private void SetConnectingState(bool isConnecting)
        {
            btnLogin.Enabled = !isConnecting;
            txtUsername.Enabled = !isConnecting;
            txtIP.Enabled = !isConnecting;
            txtPort.Enabled = !isConnecting;

            if (isConnecting)
            {
                lblStatus.Text = "Dang ket noi...";
                lblStatus.ForeColor = Color.FromArgb(180, 130, 30);
            }
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin_Click(sender, e);
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
                tb.BackColor = Color.FromArgb(245, 248, 255);
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
                tb.BackColor = Color.White;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản muốn đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                TcpClient tempClient = new TcpClient("127.0.0.1", 9999); // Thay port đúng với của bạn
                NetworkStream stream = tempClient.GetStream();

                // Gửi lệnh Đăng ký
                string msg = $"REGISTER|{username}\n";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);

                // Chờ Server trả lời
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                if (response == "REGISTER_OK")
                {
                    MessageBox.Show("Đăng ký tài khoản thành công! Bây giờ bạn có thể đăng nhập.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (response.StartsWith("ERROR|"))
                {
                    string errorMsg = response.Substring(6); // Bỏ chữ "ERROR|" đi
                    MessageBox.Show(errorMsg, "Lỗi đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                tempClient.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối đến Server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
