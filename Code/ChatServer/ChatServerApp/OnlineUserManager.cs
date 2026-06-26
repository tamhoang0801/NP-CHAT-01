using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ChatServerApp
{
    /// <summary>
    /// Quản lý danh sách người dùng online ở phía Server.
    /// Cung cấp các phương thức thêm, xoá, broadcast cập nhật danh sách.
    /// </summary>
    public class OnlineUserManager
    {
        private readonly ConcurrentDictionary<string, UserInfo> _users =
            new ConcurrentDictionary<string, UserInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Số lượng người dùng đang online.</summary>
        public int Count => _users.Count;

        /// <summary>Danh sách tên tất cả user đang online.</summary>
        public IEnumerable<string> Usernames => _users.Keys;

        /// <summary>Danh sách tất cả UserInfo đang online.</summary>
        public IEnumerable<UserInfo> AllUsers => _users.Values;

        /// <summary>
        /// Thêm user mới vào danh sách online.
        /// Trả về true nếu thêm thành công, false nếu tên đã tồn tại.
        /// </summary>
        public bool TryAdd(string username, TcpClient client, out UserInfo userInfo)
        {
            userInfo = new UserInfo(username, client);
            return _users.TryAdd(username, userInfo);
        }

        /// <summary>
        /// Xoá user khỏi danh sách online theo username.
        /// Trả về UserInfo đã bị xoá, null nếu không tìm thấy.
        /// </summary>
        public UserInfo TryRemove(string username)
        {
            if (_users.TryRemove(username, out var removed))
                return removed;
            return null;
        }

        /// <summary>
        /// Kiểm tra username đã tồn tại trong danh sách online chưa.
        /// </summary>
        public bool Contains(string username)
        {
            return _users.ContainsKey(username);
        }

        /// <summary>
        /// Lấy thông tin user theo username.
        /// </summary>
        public UserInfo GetByUsername(string username)
        {
            _users.TryGetValue(username, out var user);
            return user;
        }

        /// <summary>
        /// Tạo chuỗi danh sách tên user, cách nhau bởi dấu phẩy, để gửi qua giao thức.
        /// </summary>
        public string BuildOnlineListString()
        {
            return string.Join(",", _users.Keys);
        }

        /// <summary>
        /// Xoá tất cả user (khi server dừng).
        /// </summary>
        public void Clear()
        {
            _users.Clear();
        }
    }
}
