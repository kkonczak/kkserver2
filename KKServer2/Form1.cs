using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KKServer2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Server ser = new Server(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 80);//(new byte[] { 192, 168, 43, 145 })
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //mimetype
            ser.Mimetypes.Add(new KKServer2.Server.Mimetype() { EndsWith = ".htm", Type = "text/html" });
            ser.Mimetypes.Add(new KKServer2.Server.Mimetype() { EndsWith = ".html", Type = "text/html" });
            ser.Mimetypes.Add(new KKServer2.Server.Mimetype() { EndsWith = ".php", Type = "text/html" });
            ser.Mimetypes.Add(new KKServer2.Server.Mimetype() { EndsWith = ".mp4", Type = "video/mp4" });
            //cgi do PHP
            ser.CgiList.Add(new KKServer2.Server.CgiItemInfo() { EndsWith = ".php", CGIpath = @"C:\php\php-cgi.exe" });
           
            //reszta ustawień
            ser.RootAddress = Application.StartupPath.Replace('\\','/') + "/documents";
            ser.ErrorsPagesAddress= Application.StartupPath + "\\errors";

            ser.LogsPath  = Application.StartupPath + "\\log.log";
            ser.CreateLogs = true;

            ser.NameOfRootDocuments.Add("index.html");
            ser.NameOfRootDocuments.Add("index.htm");
                  ser.NameOfRootDocuments.Add("index.php");

            ser.BrowseDirectories = true;
            ser.Version = new Version( Application.ProductVersion);
            ser.Start();

            //test modules
            // ser.modulesConnectionManage += (System.Net.Sockets.Socket s) => { s.Close(); /*System.Windows.Forms.MessageBox.Show(s.RemoteEndPoint.ToString()); */};
            //ser.modulesRequestManage += (System.Net.Sockets.Socket s,ref string str) => { str+="ttrrr"; };

        }
    }
}
