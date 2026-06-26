using System;
using System.Collections.Generic;

namespace ChatApp
{
    /// <summary>
    /// Quản lý một phiên chat riêng biệt, lưu lịch sử tin nhắn.
    /// Hỗ trợ cả chat công khai (phòng) và chat riêng tư (private).
    /// </summary>
    public class ChatSession
    {
        private static int _nextId = 1;

        public int Id { get; private set; }
        public string Name { get; set; }
        public List<string> Messages { get; private set; }
        public string? RtfContent { get; set; }

        /// <summary>
        /// Đánh dấu đây có phải là phiên chat riêng tư hay không.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Nếu là chat riêng, lưu tên người nhận để gửi tin nhắn.
        /// </summary>
        public string? PrivateChatPartner { get; set; }

        /// <summary>Số tin nhắn chưa đọc trong session này.</summary>
        public int UnreadCount { get; set; }

        public ChatSession(string name)
        {
            Id = _nextId++;
            Name = name;
            Messages = new List<string>();
            RtfContent = null;
            IsPrivate = false;
            PrivateChatPartner = null;
            UnreadCount = 0;
        }

        public void AddMessage(string formattedMessage)
        {
            Messages.Add(formattedMessage);
        }

        /// <summary>
        /// Reset số tin nhắn chưa đọc về 0.
        /// </summary>
        public void MarkAsRead()
        {
            UnreadCount = 0;
        }
    }
}
