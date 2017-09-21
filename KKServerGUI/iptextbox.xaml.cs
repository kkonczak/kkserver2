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
    /// Interaction logic for iptextbox.xaml
    /// </summary>
    public partial class iptextbox : UserControl
    {
        public iptextbox()
        {
            InitializeComponent();
        }
        public System.Net.IPAddress address
        {
            get
            {
                return new System.Net.IPAddress(new byte[] { byte.Parse(octet1.Text), byte.Parse(octet2.Text), byte.Parse(octet3.Text), byte.Parse(octet4.Text) });
            }
            set
            {
                byte[] b = value.GetAddressBytes();
                octet1.Text = b[0].ToString();
                octet2.Text = b[1].ToString();
                octet3.Text = b[2].ToString();
                octet4.Text = b[3].ToString();
            }
        }

        private void octet1_TextChanged(object sender, TextChangedEventArgs e)
        {
            check(sender);
        }

        private void octet1_KeyDown(object sender, KeyEventArgs e)
        {
            check(sender);
            if (e.Key == Key.Back)
            {
                if (((System.Windows.Controls.TextBox)sender).SelectionStart == 0)
                {
                    switch (((System.Windows.Controls.TextBox)sender).Name)
                    {
                        case "octet2":
                            {
                                System.Windows.Controls.TextBox el = octet1;
                                if (el.Text.Length > 0)
                                {
                                    el.Text = el.Text.Substring(0, el.Text.Length);
                                }
                                el.Focus();
                                el.Select(el.Text.Length, 0);
                            }

                            break;
                        case "octet3":
                            {
                                System.Windows.Controls.TextBox el = octet2;
                                if (el.Text.Length > 0)
                                {
                                    el.Text = el.Text.Substring(0, el.Text.Length);
                                }
                                el.Focus();
                                el.Select(el.Text.Length, 0);
                            }

                            break;
                        case "octet4":
                            {
                                System.Windows.Controls.TextBox el = octet3;
                                if (el.Text.Length > 0)
                                {
                                    el.Text = el.Text.Substring(0, el.Text.Length);
                                }
                                el.Focus();
                                el.Select(el.Text.Length, 0);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (((System.Windows.Controls.TextBox)sender).SelectionStart == 3)
                {
                    switch (((System.Windows.Controls.TextBox)sender).Name)
                    {
                        case "octet1":
                            {
                                TextBox el = octet2;
                                el.Focus();
                                el.SelectAll();
                            }
                            break;
                        case "octet2":
                            {
                                TextBox el = octet3;
                                el.Focus();
                                el.SelectAll();
                            }
                            break;
                        case "octet3":
                            {
                                TextBox el = octet4;
                                el.Focus();
                                el.SelectAll();
                            }
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        void check(object sender)
        {
            int val;
            bool ok = Int32.TryParse(((System.Windows.Controls.TextBox)sender).Text, out val);
            if (ok)
            {
                if (val > 255)
                {
                    ((System.Windows.Controls.TextBox)sender).Text = "255";
                    ((System.Windows.Controls.TextBox)sender).SelectAll();
                }
                else if (val < 0)
                {
                    ((System.Windows.Controls.TextBox)sender).Text = "0";
                    ((System.Windows.Controls.TextBox)sender).SelectAll();
                }
            }else
            {
                if (((System.Windows.Controls.TextBox)sender).Text == "")
                {

                }
                else
                {
((System.Windows.Controls.TextBox)sender).Text = "0";
                }
                
            }
        }
    }
}
