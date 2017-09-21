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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var path = new System.Windows.Forms.FolderBrowserDialog();
            if (path.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rootpath.Text = path.SelectedPath;
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var path = new System.Windows.Forms.FolderBrowserDialog();
            if (path.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                errorpath.Text = path.SelectedPath;
            }
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            System.Xml.XmlDocument sett = new System.Xml.XmlDocument();
            try
            {
                sett.Load(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                ipaddress.address = System.Net.IPAddress.Parse(sett.SelectSingleNode("settings/server/ip").InnerText);
                port.Text = sett.SelectSingleNode("settings/server/port").InnerText;
                rootpath.Text = sett.SelectSingleNode("settings/server/wwwdirectory").InnerText;
                errorpath.Text = sett.SelectSingleNode("settings/server/errorsdirectory").InnerText;
                browsedirectories.IsChecked = (sett.SelectSingleNode("settings/server/browseDirectories").InnerText == "true" ? true : false);
                var prms = sett.SelectNodes("settings/server/param");
                for (int i = 0; i < prms.Count; i++)
                {
                    if (prms[i].Attributes["name"].InnerText == "headerTimeout")
                    {
                        headertimeout.Text = prms[i].Attributes["value"].InnerText;
                    }
                    else if (prms[i].Attributes["name"].InnerText == "bodyOfRequestTimeout")
                    {
                        bodytimeout.Text = prms[i].Attributes["value"].InnerText;
                    }
                }
                var defaultpg = sett.SelectNodes("settings/server/rootDocuments/file");
                for (int i = 0; i < defaultpg.Count; i++)
                {
                    listBox.Items.Add(defaultpg[i].Attributes["filename"].InnerText);
                }

                var interpreters = sett.SelectNodes("settings/server/cgiinterpreters/interpreter");
                for (int i = 0; i < interpreters.Count; i++)
                {
                    listView.Items.Add(new interpreteritem() { endswith = interpreters[i].Attributes["endswith"].InnerText, path = interpreters[i].Attributes["interpreterpath"].InnerText });
                }

                var mimetypes = sett.SelectNodes("settings/server/mimetypes/type");
                for (int i = 0; i < mimetypes.Count; i++)
                {
                    listView2.Items.Add(new mimetypeitem() { endswith = mimetypes[i].Attributes["endswith"].InnerText, mimetype = mimetypes[i].Attributes["mimetype"].InnerText });
                }
               

            }
            catch
            {

            }

        }

        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            string wynik = Input.showDialog("File name:");
            if (wynik != "")
            {
                listBox.Items.Add(wynik);
            }
            // w.show

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Remove(listBox.SelectedItem);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex > 0)
            {
                string current = (string)listBox.SelectedItem;
                int i = listBox.SelectedIndex;
                listBox.Items.Remove(current);
                listBox.Items.Insert(i - 1, current);
                listBox.SelectedItem = current;
            }

        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1 && listBox.SelectedIndex >= 0)
            {
                string current = (string)listBox.SelectedItem;
                int i = listBox.SelectedIndex;
                listBox.Items.Remove(current);
                listBox.Items.Insert(i + 1, current);
                listBox.SelectedItem = current;
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            var window = new insertinterpreter();
            if (window.ShowDialog() == true)
            {
                if (window.endswith.Text != "" && window.path.Text != "")
                {
                    listView.Items.Add(new interpreteritem() { endswith = window.endswith.Text, path = window.path.Text });
                }
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedIndex != -1)
            {
                listView.Items.Remove(listView.SelectedItem);
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            var window = new InsertMimetype();
            if (window.ShowDialog() == true)
            {
                if (window.endswith.Text != "" && window.mimetype.Text != "")
                {
                    listView2.Items.Add(new mimetypeitem() { endswith = window.endswith.Text, mimetype = window.mimetype.Text });
                }

            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            if (listView2.SelectedIndex != -1)
            {
                listView2.Items.Remove(listView2.SelectedItem);
            }
        }

        private void MenuButton_click(object sender, RoutedEventArgs e)
        {
            System.Xml.XmlDocument sett = new System.Xml.XmlDocument();
            try
            {
                sett.Load(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                sett.SelectSingleNode("settings/server/ip").InnerText = ipaddress.address.ToString();
                sett.SelectSingleNode("settings/server/port").InnerText = port.Text;
                sett.SelectSingleNode("settings/server/wwwdirectory").InnerText = rootpath.Text;
                sett.SelectSingleNode("settings/server/errorsdirectory").InnerText = errorpath.Text;
                sett.SelectSingleNode("settings/server/browseDirectories").InnerText = (browsedirectories.IsChecked==true?"true":"false");
                var prms = sett.SelectNodes("settings/server/param");
                for (int i = 0; i < prms.Count; i++)
                {
                    if (prms[i].Attributes["name"].InnerText == "headerTimeout")
                    {
                         prms[i].Attributes["value"].InnerText= headertimeout.Text ;
                    }
                    else if (prms[i].Attributes["name"].InnerText == "bodyOfRequestTimeout")
                    {
                         prms[i].Attributes["value"].InnerText= bodytimeout.Text;
                    }
                }

                var defaultpg = sett.SelectSingleNode("settings/server/rootDocuments");
                defaultpg.RemoveAll();
                for(int i = 0; i < listBox.Items.Count; i++)
                {
                    var node =  sett.CreateElement("file");
                    node.SetAttribute("filename", (string)listBox.Items[i]);
                    defaultpg.AppendChild(node);
                }

                var interpreters = sett.SelectSingleNode("settings/server/cgiinterpreters");
                interpreters.RemoveAll();
                for (int i = 0; i < listView.Items.Count; i++)
                {
                    var node = sett.CreateElement("interpreter");
                    node.SetAttribute("endswith", ((interpreteritem)listView.Items[i]).endswith);
                    node.SetAttribute("interpreterpath", ((interpreteritem)listView.Items[i]).path );
                    interpreters.AppendChild(node);
                }

                var mimetypes = sett.SelectSingleNode("settings/server/mimetypes");
                mimetypes.RemoveAll();
                for (int i = 0; i < listView2.Items.Count ; i++)
                {
                    var node = sett.CreateElement("type");
                    node.SetAttribute("endswith", ((mimetypeitem)listView2.Items[i]).endswith);
                    node.SetAttribute("mimetype", ((mimetypeitem)listView2.Items[i]).mimetype );
                    mimetypes.AppendChild(node);
                    //listView2.Items.Add(new mimetypeitem() { endswith = mimetypes[i].Attributes["endswith"].InnerText, mimetype = mimetypes[i].Attributes["mimetype"].InnerText });
                }

                sett.Save(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                KKServerGUI.Class1.restartServer();
            }
            catch
            {

            }
        }
    }
    public class interpreteritem
    {
        string _ends;
        string _path;
        public string endswith
        {
            get
            {
                return _ends;
            }
            set
            {
                _ends = value;
            }
        }

        public string path
        {
            get
            {
                return _path ;
            }
            set
            {
                _path  = value;
            }
        }
    }
    public class mimetypeitem
    {
        string _ends;
        string _mimetype;
        public string endswith
        {
            get
            {
                return _ends;
            }
            set
            {
                _ends = value;
            }
        }

        public string mimetype
        {
            get
            {
                return _mimetype;
            }
            set
            {
                _mimetype = value;
            }
        }
    }
}
