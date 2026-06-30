using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ChatApp
{
    public static class AvatarHelper
    {
        public const string AvatarUploadCommand = "AVATAR";
        public const string AvatarUpdateCommand = "AVATAR_UPDATE";
        private const int AvatarSize = 64;
        public const long MaxAvatarFileSizeBytes = 2 * 1024 * 1024;

        public static string BuildAvatarUploadMessage(string username, string base64)
        {
            return $"{AvatarUploadCommand}|{username}|{base64.Length}|{base64}";
        }

        public static bool TryParseAvatarMessage(string data, out string username, out string base64)
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

        public static string ReadCompleteAvatarMessage(System.Net.Sockets.Socket socket, string initialData)
        {
            var sb = new StringBuilder(initialData);
            int expected = GetExpectedMessageCharLength(sb.ToString());

            while (expected < 0 || sb.Length < expected)
            {
                byte[] buf = new byte[4096];
                int read = socket.Receive(buf, 0, buf.Length, System.Net.Sockets.SocketFlags.None);
                if (read == 0) break;
                sb.Append(Encoding.UTF8.GetString(buf, 0, read));
                expected = GetExpectedMessageCharLength(sb.ToString());
                if (expected > 0 && sb.Length >= expected) break;
            }

            return sb.ToString();
        }

        public static bool TryValidateAvatarFile(string filePath, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                errorMessage = "Khong tim thay file avatar.";
                return false;
            }

            string extension = Path.GetExtension(filePath);
            if (!string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Chi chap nhan file avatar dinh dang .jpg!";
                return false;
            }

            long fileSize = new FileInfo(filePath).Length;
            if (fileSize > MaxAvatarFileSizeBytes)
            {
                errorMessage = $"File avatar qua lon! Toi da {MaxAvatarFileSizeBytes / (1024 * 1024)} MB.";
                return false;
            }

            try
            {
                using FileStream fs = File.OpenRead(filePath);
                using Image img = Image.FromStream(fs);
            }
            catch
            {
                errorMessage = "File avatar khong hop le hoac bi hong. Vui long chon file .jpg khac.";
                return false;
            }

            return true;
        }

        public static string ImageFileToBase64(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(bytes);
        }

        public static Image Base64ToImage(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            using MemoryStream ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        public static Image GetDefaultAvatar(string username, int size = 64)
        {
            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Vẽ nền hình tròn
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, size - 1, size - 1);
                    g.SetClip(path);
                    using var bgBrush = new SolidBrush(Color.FromArgb(120, 140, 180));
                    g.FillEllipse(bgBrush, 0, 0, size - 1, size - 1);
                }

                char letter = string.IsNullOrEmpty(username) ? '?' : char.ToUpper(username[0]);
                using Font font = new Font("Segoe UI", size / 2.2f, FontStyle.Bold);
                SizeF sz = g.MeasureString(letter.ToString(), font);
                g.DrawString(
                    letter.ToString(),
                    font,
                    Brushes.White,
                    (size - sz.Width) / 2f,
                    (size - sz.Height) / 2f);
            }
            return bmp;
        }

        public static Image ToThumbnail(Image source, int size)
        {
            return MakeCircular(ResizeImage(source, size, size), size);
        }

        /// <summary>Cắt ảnh vuông thành hình tròn với góc trong suốt.</summary>
        public static Bitmap MakeCircular(Image source, int size)
        {
            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, size - 1, size - 1);
                g.SetClip(path);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, size, size);
            }
            return bmp;
        }

        private static Image ResizeImage(Image img, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
            return bmp;
        }
    }
}
