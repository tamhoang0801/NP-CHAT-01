using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>Bảng chọn emoji để chèn vào ô nhập tin nhắn.</summary>
    public class EmojiPickerForm : Form
    {
        private readonly FlowLayoutPanel _flowPanel;
        private readonly string[] _emojis;
        private readonly TextBox _targetTextBox;
        private readonly int _buttonSize = 36;

        public EmojiPickerForm(TextBox targetTextBox)
        {
            _targetTextBox = targetTextBox;

            _emojis = new string[]
            {
                "😀","😁","😂","🤣","😃","😄","😅","😆","😉","😊",
                "😋","😎","😍","🥰","😘","🙂","🤗","🤔","😐","😑",
                "🙄","😏","😴","😌","😜","😒","😔","🙃","😲","😭",
                "😱","😡","😠","👍","👎","👊","👏","🙌","🙏","👌",
                "❤️","💔","💖","💙","💚","💛","💜","💯","🔥","✨",
                "⭐","🎉","🎊","🎈","🎁","🏆","✅","❌","❓","❗",
                "⚠️","🔴","🟢","🔵","🟡","💬","📌","🚀","🏠","🌈",
                "☀️","🌙","💧","🍕","🍔","🍟","🍰","🎂","☕","🍺",
                "🎵","🎶","🎤","🎧","🎮","👻","💀","👽"
            };

            this.Text = "Chọn Emoji";
            this.Size = new Size(380, 344);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.White;

            _flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(4),
                BackColor = Color.White
            };

            foreach (string emoji in _emojis)
            {
                var btn = new Button
                {
                    Text = emoji,
                    Size = new Size(_buttonSize, _buttonSize),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Emoji", 14F, FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    BackColor = Color.White,
                    TabStop = false
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(240, 240, 245);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.White;
                btn.Click += (s, e) => { InsertEmoji(emoji); };
                _flowPanel.Controls.Add(btn);
            }

            // Thanh dưới cùng chứa nút Đóng
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.White,
                Padding = new Padding(8)
            };

            var btnClose = new Button
            {
                Text = "Đóng",
                Dock = DockStyle.Right,
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnClose.Click += (s, e) => this.Close();
            pnlBottom.Controls.Add(btnClose);

            // Thêm _flowPanel TRƯỚC, pnlBottom SAU (để nút Đóng nằm đúng dưới cùng)
            this.Controls.Add(_flowPanel);
            this.Controls.Add(pnlBottom);

            // Nhấn Esc cũng đóng được
            this.CancelButton = btnClose;
        }

        private void InsertEmoji(string emoji)
        {
            int selStart = _targetTextBox.SelectionStart;
            string text = _targetTextBox.Text;
            _targetTextBox.Text = text.Substring(0, selStart) + emoji + text.Substring(selStart);
            _targetTextBox.SelectionStart = selStart + emoji.Length;
            _targetTextBox.Focus();
        }
    }
}