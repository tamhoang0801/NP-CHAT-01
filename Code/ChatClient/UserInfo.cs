using System;

namespace ChatApp
{
    /// <summary>
    /// Thông tin người dùng hiển thị ở Client.
    /// Được đồng bộ từ Server qua giao thức UPDATE_ONLINE.
    /// </summary>
    public class UserInfo
    {
        /// <summary>Tên đăng nhập của user.</summary>
        public string Username { get; set; }

        /// <summary>User luôn Online khi nhận được từ Server.</summary>
        public bool IsOnline => true;

        /// <summary>Thời gian đăng nhập (để sau này mở rộng).</summary>
        public DateTime LoginTime { get; set; }

        // ===== Các trường dự trữ cho mở rộng sau này =====

        /// <summary>Đường dẫn avatar (dùng sau).</summary>
        public string? Avatar { get; set; }

        /// <summary>Trạng thái tuỳ chỉnh (dùng sau).</summary>
        public string? Status { get; set; }

        public UserInfo(string username)
        {
            Username = username;
            LoginTime = DateTime.Now;
        }
    }
}
