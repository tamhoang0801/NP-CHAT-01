using System;
using System.Net.Sockets;

namespace ChatServerApp
{
    /// <summary>
    /// Thông tin người dùng đang kết nối.
    /// Lưu trữ thông tin cơ bản và chuẩn bị sẵn cấu trúc cho các mở rộng sau này.
    /// </summary>
    public class UserInfo
    {
        /// <summary>Tên đăng nhập của user.</summary>
        public string Username { get; set; }

        /// <summary>Kết nối TCP tới client.</summary>
        public TcpClient Client { get; set; }

        /// <summary>Thời điểm user đăng nhập vào hệ thống.</summary>
        public DateTime LoginTime { get; set; }

        /// <summary>User luôn Online khi đang trong danh sách quản lý.</summary>
        public bool IsOnline => true;

        // ===== Các trường dự trữ cho mở rộng sau này =====

        /// <summary>Đường dẫn avatar (dùng sau).</summary>
        public string Avatar { get; set; }

        /// <summary>Trạng thái tuỳ chỉnh (dùng sau).</summary>
        public string Status { get; set; }

        public UserInfo(string username, TcpClient client)
        {
            Username = username;
            Client = client;
            LoginTime = DateTime.Now;
            Avatar = null;
            Status = null;
        }
    }
}
