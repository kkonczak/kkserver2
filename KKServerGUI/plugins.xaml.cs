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
using System.Xml;
using pluginInterface;

namespace KKServerGUI
{
    /// <summary>
    /// Interaction logic for plugins.xaml
    /// </summary>
    public partial class Plugins : Page
    {
        public Plugins()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            MakePluginList();
        }
        private void MakePluginList()
        {
            listOfPlugins.reset();
            if (new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins.xml").Exists)
            {
                XmlDocument plugins = new XmlDocument();
                plugins.Load(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");
                var pluginsItems = plugins.SelectNodes("plugins/plugin");

                for (int i = 0; i < pluginsItems.Count; i++)
                {
                    //if (pluginsItems[i].Attributes["enabled"].InnerText == "true")
                    //{
                    //}
                    System.Windows.Media.ImageSource image;
                    image = null;
                    if (pluginsItems[i].SelectSingleNode("icon") == null)
                    {
                        var bitmapRes = Properties.Resources.pluginIcon;
                        var lockBits = bitmapRes.LockBits(new System.Drawing.Rectangle(0, 0, bitmapRes.Width, bitmapRes.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        image = BitmapSource.Create(bitmapRes.Width, bitmapRes.Height, bitmapRes.HorizontalResolution, bitmapRes.VerticalResolution, PixelFormats.Bgra32, null, lockBits.Scan0,lockBits.Stride*lockBits.Height , lockBits.Stride);
                        // Properties.Resources.pluginIcon;
                    }else
                    {
                        byte[] imgArray;
                        imgArray = Convert.FromBase64String(pluginsItems[i].SelectSingleNode("icon").InnerText);
                        image = BitmapSource.Create(80, 80, 96, 96, PixelFormats.Bgra32, null,imgArray,80*4);
                    }
                    listOfPlugins.addPlugin(pluginsItems[i].SelectSingleNode("name").InnerText, pluginsItems[i].SelectSingleNode("description").InnerText, image, Disable, (pluginsItems[i].Attributes["enabled"].InnerText == "true" ? true : false), Uninstall, pluginsItems[i].SelectSingleNode("classname").InnerText);
                }

            }
        }
        private void Disable(object plugin)
        {
            XmlDocument plugins = new XmlDocument();
            plugins.Load(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");
            var pluginsItems = plugins.SelectNodes("plugins/plugin");
            for (int i = 0; i < pluginsItems.Count; i++)
            {
                if (pluginsItems[i].SelectSingleNode("classname").InnerText == (string)plugin)
                {
                    if (pluginsItems[i].Attributes["enabled"].InnerText == "true")
                    {
                        pluginsItems[i].Attributes["enabled"].InnerText = "false";
                    }else
                    {
                        pluginsItems[i].Attributes["enabled"].InnerText = "true";
                    }
                    break;
                }
            }
            plugins.Save(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");
            MakePluginList();
            Class1.restartServer();
        }
        private void Uninstall(object plugin)
        {
            Class1.shutDown();
            XmlDocument plugins = new XmlDocument();
            plugins.Load(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");
            var pluginsItems = plugins.SelectNodes("plugins/plugin");
            string fileName="";
            for(int i = 0; i < pluginsItems.Count; i++)
            {
                if (pluginsItems[i].SelectSingleNode("classname").InnerText == (string)plugin)
                {
                    fileName = pluginsItems[i].SelectSingleNode("file").InnerText;
                    plugins.SelectSingleNode("plugins").RemoveChild(pluginsItems[i]);
                    break;
                }
            }
            //settings
            var pluginsSettings = plugins.SelectSingleNode("plugins/settings").ChildNodes;
            for(int i = 0; i < pluginsSettings.Count; i++)
            {
                if (pluginsSettings[i].Name == (string)plugin)
                {
                    plugins.SelectSingleNode("plugins/settings").RemoveChild(pluginsSettings[i]);
                }
            }
            System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "/plugins/" + fileName);

            plugins.Save(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");

            Class1.run();
            MakePluginList();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "KKServer plugins (*.dll)|*.dll";
            if(dialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;
                LoadPlugin(path);
                MakePluginList();
                Class1.restartServer();
            }
        }

        private void LoadPlugin(string path)
        {
            bool isKKServerPlugin = false;

            try
            {
 System.Reflection.AssemblyName name = System.Reflection.AssemblyName.GetAssemblyName(path);
            
           // pluginDomain.Load()
            System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(name);
            //szukanie klasy, gdzie jest klasa główna
            var types = assembly.GetExportedTypes();
             for (int q = 0; q < types.Length; q++)
            {
                var attrib = types[q].GetCustomAttributes(true);
                for (int w = 0; w < attrib.Length; w++)
                {

                    if (attrib[w].GetType() == typeof(pluginInterface.KKServerPluginClass))
                    {
                        isKKServerPlugin = true;
                        //aktywowanie klasy
                        object plugin = Activator.CreateInstance(types[q]);
                        string pluginName = ((pluginInterface.IPlugin)plugin).Name;
                        string pluginDescription = ((pluginInterface.IPlugin)plugin).Description;
                        string pluginVersion = ((pluginInterface.IPlugin)plugin).Version.ToString();
                        string pluginAuthor = ((pluginInterface.IPlugin)plugin).Author.ToString();
                        string pluginClassName = types[q].FullName;
                        //kopiowanie do folderu
                        string[] pathParts = path.Split(new char[] { '/', '\\' });
                        string pluginFileName = pathParts[pathParts.Length - 1];
                        System.IO.File.Copy(path, AppDomain.CurrentDomain.BaseDirectory + "plugins/" + pluginFileName, true);
                        XmlDocument pluginsSettings = new XmlDocument();
                        pluginsSettings.Load(AppDomain.CurrentDomain.BaseDirectory + "plugins.xml");
                        var elem = pluginsSettings.CreateElement("plugin");
                        elem.Attributes.Append(pluginsSettings.CreateAttribute("enabled"));
                        elem.Attributes["enabled"].InnerText = "true";
                        pluginsSettings.SelectSingleNode("plugins").AppendChild(elem);
                        var titleElement = pluginsSettings.CreateElement("name");
                        titleElement.InnerText = pluginName;
                        elem.AppendChild(titleElement);
                        var fileElement = pluginsSettings.CreateElement("file");
                        fileElement.InnerText = pluginFileName;
                        elem.AppendChild(fileElement);
                        var descriptionElement = pluginsSettings.CreateElement("description");
                        descriptionElement.InnerText = pluginDescription;
                        elem.AppendChild(descriptionElement);
                        var versionElement = pluginsSettings.CreateElement("version");
                        versionElement.InnerText = pluginVersion;
                        elem.AppendChild(versionElement);
                        var authorElement = pluginsSettings.CreateElement("author");
                        authorElement.InnerText = pluginAuthor;
                        elem.AppendChild(authorElement);
                        var classElement = pluginsSettings.CreateElement("classname");
                        classElement.InnerText = pluginClassName;
                        elem.AppendChild(classElement);
                        //jeśli zawiera zdjęcie
                        pluginInterface.IPluginImage image = plugin as pluginInterface.IPluginImage;
                        if (image != null)
                        {
                            System.Drawing.Image icon = image.Image;
                            string res;
                            var lockBits = ((System.Drawing.Bitmap)icon).LockBits(new System.Drawing.Rectangle(0, 0, icon.Width, icon.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            byte[] imgcpy = new byte[lockBits.Stride * lockBits.Height];
                            System.Runtime.InteropServices.Marshal.Copy(lockBits.Scan0, imgcpy, 0, imgcpy.Length);
                            res = Convert.ToBase64String(imgcpy);
                            var iconElement = pluginsSettings.CreateElement("icon");
                            iconElement.InnerText = res ;
                            elem.AppendChild(iconElement);
                        }
                        //kiedyś sprawdzanie uprawnień


                        pluginsSettings.Save(AppDomain.CurrentDomain.BaseDirectory + "plugins.xml");
                    }
                }
            }
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("IO Error!", "KKServer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid plugin file!", "KKServer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
            
            if (!isKKServerPlugin)
            {
                MessageBox.Show("Invalid plugin file!", "KKServer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }
    }
}
