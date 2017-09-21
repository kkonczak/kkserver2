using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KKServerGUI
{
    
    class Class1
    {
        static System.Diagnostics.Process serverProces;

        public static void findHTTPD()
        {
            var prc = System.Diagnostics.Process.GetProcessesByName("httpd");
            if (prc.Length == 1)
            {
                //można sprawdzić czy to ten plik akurat
 serverProces =prc[0];
            }else
            {
                serverProces = null;
            }
           
        }
        public static bool isRunning()
        {
            findHTTPD();
            if (serverProces==null)
            {
                return false;
            }else
            {
                return true;
            }
        }
        public static TimeSpan uptime()
        {
            return (DateTime.Now - serverProces.StartTime);
        }
        public static  Int64 getMemoryUsage()
        {
            return serverProces.PrivateMemorySize64;
        }
        private static  TimeSpan lastCPUUsage = new TimeSpan(0, 0, 0, 0, 0);
       

        public static double getProcessorUsage(int interval)
        {
            TimeSpan current = serverProces.TotalProcessorTime;
            double res = (double)((current - lastCPUUsage).Milliseconds) / (double)interval ;
            lastCPUUsage = current;
            return res ;
        }
        public static void shutDown()
        {
            if (serverProces != null)
            {
 serverProces.Kill();
            }
           
        }
        public static void run()
        {
            serverProces = new System.Diagnostics.Process();
            serverProces.StartInfo.FileName = System.AppDomain.CurrentDomain.DynamicDirectory + "httpd.exe";
            serverProces.StartInfo.UseShellExecute = false;
            serverProces.StartInfo.CreateNoWindow = true;
            serverProces.Start();
        }
        public static void restartServer()
        {
            if (serverProces != null)
            {
                string pathname = serverProces.MainModule.FileName;
                serverProces.Kill();
                serverProces.StartInfo.FileName = pathname;
                serverProces.StartInfo.UseShellExecute = false;
                serverProces.StartInfo.CreateNoWindow = true;
                serverProces.Start();
            }
        }
    }
}
