using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Client
{
    
    public static class FileReceiver
    {
       
        private const int CHUNK_SIZE = 65536;

      
        public static byte[] ReceiveExactly(Socket socket, int count)
        {
            byte[] buffer = new byte[count];
            int totalReceived = 0;

            while (totalReceived < count)
            {
                int received = socket.Receive(buffer, totalReceived, count - totalReceived, SocketFlags.None);
                if (received == 0)
                    throw new Exception("Bị ngắt kết nối với Server khi đang nhận dữ liệu.");
                totalReceived += received;
            }

            return buffer;
        }

      
        public static bool ReceiveFile(Socket socket,
            out byte dataType,
            out byte[] payload,
            out byte[] expectedMd5,
            out string errorMessage,
            ProgressBar progressBar)
        {
            dataType = 0;
            payload = null;
            expectedMd5 = null;
            errorMessage = null;

          
            int lastReportedProgress = -1;

            try
            {
                // 1. Nhận chính xác HEADER_SIZE bytes
                byte[] header = ReceiveExactly(socket, FilePacket.HEADER_SIZE);

                // 2. Phân tích header
                FilePacket.ParseHeader(header, out dataType, out long dataSize, out expectedMd5);

                // 3. Kiểm tra dataSize hợp lệ (tránh mảng quá lớn)
                if (dataSize < 0 || dataSize > int.MaxValue)
                    throw new Exception("Kích thước dữ liệu không hợp lệ: " + dataSize);

                // 4. Nhận payload theo chunk, cập nhật progress
                payload = new byte[dataSize];
                long totalReceived = 0;

                while (totalReceived < dataSize)
                {
                    // Tính số byte cần đọc lần này (tối đa CHUNK_SIZE)
                    int remaining = (int)(dataSize - totalReceived);
                    int toRead = Math.Min(remaining, CHUNK_SIZE);

                    int received = socket.Receive(payload, (int)totalReceived, toRead, SocketFlags.None);
                    if (received == 0)
                        throw new Exception("Mất kết nối với Server khi đang nhận dữ liệu.");

                    totalReceived += received;

                    // Cập nhật ProgressBar (chỉ khi tiến trình thay đổi >= 1%)
                    if (progressBar != null && dataSize > 0)
                    {
                        int currentProgress = (int)((totalReceived * 100) / dataSize);
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

                // Reset thanh tiến trình về 0 sau khi nhận xong
                if (progressBar != null)
                {
                    progressBar.Invoke((MethodInvoker)(() => progressBar.Value = 0));
                }

                // 5. Kiểm tra MD5 toàn vẹn
                using (MD5 md5 = MD5.Create())
                {
                    byte[] actualMd5 = md5.ComputeHash(payload);

                    // So sánh MD5 từng byte
                    for (int i = 0; i < 16; i++)
                    {
                        if (expectedMd5[i] != actualMd5[i])
                        {
                            errorMessage = "File nhận được không toàn vẹn (MD5 không khớp).";
                            return false;
                        }
                    }
                }

                return true; // Thành công
            }
            catch (Exception ex)
            {
                errorMessage = "Lỗi nhận file: " + ex.Message;
                // Reset thanh tiến trình về 0 khi có lỗi
                if (progressBar != null)
                {
                    try { progressBar.Invoke((MethodInvoker)(() => progressBar.Value = 0)); } catch { }
                }
                return false;
            }
        }
    }
}
