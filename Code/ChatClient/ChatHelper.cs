using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ChatApp
{
    /// <summary>
    /// Lớp tiện ích dùng chung cho các thao tác mạng và xử lý chuỗi.
    /// Tránh duplicate code giữa các Form/Service.
    /// </summary>
    public static class ChatHelper
    {
        /// <summary>
        /// Đọc chính xác 1 dòng UTF-8 kết thúc bằng \n từ NetworkStream (tối đa 8KB).
        /// Trả về null nếu stream đóng hoặc vượt quá giới hạn.
        /// </summary>
        public static string? ReadLineUtf8(NetworkStream stream)
        {
            const int maxLineBytes = 8192;
            var bytes = new List<byte>(maxLineBytes);
            byte[] one = new byte[1];
            while (true)
            {
                int n = stream.Read(one, 0, 1);
                if (n == 0) return null;
                if (one[0] == (byte)'\n') break;
                if (bytes.Count >= maxLineBytes) return null;
                bytes.Add(one[0]);
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Đọc chính xác count byte từ socket, xử lý TCP fragmentation.
        /// Trả về false nếu kết nối bị đóng giữa chừng.
        /// </summary>
        public static bool ReceiveExact(Socket socket, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int n = socket.Receive(buffer, total, count - total, SocketFlags.None);
                if (n == 0) return false;
                total += n;
            }
            return true;
        }

        /// <summary>
        /// Loại bỏ \r và \n khỏi chuỗi để tránh phá vỡ giao thức dòng.
        /// </summary>
        public static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace('\r', ' ').Replace('\n', ' ');
        }

        /// <summary>
        /// Kiểm tra kết nối TCP còn sống không (không blocking lâu).
        /// </summary>
        public static bool IsClientConnected(TcpClient client)
        {
            try
            {
                if (client?.Client == null) return false;
                return !(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);
            }
            catch
            {
                return false;
            }
        }
    }
}
