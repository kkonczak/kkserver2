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
    /// Interaction logic for TextBoxWithRegExWindow.xaml
    /// </summary>
    public partial class TextBoxWithRegExWindow : Window
    {
        public List<string> requiredGroups;
        public TextBoxWithRegExWindow()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshResults();
        }

        void RefreshResults()
        {
            try
            {
 try
            {
 var reg = new System.Text.RegularExpressions.Regex(textBox.Text);
            bool math = reg.IsMatch(textBox1.Text);
            label2.Content = (math ? "Result: true" : "Result: false");
                    if (requiredGroups != null)
                    {
                        for(int i = 0; i < requiredGroups.Count; i++)
                        {
                            label2.Content +="\n"+ requiredGroups[i] + ": " + reg.Match(textBox1.Text).Groups[requiredGroups[i]];
                        }
                    }
                    button.IsEnabled = true;
            }catch
            {
                label2.Content = "Invalid regular expression!";
                    button.IsEnabled = false;
            }
            }
            catch { }
           
           
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshResults();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
