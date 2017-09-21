using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KKServerGUI
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        public StartPage()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0,0,0,0,1000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            bool state = KKServerGUI.Class1.isRunning();
            textBlock2.Text = (state ? "running" : "off");
            if (state)
            {
                button1.Text = "Shut down";
                uptime.Text = KKServerGUI.Class1.uptime().ToString(@"hh\:mm\:ss");
                cpuusage.Text = (KKServerGUI.Class1.getProcessorUsage(1000) * 100).ToString("N2") + "%";
                memoryusage.Text = ((double)KKServerGUI.Class1.getMemoryUsage()/1024/1024).ToString("N1")+"MB";
            }else
            {
                button1.Text = "Run server";
            }
        }

        private void button1_click(object sender, RoutedEventArgs e)
        {
            bool state = KKServerGUI.Class1.isRunning();
            if(state)
            {
                KKServerGUI.Class1.shutDown();
            }else
            {
                KKServerGUI.Class1.run();
            }
        }
    }
}
