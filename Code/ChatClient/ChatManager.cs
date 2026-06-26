using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>
    /// Quản lý danh sách các cuộc trò chuyện (multi-chat sessions).
    /// Hỗ trợ cả phòng chat công khai và chat riêng tư.
    /// </summary>
    public class ChatManager
    {
        private readonly List<ChatSession> _sessions = new List<ChatSession>();
        private ChatSession? _activeSession;
        private readonly string _historyDir;
        private string? _username;

        public event Action? SessionListChanged;
        public event Action<ChatSession>? ActiveSessionChanged;

        public ChatSession? ActiveSession => _activeSession;
        public IReadOnlyList<ChatSession> Sessions => _sessions.AsReadOnly();

        public ChatManager()
        {
            _historyDir = Path.Combine(Application.LocalUserAppDataPath, "History");
            Directory.CreateDirectory(_historyDir);
            _username = null;
            CreateSession("Phòng chat chính");
        }

        /// <summary>
        /// Gắn username cho manager này (dùng để lưu/đọc lịch sử riêng).
        /// </summary>
        public void SetUsername(string username)
        {
            _username = username;
        }

        /// <summary>
        /// Lưu lịch sử chat của user hiện tại xuống file JSON.
        /// </summary>
        public void SaveHistory()
        {
            if (string.IsNullOrEmpty(_username)) return;

            try
            {
                var data = new HistoryData
                {
                    Username = _username,
                    Sessions = _sessions.Select(s => new SessionData
                    {
                        Name = s.Name,
                        IsPrivate = s.IsPrivate,
                        PrivateChatPartner = s.PrivateChatPartner,
                        Messages = s.Messages.ToList(),
                        UnreadCount = s.UnreadCount
                    }).ToList()
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                string filePath = Path.Combine(_historyDir, $"history_{_username}.json");
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SAVE HISTORY] {ex.Message}");
            }
        }

        /// <summary>
        /// Đọc lịch sử chat của user hiện tại từ file JSON.
        /// </summary>
        public bool LoadHistory()
        {
            if (string.IsNullOrEmpty(_username)) return false;

            try
            {
                string filePath = Path.Combine(_historyDir, $"history_{_username}.json");
                if (!File.Exists(filePath)) return false;

                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<HistoryData>(json);
                if (data == null) return false;

                // Giữ lại session đầu tiên (public), xoá các session cũ
                _sessions.Clear();
                _activeSession = null;

                // Tập hợp để khử trùng lặp PrivateChatPartner (so sánh không phân biệt hoa thường)
                var seenPrivatePartners = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var sd in data.Sessions)
                {
                    // Bỏ qua session private có PrivateChatPartner rỗng/null
                    if (sd.IsPrivate && string.IsNullOrEmpty(sd.PrivateChatPartner))
                        continue;

                    // Bỏ qua session private có PrivateChatPartner chứa chuỗi rác
                    if (sd.IsPrivate && sd.PrivateChatPartner != null)
                    {
                        string partner = sd.PrivateChatPartner;
                        if (partner.Contains("PRIVATE") || partner.Contains("ECHO") ||
                            partner.Contains("DELIVERED") || partner.Contains('|'))
                            continue;

                        // Khử trùng lặp: nếu đã thêm private session cùng partner thì bỏ qua
                        if (!seenPrivatePartners.Add(partner))
                            continue;
                    }

                    var session = new ChatSession(sd.Name)
                    {
                        IsPrivate = sd.IsPrivate,
                        PrivateChatPartner = sd.PrivateChatPartner,
                        UnreadCount = sd.UnreadCount
                    };
                    foreach (var msg in sd.Messages)
                        session.AddMessage(msg);
                    _sessions.Add(session);
                }

                // Đảm bảo có ít nhất 1 session
                if (_sessions.Count == 0)
                    CreateSession("Phòng chat chính");

                // Chuyển về session đầu tiên
                SetActiveSession(_sessions[0]);
                SessionListChanged?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOAD HISTORY] {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Đánh dấu active session đã đọc (reset unread count).
        /// </summary>
        public void MarkActiveSessionRead()
        {
            if (_activeSession != null)
            {
                _activeSession.MarkAsRead();
                SessionListChanged?.Invoke();
            }
        }

        public ChatSession CreateSession(string name)
        {
            var session = new ChatSession(name);
            _sessions.Add(session);
            SetActiveSession(session);
            SessionListChanged?.Invoke();
            return session;
        }

        /// <summary>
        /// Tìm hoặc tạo mới một phiên chat riêng với người dùng chỉ định.
        /// Nếu đã có phiên chat riêng với người đó, chuyển sang phiên đó.
        /// </summary>
        public ChatSession GetOrCreatePrivateSession(string partnerName, string currentUsername)
        {
            var existing = _sessions.FirstOrDefault(s =>
                s.IsPrivate && string.Equals(s.PrivateChatPartner, partnerName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                SetActiveSession(existing);
                SessionListChanged?.Invoke();
                return existing;
            }

            var session = new ChatSession($"🔒 {partnerName}")
            {
                IsPrivate = true,
                PrivateChatPartner = partnerName
            };
            _sessions.Add(session);
            SetActiveSession(session);
            SessionListChanged?.Invoke();
            return session;
        }

        /// <summary>
        /// Lấy phiên chat riêng dựa vào tên người gửi (dùng khi nhận tin nhắn PRIVATE từ server).
        /// Nếu chưa có, tạo mới nhưng không tự động chuyển sang (giữ nguyên session hiện tại).
        /// </summary>
        public ChatSession GetOrCreatePrivateSessionSilent(string partnerName)
        {
            var existing = _sessions.FirstOrDefault(s =>
                s.IsPrivate && string.Equals(s.PrivateChatPartner, partnerName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                return existing;

            var session = new ChatSession($"🔒 {partnerName}")
            {
                IsPrivate = true,
                PrivateChatPartner = partnerName
            };
            _sessions.Add(session);
            SessionListChanged?.Invoke();
            return session;
        }

        public void RenameSession(int sessionId, string newName)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null && !session.IsPrivate)
            {
                session.Name = newName;
                SessionListChanged?.Invoke();
            }
        }

        public bool DeleteSession(int sessionId)
        {
            if (_sessions.Count <= 1)
            {
                MessageBox.Show("Không thể xóa cuộc trò chuyện cuối cùng.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null) return false;

            if (session.IsPrivate)
            {
                MessageBox.Show("Không thể xóa cuộc trò chuyện riêng tư.\nLịch sử sẽ được giữ lại.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            int index = _sessions.IndexOf(session);
            _sessions.Remove(session);

            if (_activeSession == session)
            {
                int newIndex = Math.Min(index, _sessions.Count - 1);
                SetActiveSession(_sessions[newIndex]);
            }

            SessionListChanged?.Invoke();
            return true;
        }

        public void SetActiveSession(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
                SetActiveSession(session);
        }

        private void SetActiveSession(ChatSession session)
        {
            if (_activeSession != session)
            {
                if (_activeSession != null)
                {
                    // Lưu RTF trước khi chuyển
                }
                _activeSession = session;
                _activeSession.MarkAsRead();
                ActiveSessionChanged?.Invoke(session);
                SessionListChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// DTO để serialize/deserialize lịch sử chat.
    /// </summary>
    internal class HistoryData
    {
        public string Username { get; set; } = "";
        public List<SessionData> Sessions { get; set; } = new List<SessionData>();
    }

    internal class SessionData
    {
        public string Name { get; set; } = "";
        public bool IsPrivate { get; set; }
        public string? PrivateChatPartner { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public int UnreadCount { get; set; }
    }
}
