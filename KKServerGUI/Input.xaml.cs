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
using System.Windows.Shapes;

namespace KKServerGUI
{
    /// <summary>
    /// Interaction logic for Input.xaml
    /// </summary>
    public partial class Input : Window
    {
        public Input()
        {
            InitializeComponent();
        }
        static string val="";
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            val = textBox.Text;
            DialogResult = true;
        }
        public static string showDialog(string input)
        {
            var window = new Input();
            window.textBlock.Text  = input;
            window.ShowDialog();
            return val  ;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            val = "";
            DialogResult = true;
        }
    }
}
