using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EO.WebBrowser;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;

namespace WinFormOBS
{
    public partial class Form1 : Form
    {
        //obs-socket
        private OBSWebsocket _obs;

        //紀錄是否已連線完成
        private bool isConnect;

        private string ftpIP;
        private string ftpAccount;
        private string ftpPassword;

        public Form1()
        {
            InitializeComponent();

            //取得環境變數
            this.ftpIP = Environment.GetEnvironmentVariable("FTP_IP");
            this.ftpAccount = Environment.GetEnvironmentVariable("FTP_ACCOUNT");
            this.ftpPassword = Environment.GetEnvironmentVariable("FTP_PASS");

            //設定URL
            this.webView1.Url = "http://192.168.0.132:1020/1020-sangoslot-mobile/index.html";

            //連接OBS
            _obs = new OBSWebsocket();
            _obs.Connected += onConnect;
            _obs.Connect("ws://127.0.0.1:4444", "123456");

        }

        private void onConnect(object sender, EventArgs e)
        {
            //更換影片放置資料夾(目前沒有成功)
            //_obs.SetRecordingFolder("C:/Users/RayJi/Desktop/ssss/");
            //string aa = _obs.GetRecordingFolder();

            string profile1 = _obs.GetCurrentProfile();

            OBSScene scene = _obs.GetCurrentScene();

            this.isConnect = true;
        }

        void WebView_JSDemoAbout(object sender, JSExtInvokeArgs e)
        {
            string aa = _obs.GetRecordingFolder();

            if (this.isConnect == false)
            {
                Console.WriteLine("尚未連線");
            }
            string command = e.Arguments[0] as string;
            if (command == "start_record")
            {
                _obs.StartRecording();
            }
            else
            {
                _obs.StopRecording();

                //等待5秒存檔，並上傳FTP
                wait(5000);
                this.UploadFtpFile("Client\\Test\\Ray", "D:\\git\\VisualStudio\\WebViewTest\\WebViewTest\\video\\aaa.ts");
            }
        }

        void wait(int milliseconds)
        {
            var timer1 = new System.Windows.Forms.Timer();
            if (milliseconds == 0 || milliseconds < 0) return;

            // Console.WriteLine("start wait timer");
            timer1.Interval = milliseconds;
            timer1.Enabled = true;
            timer1.Start();

            timer1.Tick += (s, e) =>
            {
                timer1.Enabled = false;
                timer1.Stop();
                // Console.WriteLine("stop wait timer");
            };

            while (timer1.Enabled)
            {
                Application.DoEvents();
            }
        }

        public void UploadFtpFile(string folderName, string fileName)
        {

            FtpWebRequest request;

            string absoluteFileName = Path.GetFileName(fileName);
            
            request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/{2}", this.ftpIP, folderName, absoluteFileName))) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = true;
            request.Credentials = new NetworkCredential(this.ftpAccount, this.ftpPassword);
            request.ConnectionGroupName = "group";

            using (FileStream fs = File.OpenRead(fileName))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Flush();
                requestStream.Close();
            }
        }

        private void webControl1_Click(object sender, EventArgs e)
        {

        }
    }
}
