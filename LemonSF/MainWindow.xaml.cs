using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LemonSF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var c = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            this.BeginAnimation(OpacityProperty, c);
        }

        private void CLOSE_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var c = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            c.Completed += delegate { Environment.Exit(0); };
            this.BeginAnimation(OpacityProperty, c);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        string sendpath = "";
        private void window_Drop(object sender, DragEventArgs e)
        {
            sendpath = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            bc.Text = "已选择文件:" + sendpath;
            bc.ToolTip = bc.Text;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                sendpath = openFileDialog.FileName;
                bc.Text = "已选择文件:" + sendpath;
                bc.ToolTip = "已选择文件:" + sendpath;
            }
        }

        private string GetIP() 
        {
            IPHostEntry ipHost = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            return ipAddr.ToString();
        }
        Socket client;
        private async void border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            await Task.Run(new Action(delegate {
                this.Dispatcher.Invoke(new Action(delegate { tb.Text = $"连接{GetIP()} 端口:9050"; }));
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
                Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                newsock.Bind(ipep);
                newsock.Listen(10);
                client = newsock.Accept();
                IPEndPoint clientip = (IPEndPoint)client.RemoteEndPoint;
                this.Dispatcher.Invoke(new Action(delegate { tb.Text = "已连接" + client.AddressFamily + ":" + clientip.Port; }));
                this.Dispatcher.Invoke(new Action(delegate { tbf.Text = "已连接，可以发送"; }));
            }));
        }

        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            if (sendpath != "")
            {
                try
                {
                    var info = new FileInfo(sendpath);
                    var name= Encoding.Default.GetBytes(info.Name);
                    var data = File.ReadAllBytes(sendpath);
                    var lenght = Encoding.Default.GetBytes(data.Length.ToString());
                    client.Send(name, name.Length, SocketFlags.None);
                    client.Send(lenght, lenght.Length, SocketFlags.None);
                    client.Send(data, data.Length, SocketFlags.None);
                    tbf.Text = "发送成功";
                }
                catch { tbf.Text = "发送失败"; }
            }
        }

        private async void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            await Task.Run(new Action(delegate
            {
                var ipx = "";
                this.Dispatcher.Invoke(new Action(delegate { ipx = ip.Text; }));
                byte[] data = new byte[2048];
                Socket newclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                newclient.Bind(new IPEndPoint(IPAddress.Any, 905));
                IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipx), 9050); 
                try
                {
                    newclient.Connect(ie);
                    Dispatcher.Invoke(new Action(delegate { w.Text = "连接成功，等待发送"; }));
                }
                catch { Dispatcher.Invoke(new Action(delegate { w.Text = "连接失败，请重试"; })); }
                var ind=newclient.Receive(data);
                var name = Encoding.Default.GetString(data,0,ind);
                data = new byte[2048];
                newclient.Receive(data);
                var lenght=int.Parse( Encoding.Default.GetString(data));
                data = new byte[lenght];
                int receivedDataLength = newclient.Receive(data);
                FileStream fs = new FileStream(savefile+"\\"+name, FileMode.Create);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
                Dispatcher.Invoke(new Action(delegate { w.Text = "传输完成"; }));
            }));
        }
        string savefile = "";
        private void Border_MouseDown_4(object sender, MouseButtonEventArgs e)
        {
            var m_Dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                savefile = m_Dialog.SelectedPath;
                bc1.Text = "保存到:"+savefile;
                bc1.ToolTip = bc1.Text;
            }
        }
    }
}
