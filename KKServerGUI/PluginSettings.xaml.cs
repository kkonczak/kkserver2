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
using System.Xml;

namespace KKServerGUI
{
    /// <summary>
    /// Interaction logic for PluginSettings.xaml
    /// </summary>
    public partial class PluginSettings : Window
    {
        public string pluginName;
        public PluginSettings()
        {
            InitializeComponent();
        }
        List<pluginInterface.SettingDescriptionBase> settingsDescription = new List<pluginInterface.SettingDescriptionBase>();
        Grid settingGrid = new Grid();
        private static readonly  Brush[] backgroundGroupsBrushes = new Brush[] {new SolidColorBrush(Color.FromArgb(100,255,0,0)), new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)), new SolidColorBrush(Color.FromArgb(100, 0, 0, 255)), new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)), new SolidColorBrush(Color.FromArgb(100, 255, 0, 255)), new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)), new SolidColorBrush(Color.FromArgb(100, 100, 0, 0)) };

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
 if (new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins.xml").Exists)
            {
                XmlDocument plugins = new XmlDocument();
                plugins.Load(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");
                var pluginsItems = plugins.SelectNodes("plugins/plugin");

                for (int i = 0; i < pluginsItems.Count; i++)
                {
                    if (pluginsItems[i].SelectSingleNode("classname").InnerText == pluginName)
                    {
                        System.Reflection.AssemblyName name = System.Reflection.AssemblyName.GetAssemblyName(AppDomain.CurrentDomain.BaseDirectory + "plugins/" + pluginsItems[i].SelectSingleNode("file").InnerText);

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
                                    object plugin = Activator.CreateInstance(types[q]);
                                    string pluginName = ((pluginInterface.IPlugin)plugin).Name;
                                    pluginInterface.ISettingsInfo info = plugin as pluginInterface.ISettingsInfo;
                                    if (info != null)
                                    {
                                        settingsDescription = info.SettingsInfo;
                                    }
                                    else
                                    {
                                        //biblioteka nie udostępnia informacji
                                    }

                                }
                            }
                        }

                    }
                }
            }


            {

                //odczytywanie obecnych ustawien
                XmlDocument pluginSettings = new XmlDocument();
                pluginSettings.Load(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");
                var settingItems = pluginSettings.SelectNodes("plugins/settings/" + pluginName + "/setting");
                
                settingGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Auto) });
                settingGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
settingGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                NoSettings.Visibility=Visibility.Collapsed;
                for (int i = 0; i < settingItems.Count; i++)
                {
                    AddItemOnList(settingItems[i].Attributes["key"].InnerText, settingItems[i].Attributes["value"].InnerText );
                }

                settingsList.Children.Add(settingGrid);
            }
            }
            catch 
            {
                MessageBox.Show("Cannot show settings for this plugin");
            }
           

        }
        private int numOfActiveGroup = 0;
        Dictionary<string,int> namesOfGroup = new Dictionary<string,int>();
        void AddItemOnList(string key, string value)
        {

            settingGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });
            pluginInterface.SettingDescriptionBase type = GetSettingType(key);
            var elem = GetGoodControl(type, value);
            Grid.SetColumn(elem, 1);
            Grid.SetRow(elem, settingGrid.RowDefinitions.Count-1);
            settingGrid.Children.Add(elem);
            var tb = new TextBlock();
            tb.Text = key;
            tb.VerticalAlignment = VerticalAlignment.Stretch;
            tb.Margin = new Thickness(3, 3, 3, 3);
            //tworzenie etykietki
            StackPanel toolTipStackPanel = new StackPanel();
            toolTipStackPanel.Children.Add(new TextBlock() { Text = type.name, FontWeight = FontWeights.Bold });
            toolTipStackPanel.Children.Add(new TextBlock() { Text = type.description });
            toolTipStackPanel.Children.Add(new TextBlock() { Text = "Multiple: " + (type.isSimple ? "false" : "true") });
            if (!type.isSimple)
            {
                if (!namesOfGroup.Keys.Contains(type.name)) { 
              
                    namesOfGroup.Add(type.name, 0);
                    numOfActiveGroup++;
                }
                tb.Background = backgroundGroupsBrushes[namesOfGroup[type.name] % backgroundGroupsBrushes.Length];
                namesOfGroup[type.name]++;
            }
            tb.ToolTip = toolTipStackPanel;//type.name +"\n"+type.description +"\nIs Multiple: "+(type.isSimple?"false":"true");
                                           //dodawanie
            settingGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, settingGrid.RowDefinitions.Count - 1);
        }
        UIElement GetGoodControl(Object type, string value)
        {
            if (type is pluginInterface.SettingsDescription.SettingIP)
            {
                iptextbox elem = new iptextbox();
                elem.BorderBrush = Brushes.Transparent;
                //elem.Margin = new Thickness(3, 3, 3, 3);
                try
                {
                    elem.address = System.Net.IPAddress.Parse(value);
                }
                catch { elem.address = new System.Net.IPAddress(new byte[] { 0, 0, 0, 0 }); }
                return elem;
            }
            else if (type is pluginInterface.SettingsDescription.SettingInt)
            {
                TextBox elem = new TextBox();
                elem.BorderBrush = Brushes.Transparent;
                //elem.Margin = new Thickness(3, 3, 3, 3);
                elem.PreviewTextInput += (object sender, System.Windows.Input.TextCompositionEventArgs   e) => {
                    System.Text.RegularExpressions.Regex regularExpr = new System.Text.RegularExpressions.Regex("[^0-9.-]+", System.Text.RegularExpressions.RegexOptions.None);
                    e.Handled=regularExpr.IsMatch(e.Text);
                };
                
                try
                {
                    elem.Text = Int64.Parse( value).ToString();
                }
                catch { elem.Text=((pluginInterface.SettingsDescription.SettingInt)type).minValue.ToString(); }
                return elem;
            }else if(type is pluginInterface.SettingsDescription.SettingTrueFalse)
            {
                CheckBox elem = new CheckBox();
                if (value == "true")
                {
                    elem.IsChecked = true;
                }else
                {
                    elem.IsChecked = false;
                }
                return elem;
            }else if(type is pluginInterface.SettingsDescription.SettingsRegEx)
            {
                TextBoxWithRegEx elem = new KKServerGUI.TextBoxWithRegEx();
                elem.parameters = ((pluginInterface.SettingsDescription.SettingsRegEx)type).requiredGroups;
                elem.text = value;
                return elem;
            }
            else
            {
                TextBox elem = new TextBox();
                elem.BorderBrush = Brushes.Transparent;
                // elem.Margin = new Thickness(3, 3, 3, 3);
                elem.Text = value;
                return elem;
            } 
        }
        string ElementToSettingValue(UIElement element)
        {
            if(element is TextBox)
            {
                return ((TextBox)element).Text;
            }
            else if (element is iptextbox )
            {
                return ((iptextbox )element).address.ToString();
            }else if(element is CheckBox)
            {
                return (((CheckBox)element).IsChecked==true ? "true" : "false");
            }else if(element is TextBoxWithRegEx)
            {
                return ((TextBoxWithRegEx)element).text;
            }
            return "";
        }

        pluginInterface.SettingDescriptionBase GetSettingType(string key)
        {
            for (int i = 0; i < settingsDescription.Count; i++)
            {
                if (settingsDescription[i].name == key)
                {
                    return settingsDescription[i];
                }
            }
            return null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument pluginSettings = new XmlDocument();
            pluginSettings.Load(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");
            if(pluginSettings.SelectSingleNode("plugins/settings/" + pluginName) == null)
            {
                pluginSettings.SelectSingleNode("plugins/settings").AppendChild(pluginSettings.CreateElement(pluginName));
            }
            pluginSettings.SelectSingleNode("plugins/settings/" + pluginName).RemoveAll();

            var children = settingsList.Children;
           
                    var grid = settingGrid;
                    for(int j = 0; j < grid.Children.Count; j++)
                    {
                        if(!(grid.Children[j] is TextBlock))
                        {
                            string name = ((TextBlock)grid.Children[j + 1]).Text;
                            string value = ElementToSettingValue(grid.Children[j]);
                            XmlElement setting = pluginSettings.CreateElement("setting");
                            setting.Attributes.Append(pluginSettings.CreateAttribute("key"));
                            setting.Attributes.Append(pluginSettings.CreateAttribute("value"));
                            setting.Attributes["key"].InnerText  = name;
                            setting.Attributes["value"].InnerText = value;
                            pluginSettings.SelectSingleNode("plugins/settings/"+pluginName ).AppendChild(setting);
                        }
                    }
      
            pluginSettings.Save(AppDomain.CurrentDomain.BaseDirectory + "/plugins.xml");
        }

        void ComboboxSettingsAvailable()
        {
            if (settingsDescription != null)
            {
 SettingsAddingList.Items.Clear();
            SettingsAddingList.Items.Add("");
            for (int i = 0; i < settingsDescription.Count ; i++)
            {
                if (settingsDescription[i].isSimple == false)
                {
                    SettingsAddingList.Items.Add(settingsDescription[i].name);
                }else
                {
                    var children = settingsList.Children;
                    bool isItemFound = false;
                   
                            for (int j = 0; j < settingGrid.Children.Count && !isItemFound; j++)
                            {
                                if ((settingGrid.Children[j] is TextBlock))
                                {
                                    if(((TextBlock )settingGrid.Children[j]).Text== settingsDescription[i].name)
                                    {
                                        isItemFound = true;
                                    }
                                }
                         
                    }
                    if(!isItemFound)
                    {
                        SettingsAddingList.Items.Add(settingsDescription[i].name);
                    }
                                }
            }
            }
           
        }

        private void SettingsAddingList_DropDownOpened(object sender, EventArgs e)
        {
            ComboboxSettingsAvailable();
        }

        private void SettingsAddingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingsAddingList.SelectedIndex!=-1 && (string)SettingsAddingList.SelectedItem != "")
            {
                AddItemOnList((string)SettingsAddingList.SelectedItem, "");
                SettingsAddingList.SelectedIndex = 0;
            }
        }
    }
}
