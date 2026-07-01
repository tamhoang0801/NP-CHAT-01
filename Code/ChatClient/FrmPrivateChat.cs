using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>Cửa sổ chat riêng 1-1: emoji + avatar + gửi ảnh/video riêng.</summary>
    public class FrmPrivateChat : Form
    {
        private readonly string _myName;
        private readonly string _partner;
        private readonly NetworkStream _stream;
        private readonly RichTextBox _rtb;
        private readonly TextBox _txtInput;
        private readonly Button _btnSend;
        private readonly Button _btnEmoji;
        private readonly Button _btnImg;
        private readonly Button _btnVid;

        public string Partner => _partner;

        public FrmPrivateChat(string myName, string partner, NetworkStream stream, Image myAvatar, Image partnerAvatar)
        {
            _myName = myName;
            _partner = partner;
            _stream = stream;

            this.Text = "Chat riêng với " + partner;
            this.Size = new Size(460, 540);
            this.StartPosition = FormStartPosition.CenterParent;

            // Header: avatar + tên
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(245, 245, 245) };
            var picPartner = new PictureBox { Size = new Size(38, 38), Location = new Point(8, 6), SizeMode = PictureBoxSizeMode.StretchImage };
            if (partnerAvatar != null)
                picPartner.Image = AvatarHelper.MakeCircular(partnerAvatar, 38);
            var lblPartner = new Label { Text = partner, Location = new Point(54, 15), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            pnlHeader.Controls.Add(picPartner);
            pnlHeader.Controls.Add(lblPartner);

            _rtb = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White, Font = new Font("Segoe UI Emoji", 10F) };

            // Thanh nhập: [input] [📷] [🎬] [😊] [Gửi]
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            _btnSend = new Button { Text = "Gửi", Dock = DockStyle.Right, Width = 66 };
            _btnEmoji = new Button { Text = "😊", Dock = DockStyle.Right, Width = 42, Font = new Font("Segoe UI Emoji", 12F) };
            _btnVid = new Button { Text = "🎬", Dock = DockStyle.Right, Width = 42, Font = new Font("Segoe UI Emoji", 12F) };
            _btnImg = new Button { Text = "📷", Dock = DockStyle.Right, Width = 42, Font = new Font("Segoe UI Emoji", 12F) };
            _txtInput = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI Emoji", 10F) };
            pnlBottom.Controls.Add(_txtInput);
            pnlBottom.Controls.Add(_btnImg);
            pnlBottom.Controls.Add(_btnVid);
            pnlBottom.Controls.Add(_btnEmoji);
            pnlBottom.Controls.Add(_btnSend);

            this.Controls.Add(_rtb);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlBottom);

            _btnSend.Click += (s, e) => SendMessage();
            _btnEmoji.Click += (s, e) => { var p = new EmojiPickerForm(_txtInput); p.StartPosition = FormStartPosition.CenterParent; p.Show(this); };
            _btnImg.Click += (s, e) => SendPrivateFile(0x20, "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp", "Ảnh");
            _btnVid.Click += (s, e) => SendPrivateFile(0x21, "Video Files|*.mp4;*.avi;*.mkv", "Video");
            _txtInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { SendMessage(); e.SuppressKeyPress = true; } };
        }

        private void SendMessage()
        {
            string content = _txtInput.Text.Trim();
            if (content == "") return;
            try
            {
                byte[] data = Encoding.UTF8.GetBytes($"PRIVATE|{_myName}|{_partner}|{content}");
                _stream.Write(data, 0, data.Length);
                AppendLine("Tôi", content, Color.Blue);
                _txtInput.Clear();
                _txtInput.Focus();
            }
            catch { AppendLine("[Lỗi]", "Không gửi được tin.", Color.Red); }
        }

        private void SendPrivateFile(byte marker, string filter, string typeStr)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = filter })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    byte[] fileData = File.ReadAllBytes(ofd.FileName);
                    byte[] recvBytes = Encoding.UTF8.GetBytes(_partner);
                    byte[] packet = new byte[10 + recvBytes.Length + fileData.Length];
                    packet[0] = marker;
                    Array.Copy(BitConverter.GetBytes((long)fileData.Length), 0, packet, 1, 8);
                    packet[9] = (byte)recvBytes.Length;
                    Array.Copy(recvBytes, 0, packet, 10, recvBytes.Length);
                    Array.Copy(fileData, 0, packet, 10 + recvBytes.Length, fileData.Length);
                    _stream.Write(packet, 0, packet.Length);
                    AppendLine("Tôi", $"[Đã gửi 1 {typeStr}]", Color.Blue);
                }
                catch (Exception ex) { AppendLine("[Lỗi]", "Không gửi được file: " + ex.Message, Color.Red); }
            }
        }

        public void ReceiveMessage(string content)
        {
            AppendLine(_partner, content, Color.Green);
        }

        private void AppendLine(string who, string content, Color color)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => AppendLine(who, content, color))); return; }
            _rtb.SelectionStart = _rtb.TextLength;
            _rtb.SelectionColor = color;
            _rtb.AppendText($"{who} [{DateTime.Now:HH:mm}]: {content}\n");
            _rtb.ScrollToCaret();
        }
    }
}