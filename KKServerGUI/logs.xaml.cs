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
    /// Interaction logic for loggs.xaml
    /// </summary>
    public partial class logs : Page
    {
        public logs()
        {
            InitializeComponent();
        }

        private void request_Click(object sender, RoutedEventArgs e)
        {
            frame.Children.Clear();
            frame.Children.Add(new request_line_chart() { HorizontalAlignment=HorizontalAlignment.Stretch, VerticalAlignment=VerticalAlignment.Stretch });
        }


        private void requesttime_Click(object sender, RoutedEventArgs e)
        {
            frame.Children.Clear();
            frame.Children.Add(new request_time_chart() { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch });

        }
    }
}
