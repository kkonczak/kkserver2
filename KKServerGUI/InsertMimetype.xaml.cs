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
    /// Interaction logic for InsertMimetype.xaml
    /// </summary>
    public partial class InsertMimetype : Window
    {
        public InsertMimetype()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
