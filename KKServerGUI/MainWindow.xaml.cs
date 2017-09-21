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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
            frame.Content = new StartPage();
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            frame.Content = new StartPage();
        }

        private void btnSettings_click(object sender, RoutedEventArgs e)
        {
            frame.Content = new Settings();
        }

        private void btnLoggs_click(object sender, RoutedEventArgs e)
        {
            frame.Content = new logs();
        }

        private void btnInfo_click(object sender, RoutedEventArgs e)
        {
            frame.Content = new infoPage();
        }

        private void btnPlugins_click(object sender, RoutedEventArgs e)
        {
            frame.Content = new Plugins();
        }
    }
}
