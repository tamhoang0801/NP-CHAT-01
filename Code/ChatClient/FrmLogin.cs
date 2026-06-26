using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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

            // Chạy Connect + LOGIN handshake trên luồng nền để không treo UI
            Task.Run(() =>
            {
                TcpClient? client = null;
                NetworkStream? stream = null;
                try
                {
                    client = new TcpClient();
                    client.ReceiveTimeout = 5000;
                    client.SendTimeout = 5000;

                    // Connect với timeout thủ công 5s
                    var connectTask = client.ConnectAsync(ip, port);
                    if (!connectTask.Wait(TimeSpan.FromSeconds(5)))
                    {
                        this.Invoke(new Action(() =>
                        {
                            ShowError("Ket noi qua thoi gian (5s).");
                            SetConnectingState(false);
                        }));
                        return;
                    }

                    stream = client.GetStream();

                    // Gửi LOGIN kèm \n để server tách đúng dòng
                    string loginMsg = $"LOGIN|{username}\n";
                    byte[] data = Encoding.UTF8.GetBytes(loginMsg);
                    stream.Write(data, 0, data.Length);

                    // Đọc chính xác 1 dòng phản hồi (LOGIN_OK hoặc ERROR|...), 
                    // không nuốt các gói tin sau (vd UPDATE_ONLINE bị gộp)
                    string? response = ChatHelper.ReadLineUtf8(stream);

                    if (response != null)
                    {
                        if (response.StartsWith("ERROR|"))
                        {
                            string errorMsg = response.Length > 6 ? response.Substring(6) : "Dang nhap that bai!";
                            this.Invoke(new Action(() => { ShowError(errorMsg); SetConnectingState(false); }));
                            client.Close();
                            return;
                        }

                        if (response.StartsWith("LOGIN_OK"))
                        {
                            TcpClient connectedClient = client;
                            this.Invoke(new Action(() =>
                            {
                                lblStatus.Text = "Đã kết nối thành công!";
                                lblStatus.ForeColor = Color.FromArgb(60, 160, 80);

                                FrmChat chatForm = new FrmChat(username, connectedClient);
                                chatForm.StartPosition = FormStartPosition.Manual;
                                chatForm.Location = new Point(this.Location.X + 30, this.Location.Y + 30);
                                chatForm.Show();

                                // Reset login form for next user
                                txtUsername.Clear();
                                txtUsername.Focus();
                                lblStatus.Text = "Chua ket noi";
                                lblStatus.ForeColor = Color.FromArgb(150, 150, 150);
                                SetConnectingState(false);
                            }));
                            return;
                        }
                    }

                    // Phản hồi không hợp lệ
                    this.Invoke(new Action(() => { ShowError("Phan hoi tu server khong hop le!"); SetConnectingState(false); }));
                    client.Close();
                }
                catch (SocketException)
                {
                    this.Invoke(new Action(() => { ShowError("Khong the ket noi. Kiem tra Server da bat chua!"); SetConnectingState(false); }));
                    if (client != null) client.Close();
                }
                catch (Exception)
                {
                    this.Invoke(new Action(() => { ShowError("Loi ket noi. Kiem tra Server da bat chua!"); SetConnectingState(false); }));
                    if (client != null) client.Close();
                }
            });
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
            TextBox? tb = sender as TextBox;
            if (tb != null)
                tb.BackColor = Color.FromArgb(245, 248, 255);
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox? tb = sender as TextBox;
            if (tb != null)
                tb.BackColor = Color.White;
        }
    }
}
