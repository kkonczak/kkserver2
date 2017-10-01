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
    /// Interaction logic for PluginsList.xaml
    /// </summary>
    public partial class PluginsList : UserControl
    {
        public PluginsList()
        {
            InitializeComponent();
        }
        public void addPlugin(string name, string description, System.Windows.Media.ImageSource image, Action<object> enable, bool isEnable, Action<object> uninstall, object tag = null)
        {
            var elem = new PluginListItem();
            elem.title.Text = name;
            elem.description.Text = description;
            elem.image.Source = image;
            elem.uninstall.Click += (object sender, RoutedEventArgs e) => { uninstall(tag); };
            elem.disable.Click += (object sender, RoutedEventArgs e) => { enable(tag); };
            elem.settings.Click += (object sender, RoutedEventArgs e) => { PluginSettings settingsWindow = new PluginSettings(); settingsWindow.pluginName = (string)tag; settingsWindow.ShowDialog(); };
            elem.disable.Content = (isEnable ? "Disable" : "Enable");
            elem.tag = tag;
            elem.itemClick += () =>
             {
                 for (int i = 0; i < list.Children.Count; i++)
                 {
                     if (list.Children[i] is PluginListItem)
                     {
                         ((PluginListItem)list.Children[i]).Collapse();
                     }
                 }
                 ((PluginListItem)elem ).Show();
             };
            list.Children.Add(elem);
        }
        public void reset()
        {
            list.Children.Clear();
        }

    }
}
