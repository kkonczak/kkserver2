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
    /// Interaction logic for PluginListItem.xaml
    /// </summary>
    public partial class PluginListItem : UserControl
    {
        public delegate void itemClickDelegate();
        public event itemClickDelegate itemClick;
        public bool IsItemClicked = false;
        private object _tag;
        public PluginListItem()
        {
            InitializeComponent();
        }

        public object tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }
        public void Collapse()
        {
            options.Visibility = Visibility.Collapsed;
            IsItemClicked = false;
            Background = Brushes.White;
        }
        public void Show()
        {
            options.Visibility = Visibility.Visible;
            IsItemClicked = true;
            Background = new LinearGradientBrush(Color.FromRgb(170, 221, 255), Colors.White, 90);
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            itemClick();
        }
    }
}
