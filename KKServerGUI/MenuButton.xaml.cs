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
    /// Interaction logic for MenuButton.xaml
    /// </summary>
    public partial class MenuButton : UserControl
    {
        public MenuButton()
        {
            InitializeComponent();
        }
        public static readonly RoutedEvent clEv = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuButton));
        //public event 
        public event RoutedEventHandler click
        {
            add
            {
                AddHandler(clEv , value);
            }
            remove
            {
                RemoveHandler (clEv, value);
            }
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(clEv ));
        }
        public string Text
        {
            get
            {
                return textBlock.Text;
            }
            set
            {
                textBlock.Text = value;
            }
        }
        public System.Windows.Media.ImageSource  Image
        {
            get
            {
                return image.Source;
            }
            set
            {
                image.Source = value;
            }
        }
    }
}
