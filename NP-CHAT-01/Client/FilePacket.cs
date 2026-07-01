using System;

namespace Client
{
    /// <summary>
    /// Cấu trúc gói tin (Packet) dùng chung cho truyền file qua Socket TCP.
    /// Header 25 bytes: [1 byte loại] + [8 byte kích thước (long)] + [16 byte MD5 hash].
    /// </summary>
    public class FilePacket
    {
        public const byte TEXT  = 0x01;
        public const byte IMAGE = 0x02;
        public const byte VIDEO = 0x03;

        /// <summary>Kích thước header tính bằng byte (1 + 8 + 16 = 25).</summary>
        public const int HEADER_SIZE = 25;

        /// <summary>Tạo header 25 bytes từ loại dữ liệu, kích thước và MD5 hash.</summary>
        public static byte[] CreateHeader(byte dataType, long dataSize, byte[] md5Hash)
        {
            if (md5Hash == null || md5Hash.Length != 16)
                throw new ArgumentException("MD5 hash phải có độ dài 16 bytes.", nameof(md5Hash));

            byte[] header = new byte[HEADER_SIZE];
            header[0] = dataType;

            byte[] sizeBytes = BitConverter.GetBytes(dataSize);
            // Copy 8 byte kích thước vào header[1..8]
            Array.Copy(sizeBytes, 0, header, 1, 8);

            // Copy 16 byte MD5 vào header[9..24]
            Array.Copy(md5Hash, 0, header, 9, 16);

            return header;
        }

        /// <summary>Phân tích header 25 bytes thành loại, kích thước và MD5 hash.</summary>
        public static void ParseHeader(byte[] header, out byte dataType, out long dataSize, out byte[] md5Hash)
        {
            if (header == null || header.Length < HEADER_SIZE)
                throw new ArgumentException("Header phải có ít nhất " + HEADER_SIZE + " bytes.", nameof(header));

            dataType = header[0];
            dataSize = BitConverter.ToInt64(header, 1);

            md5Hash = new byte[16];
            Array.Copy(header, 9, md5Hash, 0, 16);
        }
    }
}
