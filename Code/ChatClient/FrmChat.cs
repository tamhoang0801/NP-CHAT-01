using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class FrmChat : Form
    {

        private readonly string _username;

        private TcpClient _client;

        private NetworkStream _stream;

        private Thread _receiveThread;



        public FrmChat(string username, TcpClient client)
        {

            _username = username;

            _client = client;

            _stream = client.GetStream();


            InitializeComponent();


            btnEmoji.Click += btnEmoji_Click;


            FormClosed +=
            (s,e)=>
            {
                Environment.Exit(0);
            };

        }




        private void FrmChat_Load(object sender, EventArgs e)
        {

            Text =
            "Chat Application - " + _username;


            lblCurrentUser.Text =
            _username;



            AppendSystemMessage(
            "Chào mừng " + _username + " đã tham gia phòng chat"
            );



            _receiveThread =
            new Thread(ReceiveMessage);


            _receiveThread.IsBackground=true;


            _receiveThread.Start();

        }






        private void ReceiveMessage()
        {

            while(_stream!=null)
            {

                try
                {

                    byte[] buffer =
                    new byte[4096];


                    int size =
                    _stream.Read(buffer,0,buffer.Length);


                    if(size==0)
                        break;



                    string data =
                    Encoding.UTF8.GetString(buffer,0,size);



                    string[] parts =
                    data.Split('|');



                    if(parts[0]=="BROADCAST")
                    {

                        string msg =
                        parts[1];


                        string time =
                        DateTime.Now.ToString("HH:mm");



                        if(msg.StartsWith("[Hệ thống]"))
                        {

                            AppendSystemMessage(msg);

                        }

                        else
                        {

                            int index =
                            msg.IndexOf(':');


                            if(index>0)
                            {

                                string name =
                                msg.Substring(0,index);


                                string content =
                                msg.Substring(index+1);



                                AppendOtherMessage(
                                name,
                                content,
                                time);

                            }

                        }

                    }


                    else if(parts[0]=="UPDATE_ONLINE")
                    {

                        UpdateOnlineUsers(
                        parts[1].Split(','));

                    }


                }
                catch
                {

                    break;

                }

            }

        }








        private void SendTextMessage()
        {

            string msg =
            txtMessage.Text.Trim();


            if(msg=="")
                return;



            string time =
            DateTime.Now.ToString("HH:mm");



            AppendMyMessage(
            _username,
            msg,
            time);



            string data =
            $"MSG|{_username}|{msg}";



            byte[] bytes =
            Encoding.UTF8.GetBytes(data);



            try
            {

                _stream.Write(
                bytes,
                0,
                bytes.Length);

            }

            catch
            {

                MessageBox.Show(
                "Không thể gửi tin nhắn");

            }



            txtMessage.Clear();

        }





        private void btnSend_Click(
        object sender,
        EventArgs e)
        {

            SendTextMessage();

        }







        private void txtMessage_KeyDown(
        object sender,
        KeyEventArgs e)
        {


            if(e.KeyCode==Keys.Enter)
            {

                e.SuppressKeyPress=true;

                SendTextMessage();

            }

        }








        private void btnEmoji_Click(
        object sender,
        EventArgs e)
        {


            txtMessage.Text += " 😊 ";

            txtMessage.Focus();


        }









        private void SendFile(
        byte type,
        string filter)
        {


            OpenFileDialog dialog =
            new OpenFileDialog();


            dialog.Filter =
            filter;



            if(dialog.ShowDialog()==DialogResult.OK)
            {


                byte[] file =
                File.ReadAllBytes(dialog.FileName);



                byte[] header =
                new byte[9];


                header[0]=type;



                Array.Copy(
                BitConverter.GetBytes(
                (long)file.Length),
                0,
                header,
                1,
                8);



                _stream.Write(
                header,
                0,
                9);



                _stream.Write(
                file,
                0,
                file.Length);



                AppendSystemMessage(
                "Bạn đã gửi file");

            }


        }






        private void btnSendImage_Click(
        object sender,
        EventArgs e)
        {

            SendFile(
            0x10,
            "Image|*.jpg;*.png;*.jpeg");

        }





        private void btnSendVideo_Click(
        object sender,
        EventArgs e)
        {

            SendFile(
            0x11,
            "Video|*.mp4;*.avi");

        }







        private void btnLogout_Click(
        object sender,
        EventArgs e)
        {


            try
            {

                byte[] data =
                Encoding.UTF8.GetBytes(
                $"LOGOUT|{_username}");

                _stream.Write(
                data,
                0,
                data.Length);

            }
            catch{}

            Environment.Exit(0);

        }









        public void UpdateOnlineUsers(
        string[] users)
        {

            if(InvokeRequired)
            {

                Invoke(
                new Action(
                ()=>UpdateOnlineUsers(users)));

                return;

            }



            lstOnlineUsers.Items.Clear();



            foreach(string u in users)
            {

                lstOnlineUsers.Items.Add(
                "🟢 "+u);

            }



            lblOnlineCount.Text =
            "Online: "+users.Length;


        }









        public void AppendMyMessage(
        string name,
        string text,
        string time)
        {

            AppendMessage(
            name,
            text,
            time,
            Color.FromArgb(0,132,255));

        }





        public void AppendOtherMessage(
        string name,
        string text,
        string time)
        {

            AppendMessage(
            name,
            text,
            time,
            Color.DarkOrange);

        }






        public void AppendSystemMessage(
        string text)
        {

            rtbChat.AppendText(
            "\n[System] "
            +text+
            "\n");

        }








        private void AppendMessage(
        string name,
        string msg,
        string time,
        Color color)
        {


            rtbChat.SelectionStart =
            rtbChat.TextLength;



            rtbChat.SelectionColor =
            color;



            rtbChat.SelectionFont =
            new Font(
            "Segoe UI",
            10,
            FontStyle.Bold);



            rtbChat.AppendText(
            "\n"+name);



            rtbChat.SelectionColor =
            Color.Black;



            rtbChat.SelectionFont =
            new Font(
            "Segoe UI",
            11);



            rtbChat.AppendText(
            "\n"+msg);



            rtbChat.SelectionColor =
            Color.Gray;



            rtbChat.AppendText(
            "\n"+time+"\n");


            rtbChat.ScrollToCaret();

        }



    }
}