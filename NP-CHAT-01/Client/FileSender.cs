using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Client
{
   
    public static class FileSender
    {
       
        private const int CHUNK_SIZE = 65536;

       
        private static readonly object _sendLock = new object();

        /// <summary>
        /// Gửi file lên Server.
        /// </summary>
        /// <param name="socket">Socket đã kết nối.</param>
        /// <param name="filePath">Đường dẫn file cần gửi.</param>
        /// <param name="dataType">Loại dữ liệu (FilePacket.IMAGE / FilePacket.VIDEO).</param>
        /// <param name="progressBar">ProgressBar cập nhật tiến trình (gọi Invoke).</param>
        /// <param name="message">Đầu ra: chuỗi thông báo kết quả (lỗi hoặc thành công).</param>
        /// <returns>True nếu gửi thành công, False nếu thất bại.</returns>
        public static bool Send(Socket socket, string filePath, byte dataType, ProgressBar progressBar, out string message)
        {
            message = null;

            // 1. Validate file
            string error = (dataType == FilePacket.IMAGE)
                ? FileValidator.ValidateImage(filePath)
                : FileValidator.ValidateVideo(filePath);

            if (error != null)
            {
                message = error;
                return false;
            }

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                // 2. Tính MD5 hash của toàn bộ file
                byte[] md5Hash;
                using (FileStream fs = File.OpenRead(filePath))
                using (MD5 md5 = MD5.Create())
                {
                    md5Hash = md5.ComputeHash(fs);
                }

                // 3. Tạo header 25 bytes
                byte[] header = FilePacket.CreateHeader(dataType, fileSize, md5Hash);

                // Biến đếm để giảm tần suất cập nhật ProgressBar
                int lastReportedProgress = -1;

                // 4. Gửi header + payload trong lock
                lock (_sendLock)
                {
                    // Kiểm tra socket còn kết nối không
                    if (socket == null || !socket.Connected)
                    {
                        message = "Socket không còn kết nối tới máy chủ.";
                        return false;
                    }

                    // Gửi header
                    socket.Send(header, 0, header.Length, SocketFlags.None);

                    // Gửi payload theo chunk
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[CHUNK_SIZE];
                        int bytesRead;
                        long totalSent = 0;

                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            socket.Send(buffer, 0, bytesRead, SocketFlags.None);
                            totalSent += bytesRead;

                            // Cập nhật ProgressBar (chỉ khi tiến trình thay đổi >= 1%)
                            if (progressBar != null && fileSize > 0)
                            {
                                int currentProgress = (int)((totalSent * 100) / fileSize);
                                if (currentProgress > 100) currentProgress = 100;
                                if (currentProgress != lastReportedProgress)
                                {
                                    lastReportedProgress = currentProgress;
                                    progressBar.Invoke((MethodInvoker)(() =>
                                    {
                                        progressBar.Value = currentProgress;
                                    }));
                                }
                            }
                        }
                    }
                }

                // 5. Reset thanh tiến trình về 0 sau khi gửi xong
                if (progressBar != null)
                {
                    progressBar.Invoke((MethodInvoker)(() => progressBar.Value = 0));
                }

                // 6. Tạo thông báo thành công
                string typeName = (dataType == FilePacket.IMAGE) ? "hình ảnh" : "video";
                string sizeStr = (fileSize / 1024.0).ToString("F2") + " KB";
                if (fileSize > 1024 * 1024)
                    sizeStr = (fileSize / (1024.0 * 1024.0)).ToString("F2") + " MB";

                message = "Gửi " + typeName + " thành công!\nFile: " + Path.GetFileName(filePath)
                          + "\nKích thước: " + sizeStr
                          + "\nMD5: " + BitConverter.ToString(md5Hash).Replace("-", "").ToLower();

                return true;
            }
            catch (SocketException)
            {
                message = "Mất kết nối với máy chủ khi đang gửi file.";
            }
            catch (Exception ex)
            {
                message = "Lỗi gửi file: " + ex.Message;
            }

            // Reset thanh tiến trình nếu có lỗi
            if (progressBar != null)
            {
                try
                {
                    progressBar.Invoke((MethodInvoker)(() => progressBar.Value = 0));
                }
                catch { }
            }

            return false;
        }
    }
}
