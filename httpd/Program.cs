using KKServer2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace httpd
{
    class Program
    {
        static List<System.Reflection.Assembly> assemblies;
        static void Main(string[] args)
        {
            //wczytywanie ustawień
            XmlDocument sett = new XmlDocument();
            string addOfSett = AppDomain.CurrentDomain.BaseDirectory;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                sett.Load(addOfSett + "/config.xml");
                //pobieranie po kolei
                string ip = "127.0.0.1";
                byte[] ipb = new byte[4];
                string port = "80";
                int porti;
                ip = sett.SelectSingleNode("settings/server/ip").InnerText;
                port = sett.SelectSingleNode("settings/server/port").InnerText;

                porti = int.Parse(port);
                ipb[0] = byte.Parse(ip.Split('.')[0]);
                ipb[1] = byte.Parse(ip.Split('.')[1]);
                ipb[2] = byte.Parse(ip.Split('.')[2]);
                ipb[3] = byte.Parse(ip.Split('.')[3]);

                Server myServer = new Server(new System.Net.IPAddress(ipb), porti);
                //main
                string wwwDir;
                string errorDir;
                string browseDirs;
                wwwDir = parsePath(sett.SelectSingleNode("settings/server/wwwdirectory").InnerText);
                errorDir = parsePath(sett.SelectSingleNode("settings/server/errorsdirectory").InnerText);
                browseDirs = sett.SelectSingleNode("settings/server/browseDirectories").InnerText;

                myServer.RootAddress = wwwDir;
                myServer.ErrorsPagesAddress = errorDir;
                myServer.BrowseDirectories = (browseDirs == "true" ? true : false);

                //test
                // pluginSettingsSet("key1", "value1", typeof(Program));
                //pluginSettingsRemove("key1", typeof(Program));
                //var tmp = pluginSettingsGet("key2",typeof(Program));



                 //params
                 var paramss = sett.SelectNodes("settings/server/param");
                for(int i = 0; i < paramss.Count ; i++)
                {
                    if(paramss[i].Attributes["name"].InnerText == "headerTimeout")
                    {
                        myServer.HeaderTimeout = int.Parse(paramss[i].Attributes["value"].InnerText);
                    }else if (paramss[i].Attributes["name"].InnerText == "bodyOfRequestTimeout")
                    {
                        myServer.BodyOfRequestTimeout  = int.Parse(paramss[i].Attributes["value"].InnerText);
                    }
                    else if (paramss[i].Attributes["name"].InnerText == "maxNumberOfCGI")
                    {
                        myServer.MaxNumberOfCGI  = int.Parse(paramss[i].Attributes["value"].InnerText);
                    }
                    else if (paramss[i].Attributes["name"].InnerText == "CGIhandle")
                    {
                        myServer.IsCGIhandle = (paramss[i].Attributes["value"].InnerText=="true"?true:false);
                    }
                }

                //mimetypes
                var mimetypes = sett.SelectNodes("settings/server/mimetypes/type");
                for (int i = 0; i < mimetypes.Count; i++)
                {
                    myServer.Mimetypes.Add(new KKServer2.Server.Mimetype() { EndsWith = mimetypes[i].Attributes["endswith"].InnerText, Type = mimetypes[i].Attributes["mimetype"].InnerText });
                }
                var cgiinterpreters = sett.SelectNodes("settings/server/cgiinterpreters/interpreter");
                for (int i = 0; i < cgiinterpreters.Count; i++)
                {
                    myServer.CgiList.Add(new KKServer2.Server.CgiItemInfo() { EndsWith = cgiinterpreters[i].Attributes["endswith"].InnerText, CGIpath = cgiinterpreters[i].Attributes["interpreterpath"].InnerText });
                }
                var rootDocs = sett.SelectNodes("settings/server/rootDocuments/file");
                for (int i = 0; i < rootDocs.Count; i++)
                {
                    myServer.NameOfRootDocuments.Add(rootDocs[i].Attributes["filename"].InnerText);
                }
                var typeOfLogs = sett.SelectNodes("settings/server/logs/log");
                for (int i = 0; i < typeOfLogs.Count; i++)
                {
                    if (typeOfLogs[i].Attributes["type"].InnerText  == "main")
                    {
                        myServer.CreateLogs = true;
                        myServer.LogsPath = parsePath(typeOfLogs[i].Attributes["path"].InnerText);
                    }
                }


                XmlDocument plugins = new XmlDocument();
                plugins.Load(addOfSett + "/plugins.xml");
                var pluginsItems = plugins.SelectNodes("plugins/plugin");
                assemblies = new List<System.Reflection.Assembly>(pluginsItems.Count);
                for(int i = 0; i < pluginsItems.Count; i++)
                {
                    if(pluginsItems[i].Attributes["enabled"].InnerText == "true")
                    {
                        try
                        {
                            System.Reflection.AssemblyName name = System.Reflection.AssemblyName.GetAssemblyName(addOfSett + "plugins/" + pluginsItems[i].SelectSingleNode("file").InnerText);
                            assemblies.Add(System.Reflection.Assembly.Load(name));
                            //szukanie klasy, gdzie jest klasa główna
                            var types = assemblies[assemblies.Count - 1].GetExportedTypes();
                            for (int q = 0; q < types.Length; q++)
                            {
                                var attrib = types[q].GetCustomAttributes(true);
                                for (int w = 0; w < attrib.Length; w++)
                                {

                                    if (attrib[w].GetType() == typeof(pluginInterface.KKServerPluginClass))
                                    {
                                        //aktywowanie klasy
                                        object plugin = Activator.CreateInstance(types[q]);
                                        if (((pluginInterface.IPlugin)plugin).MinKKServerVer <= System.Reflection.Assembly.GetExecutingAssembly().GetName().Version)
                                        {
                                            //jest wersja odpowiednia
                                            pluginInterface.IConnectionManage connectionmg = plugin as pluginInterface.IConnectionManage;
                                            if (connectionmg != null)
                                            {
                                                myServer.ModulesConnectionManage += connectionmg.ModulesConnectionManage;
                                            }
                                            pluginInterface.IRequestManage requestmg = plugin as pluginInterface.IRequestManage;
                                            if (requestmg != null)
                                            {
                                                myServer.ModulesRequestManage += requestmg.ModulesRequestManage;
                                            }
                                            pluginInterface.IDirectoryDisplay directorymg = plugin as pluginInterface.IDirectoryDisplay;
                                            if (directorymg != null)
                                            {
                                                myServer.ModulesDirectoryDisplay += directorymg.ModulesDirectoryDisplay;
                                            }
                                            //set settings
                                            ((pluginInterface.IPlugin)plugin).SettingsGet = pluginSettingsGet;
                                            ((pluginInterface.IPlugin)plugin).SettingsRemove = pluginSettingsRemove;
                                            ((pluginInterface.IPlugin)plugin).SettingsSet = pluginSettingsSet;

                                            ((pluginInterface.IPlugin)plugin).PluginLoaded();
                                        }

                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            WriteErrorToLog("PluginError\t"+i+"\t"+DateTime.Now.ToString()+"\t"+ex.Message);
                        }
                        
                    }
                }
                

                myServer.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                myServer.Start();
                while (true)
                {
                    Console.ReadKey();
                }
            }
            catch
            {

            }

           
        }
        public static void pluginSettingsSet(string key, string value,Type pluginClass)
        {
            string pluginName = pluginClass.FullName;

            string addOfSett = AppDomain.CurrentDomain.BaseDirectory;

            XmlDocument plugins = new XmlDocument();
            plugins.Load(addOfSett + "/plugins.xml");
            var settings = plugins.SelectSingleNode("plugins/settings/" + pluginName);
            if (settings == null)
            {
                settings = plugins.CreateElement(pluginName);
                plugins.SelectSingleNode("plugins/settings").AppendChild(settings);
            }
            //dodawanie klucza
            var newKey = plugins.CreateElement("setting");
            newKey.Attributes.Append(plugins.CreateAttribute("key")).InnerText=key;
            newKey.Attributes.Append(plugins.CreateAttribute("value")).InnerText = value;
            settings.AppendChild(newKey);
            plugins.Save(addOfSett + "/plugins.xml");
        }
        public static void pluginSettingsRemove(string key, Type pluginClass)
        {
            string pluginName = pluginClass.FullName;
            string addOfSett = AppDomain.CurrentDomain.BaseDirectory;

            XmlDocument plugins = new XmlDocument();
            plugins.Load(addOfSett + "/plugins.xml");
            var settings = plugins.SelectSingleNode("plugins/settings/" + pluginName);
            if (settings!= null)
            {
                var elements = settings.SelectNodes("setting");
                for(int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Attributes["key"] != null && elements[i].Attributes["key"].InnerText==key )
                    {
                        settings.RemoveChild(elements[i]);
                    }
                }
            }
            plugins.Save(addOfSett + "/plugins.xml");

        }
        public static List<string> pluginSettingsGet(string key,Type pluginClass)
        {
            List<string> ret = new List<string>();
            string pluginName = pluginClass.FullName;
            string addOfSett = AppDomain.CurrentDomain.BaseDirectory;

            XmlDocument plugins = new XmlDocument();
            plugins.Load(addOfSett + "/plugins.xml");
            var settings = plugins.SelectSingleNode("plugins/settings/" + pluginName);
            if (settings != null)
            {
                var elements = settings.SelectNodes("setting");
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Attributes["key"] != null && elements[i].Attributes["key"].InnerText == key)
                    {
                        ret.Add(elements[i].Attributes["value"].InnerText);
                    }
                }
            }
            return ret;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            WriteErrorToLog("ApplicationError\t"+DateTime.Now.ToString() + "\t" + ex.Message.Replace("\n","")+"\t"+ ex.TargetSite);
        }
        private static void WriteErrorToLog(string content){
            var logapp = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "applog.log", true);
            logapp.WriteLine(content);
            logapp.Close();
        }

        public static string parsePath(string input)
        {
            return input.Replace("%STARTPATH%", AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
