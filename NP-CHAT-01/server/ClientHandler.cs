using System;
using System.Net.Sockets;
using System.Threading;

namespace Server
{

    public class ClientHandler
    {
       
        public const int HEADER_SIZE = 25;

        private Socket _clientSocket;
        private Server _server;

        public string RemoteEndPoint
        {
            get
            {
                try { return _clientSocket.RemoteEndPoint?.ToString() ?? "Unknown"; }
                catch { return "Disconnected"; }
            }
        }

        public ClientHandler(Socket socket, Server serverInstance)
        {
            _clientSocket = socket;
            _server = serverInstance;

            Thread t = new Thread(Listen);
            t.IsBackground = true;
            t.Start();
        }

        private void Listen()
        {
            try
            {
                while (true)
                {
                    
                    byte[] header = ReceiveExactly(HEADER_SIZE);
                    if (header == null) break; 

                    
                    long dataSize = BitConverter.ToInt64(header, 1);

                   
                    if (dataSize < 0 || dataSize > int.MaxValue)
                    {
                        Console.WriteLine("!!! ClientHandler (" + RemoteEndPoint + "): Kích thước dữ liệu không hợp lệ: " + dataSize);
                        break;
                    }

                
                    byte[] payload = ReceivePayload(dataSize);
                    if (payload == null) break; 

                   
                    byte[] fullPacket = new byte[HEADER_SIZE + dataSize];
                    Array.Copy(header, 0, fullPacket, 0, HEADER_SIZE);
                    Array.Copy(payload, 0, fullPacket, HEADER_SIZE, dataSize);

                    
                    _server.Broadcast(fullPacket, this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!! Lỗi ClientHandler (" + RemoteEndPoint + "): " + ex.Message);
            }
            finally
            {
               
                _server.RemoveClient(this);
                Disconnect();
                Console.WriteLine(">>> Client " + RemoteEndPoint + " đã ngắt kết nối.");
            }
        }

        private byte[] ReceiveExactly(int count)
        {
            byte[] buffer = new byte[count];
            int totalReceived = 0;

            while (totalReceived < count)
            {
                int received = _clientSocket.Receive(buffer, totalReceived, count - totalReceived, SocketFlags.None);
                if (received == 0)
                    return null; // Mất kết nối
                totalReceived += received;
            }

            return buffer;
        }

    
        private byte[] ReceivePayload(long dataSize)
        {
            const int CHUNK_SIZE = 65536; 

            byte[] payload = new byte[dataSize];
            long totalReceived = 0;

            while (totalReceived < dataSize)
            {
                int remaining = (int)(dataSize - totalReceived);
                int toRead = Math.Min(remaining, CHUNK_SIZE);

                int read = _clientSocket.Receive(payload, (int)totalReceived, toRead, SocketFlags.None);
                if (read == 0)
                    return null; 
                totalReceived += read;
            }

            return payload;
        }

     
        public bool Send(byte[] data)
        {
            try
            {
                if (_clientSocket != null && _clientSocket.Connected)
                {
                    _clientSocket.Send(data);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

      
        public void Disconnect()
        {
            try
            {
                if (_clientSocket != null && _clientSocket.Connected)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch { }
            finally
            {
                try { _clientSocket?.Close(); } catch { }
            }
        }
    }
}
