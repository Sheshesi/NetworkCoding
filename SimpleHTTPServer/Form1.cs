using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleHTTPServer
{
    public partial class Form1 : Form
    {

        private Socket httpServer;
        private int serverPort = 80;
        private Thread thread;

        public Form1()
        {
            InitializeComponent();
        }

        private void startServerBtn_Click(object sender, EventArgs e)
        {
            serverLogsText.Text = "";

            try
            {
                httpServer = new Socket(SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    serverPort = int.Parse(serverPortText.Text.ToString());

                    if(serverPort > 65535 || serverPort <= 0)
                    {
                       throw new Exception("Серверный порт не попадает в диапазон");
                    }
                }
                catch(Exception ex)
                {
                    serverPort = 80;
                    serverLogsText.Text = "Server failed to start on specified port \n";
                }

                thread = new Thread(new ThreadStart(this.connectionThreadMethod));
                thread.Start();

                // Disable and Enable Buttons
                startServerBtn.Enabled = false;
                stopServerBtn.Enabled = true;

            }
            catch(Exception ex)
            {
                serverLogsText.Text = "Server starting failed \n";
            }

            serverLogsText.Text = "Start server";
        }

        private void stopServerBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Close the Socket
                httpServer.Close();

                // Kill the Thread
                thread.Abort();

                // Disable and Enable Buttons
                startServerBtn.Enabled = true;
                stopServerBtn.Enabled = false;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Нельзя остановить");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            stopServerBtn.Enabled = false;
        }

        private void connectionThreadMethod()
        {
            try
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, serverPort);
                httpServer.Bind(endpoint);
                httpServer.Listen(1);
                startListeningForConnection();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Нельзя стартовать");
            }
        }

        private void startListeningForConnection()
        {
            while (true)
            {
                DateTime time = DateTime.Now;

                String data = "";
                byte[] bytes = new byte[2048];

                Socket client = httpServer.Accept();
                while (true)
                {
                    int numBytes = client.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, numBytes);

                    if (data.IndexOf("\r\n") > -1)
                        break;
                }

                serverLogsText.Invoke((MethodInvoker)delegate {
                    serverLogsText.Text += "\r\n\r\n";
                    serverLogsText.Text += data;
                    serverLogsText.Text += "\n\n------ End of Request -------";
                });

                String resHeader = "HTTP/1.1 200 Everything is Fine\nServer: my_csharp_server\nContent-Type: text/html; charset: UTF-8\n\n";
                String resBody = "<!DOCTYE html> " +
                    "<html>" +
                    "<head><title>My Server</title></head>" +
                    "<body>" +
                    "<h4>Server Time is: " + time.ToString() + "</h4>" +
                    "</body></html>";

                String resStr = resHeader + resBody;

                byte[] resData = Encoding.ASCII.GetBytes(resStr);

                client.SendTo(resData, client.RemoteEndPoint);

                client.Close();
            }
        }
    }
}
