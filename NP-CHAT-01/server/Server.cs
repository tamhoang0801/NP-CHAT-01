using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
   
    public class Server
    {
        private readonly int _port;
        private TcpListener _listener;
        private readonly List<ClientHandler> _clients;
        private readonly object _clientsLock = new object();
        private volatile bool _isRunning;
        private volatile bool _isStopped; 

        public Server(int port)
        {
            _port = port;
            _clients = new List<ClientHandler>();
        }

        
        public void Start()
        {
            try
            {
                _isRunning = true;
                _isStopped = false;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                Console.WriteLine(">>> Server khởi động trên cổng " + _port);
                Console.WriteLine(">>> Đang chờ kết nối từ Client...");

                while (_isRunning)
                {
                    Socket clientSocket = _listener.AcceptSocket();
                    Console.WriteLine(">>> Client mới kết nối: " + clientSocket.RemoteEndPoint);

                    ClientHandler handler = new ClientHandler(clientSocket, this);
                    lock (_clientsLock)
                    {
                        _clients.Add(handler);
                    }

                    Console.WriteLine(">>> Số client hiện tại: " + _clients.Count);
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                
                Console.WriteLine(">>> Server listener đã dừng.");
            }
            catch (ObjectDisposedException)
            {
               
                Console.WriteLine(">>> Server listener đã dừng (disposed).");
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!! Lỗi Server: " + ex.Message);
            }
            finally
            {
                
                if (!_isStopped)
                {
                    Stop();
                }
            }
        }

        
        public void Stop()
        {
           
            if (_isStopped) return;
            _isStopped = true;
            _isRunning = false;

            try { _listener?.Stop(); } catch { }

            lock (_clientsLock)
            {
                foreach (var client in _clients)
                {
                    try { client.Disconnect(); } catch { }
                }
                _clients.Clear();
            }

            Console.WriteLine(">>> Server đã dừng.");
        }

        public void Broadcast(byte[] data, ClientHandler sender)
        {
            List<ClientHandler> clientsToRemove = null;

            lock (_clientsLock)
            {
                foreach (var client in _clients)
                {
                    if (client == sender) continue; 

                    bool sent = client.Send(data);
                    if (!sent)
                    {
                      
                        if (clientsToRemove == null)
                            clientsToRemove = new List<ClientHandler>();
                        clientsToRemove.Add(client);
                    }
                }

              
                if (clientsToRemove != null)
                {
                    foreach (var client in clientsToRemove)
                    {
                        _clients.Remove(client);
                        try { client.Disconnect(); } catch { }
                        Console.WriteLine(">>> Client " + client.RemoteEndPoint + " đã ngắt kết nối (Broadcast lỗi).");
                    }
                }
            }
        }

      
        public void RemoveClient(ClientHandler client)
        {
            lock (_clientsLock)
            {
                if (_clients.Remove(client))
                {
                    Console.WriteLine(">>> Client " + client.RemoteEndPoint + " đã bị xóa khỏi danh sách.");
                    Console.WriteLine(">>> Số client hiện tại: " + _clients.Count);
                }
            }
        }

     
        public static void Main(string[] args)
        {
            Console.Title = "NP-CHAT-01 Server";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Server server = new Server(9999);

         

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\n>>> Đang dừng Server...");
                server.Stop();
            };

            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!! Lỗi: " + ex.Message);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine(">>> Server đã kết thúc. Nhấn Enter để thoát...");
            Console.ReadLine();
        }
    }
}
