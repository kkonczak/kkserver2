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
    /// Interaction logic for TextBoxWithRegEx.xaml
    /// </summary>
    public partial class TextBoxWithRegEx : UserControl
    {
        public List<string> parameters;
        public TextBoxWithRegEx()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var window = new TextBoxWithRegExWindow();
            window.textBox.Text = textBox.Text;
            window.requiredGroups = parameters;
            if (window.ShowDialog()==true)
            {
 textBox.Text = window.textBox.Text;
           
            }
                
        }
        public string text
        {
            get
            {
                return textBox.Text;
            }
            set
            {
                textBox.Text = value;
            }
        }
    }
}
