using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class FrmLogin : Form
    {
        private string selectedAvatarPath = "";

        public static string SelectedAvatarPath { get; set; }

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
            SelectedAvatarPath = "";
            selectedAvatarPath = "";
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

            // Nhắc người dùng chọn avatar
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Tệp hình ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Tất cả tệp (*.*)|*.*";
            openFileDialog.Title = "Chọn avatar (hình đại diện)";
            openFileDialog.CheckFileExists = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedAvatarPath = openFileDialog.FileName;
                SelectedAvatarPath = selectedAvatarPath;

                // Kiểm tra kích thước file (tối đa 5MB)
                FileInfo fileInfo = new FileInfo(selectedAvatarPath);
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    ShowError("File avatar quá lớn! Tối đa 5MB.");
                    return;
                }

                // Kiểm tra định dạng file
                string ext = Path.GetExtension(selectedAvatarPath).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".bmp")
                {
                    ShowError("Định dạng file không hỗ trợ! Vui lòng chọn JPG, PNG hoặc BMP.");
                    return;
                }
            }
            else
            {
                ShowError("Vui lòng chọn một file avatar!");
                return;
            }

            SetConnectingState(true);

            try
            {
                // Kết nối tới server
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                client.Connect(ip, port);

                // Gửi lệnh đăng nhập lên Server
                System.Net.Sockets.NetworkStream stream = client.GetStream();
                string loginMsg = $"LOGIN|{username}";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(loginMsg);
                stream.Write(data, 0, data.Length);

                lblStatus.Text = "Đã kết nối thành công!";
                lblStatus.ForeColor = Color.FromArgb(60, 160, 80);

                // Mở Form Chat và truyền kết nối tới Form Chat
                FrmChat chatForm = new FrmChat(username, client);
                chatForm.Show();
                this.Hide();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                ShowError($"Không thể kết nối. Hãy kiểm tra xem Server đã bật chưa! ({ex.Message})");
                SetConnectingState(false);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
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
    }
}
