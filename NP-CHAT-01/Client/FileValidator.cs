using System;
using System.IO;
using System.Linq;

namespace Client
{
   
    public static class FileValidator
    {
       
        public const long MAX_FILE_SIZE = 100 * 1024 * 1024; // 100MB

        public static string ValidateImage(string filePath)
        {
            string result = ValidateCommon(filePath);
            if (result != null) return result;

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            string[] validExts = { ".jpg", ".jpeg", ".png", ".gif" };
            if (!validExts.Contains(ext))
                return "Định dạng ảnh không hợp lệ. Chỉ chấp nhận: " + string.Join(", ", validExts);

            if (!CheckMagicBytes(filePath, ext))
                return "File ảnh không đúng định dạng (magic bytes không khớp).";

            return null; // Hợp lệ
        }

        
        public static string ValidateVideo(string filePath)
        {
            string result = ValidateCommon(filePath);
            if (result != null) return result;

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            string[] validExts = { ".mp4", ".avi", ".mkv" };
            if (!validExts.Contains(ext))
                return "Định dạng video không hợp lệ. Chỉ chấp nhận: " + string.Join(", ", validExts);

            if (!CheckMagicBytes(filePath, ext))
                return "File video không đúng định dạng (magic bytes không khớp).";

            return null; // Hợp lệ
        }

        
        private static string ValidateCommon(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return "Đường dẫn file không hợp lệ.";

            if (!File.Exists(filePath))
                return "File không tồn tại: " + filePath;

            FileInfo fi = new FileInfo(filePath);
            if (fi.Length == 0)
                return "File rỗng, không thể gửi.";

            if (fi.Length > MAX_FILE_SIZE)
                return "File quá lớn (tối đa 100MB). File hiện tại: " + (fi.Length / (1024.0 * 1024.0)).ToString("F2") + " MB.";

            return null;
        }

       
        private static bool CheckMagicBytes(string filePath, string extension)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] magic = new byte[8]; // Đọc 8 byte đầu
                    int bytesRead = fs.Read(magic, 0, magic.Length);
                    if (bytesRead < 4) return false; // Cần ít nhất 4 byte

                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            
                            return magic[0] == 0xFF && magic[1] == 0xD8 && magic[2] == 0xFF;

                        case ".png":
                            
                            return magic[0] == 0x89 && magic[1] == 0x50 && magic[2] == 0x4E && magic[3] == 0x47;

                        case ".gif":
                            
                            return magic[0] == 0x47 && magic[1] == 0x49 && magic[2] == 0x46 && magic[3] == 0x38;

                        case ".mp4":
                            
                            if (bytesRead < 8) return false;
                            // ftyp box: byte 4-7 = "ftyp"
                            bool isFtyp = magic[4] == 0x66 && magic[5] == 0x74 && magic[6] == 0x79 && magic[7] == 0x70;
                            // moov box: byte 4-7 = "moov"
                            bool isMoov = magic[4] == 0x6D && magic[5] == 0x6F && magic[6] == 0x6F && magic[7] == 0x76;
                            return isFtyp || isMoov;

                        case ".avi":
                            // AVI bắt đầu bằng 52 49 46 46 (RIFF)
                            return magic[0] == 0x52 && magic[1] == 0x49 && magic[2] == 0x46 && magic[3] == 0x46;

                        case ".mkv":
                            // MKV/WebM bắt đầu bằng 1A 45 DF A3 (Matroska)
                            return magic[0] == 0x1A && magic[1] == 0x45 && magic[2] == 0xDF && magic[3] == 0xA3;

                        default:
                            return true; // Không xác định được thì bỏ qua
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
