using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        private Socket clientSocket;
        private Thread listenThread;
        private volatile bool _isRunning; 

        public Form1()
        {
            InitializeComponent();
            
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            ConnectToServer();
        }

        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isRunning = false;

           
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch { }
            finally
            {
                try { clientSocket?.Close(); } catch { }
            }

            
            if (listenThread != null && listenThread.IsAlive)
            {
                listenThread.Join(2000);
            }
        }

     
        private void ConnectToServer()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999));

                _isRunning = true;

               
                listenThread = new Thread(ListenFromServer);
                listenThread.IsBackground = true;
                listenThread.Start();

               
                this.Invoke((MethodInvoker)(() =>
                {
                    this.Text = "Client - Đã kết nối tới Server";
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối đến server: " + ex.Message,
                                "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSendImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Chọn hình ảnh để gửi",
                Filter = "Hình ảnh|*.jpg;*.jpeg;*.png;*.gif"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                Thread t = new Thread(() =>
                {
                    string message;
                    bool success = FileSender.Send(clientSocket, filePath, FilePacket.IMAGE, progressBarChat, out message);

                    this.Invoke((MethodInvoker)(() =>
                    {
                        if (success)
                        {
                            MessageBox.Show(message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(message ?? "Không thể gửi hình ảnh.", "Lỗi",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                });
                t.IsBackground = true;
                t.Start();
            }
        }

        
        private void btnSendVideo_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Chọn video để gửi",
                Filter = "Video|*.mp4;*.avi;*.mkv"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                Thread t = new Thread(() =>
                {
                    string message;
                    bool success = FileSender.Send(clientSocket, filePath, FilePacket.VIDEO, progressBarChat, out message);

                    this.Invoke((MethodInvoker)(() =>
                    {
                        if (success)
                        {
                            MessageBox.Show(message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(message ?? "Không thể gửi video.", "Lỗi",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                });
                t.IsBackground = true;
                t.Start();
            }
        }

       
        private void ListenFromServer()
        {
            try
            {
                while (_isRunning && clientSocket != null && clientSocket.Connected)
                {
                    
                    byte dataType;
                    byte[] payload;
                    byte[] expectedMd5;
                    string errorMessage;

                    
                    ProgressBar pb = null;
                    this.Invoke((MethodInvoker)(() =>
                    {
                        pb = progressBarChat;
                    }));

                    bool success = FileReceiver.ReceiveFile(
                        clientSocket,
                        out dataType,
                        out payload,
                        out expectedMd5,
                        out errorMessage,
                        pb 
                    );

                    if (!success)
                    {
                       
                        this.Invoke((MethodInvoker)(() =>
                        {
                            MessageBox.Show(errorMessage, "Lỗi nhận file",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                        continue;
                    }

                    
                    if (dataType == FilePacket.IMAGE)
                    {
                        ProcessReceivedImage(payload, expectedMd5);
                    }
                    else if (dataType == FilePacket.VIDEO)
                    {
                        ProcessReceivedVideo(payload, expectedMd5);
                    }
                }
            }
            catch (SocketException)
            {
                
                this.Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show("Mất kết nối với máy chủ.", "Lỗi kết nối",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Text = "Client - Mất kết nối";
                }));
            }
            catch (ObjectDisposedException)
            {
                // Form đã đóng hoặc socket đã đóng — thoát im lặng
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                this.Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show("Mất kết nối với máy chủ: " + ex.Message,
                                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Text = "Client - Mất kết nối";
                }));
            }
        }

        
        private void ProcessReceivedImage(byte[] imageData, byte[] md5Hash)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    
                    Image originalImg = Image.FromStream(ms);
                    
                    Bitmap clonedImg = new Bitmap(originalImg);
                    originalImg.Dispose();

                    
                    this.Invoke((MethodInvoker)(() =>
                    {
                        
                        if (picChatBox.Image != null)
                        {
                            Image oldImg = picChatBox.Image;
                            picChatBox.Image = null;
                            oldImg.Dispose();
                        }

                        picChatBox.Image = clonedImg;
                        picChatBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }));

                   
                    string md5Str = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
                    this.Invoke((MethodInvoker)(() =>
                    {
                        MessageBox.Show("Đã nhận hình ảnh thành công!\nKích thước: " +
                                        (imageData.Length / 1024.0).ToString("F2") + " KB" +
                                        "\nMD5: " + md5Str,
                                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                }
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show("Lỗi hiển thị ảnh: " + ex.Message,
                                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        
        private void ProcessReceivedVideo(byte[] videoData, byte[] md5Hash)
        {
            try
            {
                string md5Str = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
                string sizeStr = (videoData.Length / 1024.0).ToString("F2") + " KB";
                if (videoData.Length > 1024 * 1024)
                    sizeStr = (videoData.Length / (1024.0 * 1024.0)).ToString("F2") + " MB";

                // Mở SaveFileDialog trên luồng UI
                this.Invoke((MethodInvoker)(() =>
                {
                    SaveFileDialog sfd = new SaveFileDialog
                    {
                        Title = "Lưu video nhận được",
                        Filter = "Video File|*.mp4;*.avi;*.mkv|Tất cả các file|*.*",
                        FileName = "Video_Nhan_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4"
                    };

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            File.WriteAllBytes(sfd.FileName, videoData);
                            MessageBox.Show("Đã nhận và lưu video thành công!\n" +
                                            "File: " + sfd.FileName + "\n" +
                                            "Kích thước: " + sizeStr + "\n" +
                                            "MD5: " + md5Str,
                                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi lưu video: " + ex.Message,
                                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show("Lỗi xử lý video: " + ex.Message,
                                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }
    }
}
