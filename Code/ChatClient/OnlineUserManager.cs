using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatApp
{
    /// <summary>
    /// Quản lý danh sách người dùng online ở phía Client.
    /// Đồng bộ từ dữ liệu nhận được từ Server qua giao thức UPDATE_ONLINE.
    /// </summary>
    public class OnlineUserManager
    {
        private readonly Dictionary<string, UserInfo> _users =
            new Dictionary<string, UserInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Số lượng user đang online.</summary>
        public int Count => _users.Count;

        /// <summary>Danh sách tất cả UserInfo.</summary>
        public IEnumerable<UserInfo> AllUsers => _users.Values;

        /// <summary>Danh sách tên tất cả user.</summary>
        public IEnumerable<string> Usernames => _users.Keys;

        /// <summary>Sự kiện khi danh sách online thay đổi.</summary>
        public event Action? OnlineListChanged;

        /// <summary>
        /// Đồng bộ danh sách online từ chuỗi dữ liệu nhận từ Server.
        /// Format: user1:HH:mm:ss,user2:HH:mm:ss hoặc user1,user2
        /// Các user cũ không còn trong danh sách sẽ bị xoá.
        /// Các user mới sẽ được thêm vào.
        /// </summary>
        public void SyncFromServer(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
            {
                _users.Clear();
                OnlineListChanged?.Invoke();
                return;
            }

            var entries = rawData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var newSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Xử lý từng entry có thể có hoặc không có thời gian
            foreach (var entry in entries)
            {
                var parts = entry.Split(':');
                string username = parts[0].Trim();
                if (string.IsNullOrEmpty(username)) continue;

                newSet.Add(username);

                if (_users.TryGetValue(username, out var existing))
                {
                    // Cập nhật thời gian đăng nhập nếu có (parts.Length >= 4: user:HH:mm:ss)
                    if (parts.Length >= 4 && TimeSpan.TryParse($"{parts[1]}:{parts[2]}:{parts[3]}", out var time))
                    {
                        var today = DateTime.Today;
                        existing.LoginTime = today.Add(time);
                    }
                }
                else
                {
                    var newUser = new UserInfo(username);
                    if (parts.Length >= 4 && TimeSpan.TryParse($"{parts[1]}:{parts[2]}:{parts[3]}", out var loginTime))
                    {
                        var today = DateTime.Today;
                        newUser.LoginTime = today.Add(loginTime);
                    }
                    _users[username] = newUser;
                }
            }

            // Xoá user không còn online
            var toRemove = _users.Keys.Where(u => !newSet.Contains(u)).ToList();
            foreach (var user in toRemove)
                _users.Remove(user);

            OnlineListChanged?.Invoke();
        }

        /// <summary>Kiểm tra user có online không.</summary>
        public bool IsOnline(string username)
        {
            return _users.ContainsKey(username);
        }

        /// <summary>Lấy UserInfo theo username.</summary>
        public UserInfo? GetByUsername(string username)
        {
            _users.TryGetValue(username, out var user);
            return user;
        }

        /// <summary>Xoá tất cả user.</summary>
        public void Clear()
        {
            _users.Clear();
            OnlineListChanged?.Invoke();
        }
    }
}
