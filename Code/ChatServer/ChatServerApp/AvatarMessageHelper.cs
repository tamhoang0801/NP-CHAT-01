using System.Net.Sockets;
using System.Text;

namespace ChatServerApp
{
    public static class AvatarMessageHelper
    {
        public const string AvatarUploadCommand = "AVATAR";
        public const string AvatarUpdateCommand = "AVATAR_UPDATE";

        public static string BuildUpdateMessage(string username, string base64)
        {
            return $"{AvatarUpdateCommand}|{username}|{base64.Length}|{base64}";
        }

        public static bool TryParse(string data, out string username, out string base64)
        {
            username = "";
            base64 = "";

            if (string.IsNullOrEmpty(data))
                return false;

            int first = data.IndexOf('|');
            if (first < 0) return false;

            int second = data.IndexOf('|', first + 1);
            if (second < 0) return false;

            int third = data.IndexOf('|', second + 1);
            if (third < 0) return false;

            string cmd = data.Substring(0, first);
            if (cmd != AvatarUploadCommand && cmd != AvatarUpdateCommand)
                return false;

            username = data.Substring(first + 1, second - first - 1);

            string lenStr = data.Substring(second + 1, third - second - 1);
            if (!int.TryParse(lenStr, out int expectedLen) || expectedLen < 0)
                return false;

            int base64Start = third + 1;
            if (data.Length - base64Start < expectedLen)
                return false;

            base64 = data.Substring(base64Start, expectedLen);
            return true;
        }

        public static int GetExpectedMessageCharLength(string partialData)
        {
            if (!partialData.StartsWith(AvatarUploadCommand + "|") &&
                !partialData.StartsWith(AvatarUpdateCommand + "|"))
                return -1;

            int first = partialData.IndexOf('|');
            int second = partialData.IndexOf('|', first + 1);
            int third = partialData.IndexOf('|', second + 1);
            if (third < 0) return -1;

            string lenStr = partialData.Substring(second + 1, third - second - 1);
            if (!int.TryParse(lenStr, out int base64Len))
                return -1;

            return third + 1 + base64Len;
        }

        public static string ReadCompleteAvatarMessage(NetworkStream stream, string initialData)
        {
            var sb = new StringBuilder(initialData);
            int expected = GetExpectedMessageCharLength(sb.ToString());

            while (expected < 0 || sb.Length < expected)
            {
                byte[] buf = new byte[4096];
                int read = stream.Read(buf, 0, buf.Length);
                if (read == 0) break;
                sb.Append(Encoding.UTF8.GetString(buf, 0, read));
                expected = GetExpectedMessageCharLength(sb.ToString());
                if (expected > 0 && sb.Length >= expected) break;
            }

            return sb.ToString();
        }
    }
}
