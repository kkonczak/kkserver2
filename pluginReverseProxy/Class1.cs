using System;
using System.Collections.Generic;
using System.Net.Sockets;
using pluginInterface;

namespace pluginReverseProxy
{
    [KKServerPluginClass]
    public class Class1 : IPlugin, IRequestManage, ISettingsInfo
    {
        private PluginSettingsGet settingGet;
        private PluginSettingsRemove settingRemove;
        private PluginSettingsSet settingSet;
       private  List<string> requests;
       private List<server> redirectTo;
       private List<string> redirectPath;
        private bool rewriteHostHeader = false;
        private bool xForwarderForHeader = false;

        struct server
        {
            public string[] hosts;
            public int[] connections;
        }
        string IPlugin.Author
        {
            get
            {
                return "Krzysztof Kończak";
            }
        }

        string IPlugin.Description
        {
            get
            {
                return "The plug-in allows to use the server as a reverse-proxy";
            }
        }

        Version IPlugin.MinKKServerVer
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Reverse Proxy";
            }
        }

        PluginSettingsGet IPlugin.SettingsGet
        {
            set
            {
                settingGet = value;
            }
        }

        List<SettingDescriptionBase> ISettingsInfo.SettingsInfo
        {
            get
            {
                List<SettingDescriptionBase> settings = new List<SettingDescriptionBase>();
                settings.Add(new pluginInterface.SettingsDescription.SettingsRegEx () {name= "requestRegEx", isSimple=false, description= "A regular expression that specifies what address will be redirected.\n Example: \\/file_to_redirection\\/(?<path>.*)", requiredGroups=new List<string>() {"path"} });
                settings.Add(new pluginInterface.SettingsDescription.SettingString() { name = "redirectHostName", isSimple = false, description = "If the regular expression matches the query, forward it to the server (IP or domain name). If you want to balance traffic, split the domain with a semicolon.\nExample: example1.com;example2.com" });
                settings.Add(new pluginInterface.SettingsDescription.SettingString() { name = "redirectPath", isSimple = false, description = "The path on the server to which traffic will be redirected. Example: /remoteDir/" });
                settings.Add(new pluginInterface.SettingsDescription.SettingTrueFalse() { name = "hostHeaderRewrite", isSimple = true, description = "Do you change the \"host\" header? (true/false)" });
                settings.Add(new pluginInterface.SettingsDescription.SettingTrueFalse() { name = "xForwardedForHeader", isSimple = true, description = "Add a \"X-Forwarded-For\" heading to the query? (true/false)" });
                return settings;
            }
        }

        PluginSettingsRemove IPlugin.SettingsRemove
        {
            set
            {
                settingRemove = value;
            }
        }

        PluginSettingsSet IPlugin.SettingsSet
        {
            set
            {
                settingSet = value;
            }
        }

        Version IPlugin.Version
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }


        void IRequestManage.ModulesRequestManage(Socket socket, ref string request)
        {
          
            string[] parts = request.Split('\n');
            string path = parts[0].Split(' ')[1];
            if (requests != null && requests.Count > 0)
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(requests[i]);
                    // debug(rg.IsMatch(path).ToString()+ " - "+path );
                    if (rg.IsMatch(path))
                    {
                        //pasuje do wyrażenia, więc można przekazywać do zewnętrznego serwera proxy
                        System.Net.Sockets.TcpClient client;
                        int minConnectioni = 0;
                        int minConnectionNum = int.MaxValue ;
                        for(int j = 0; j < redirectTo[i].connections.Length ; j++)
                        {
                            if(redirectTo[i].connections[j] < minConnectionNum)
                            {
                                minConnectionNum = redirectTo[i].connections[j];
                                minConnectioni = j;
                            }
                        }
                        client = new TcpClient(redirectTo[i].hosts[minConnectioni], 80);
                        try
                        {
                            path = redirectPath[i] + rg.Match(path).Groups["path"];
                            parts[0] = parts[0].Split(' ')[0] + " " + path + " HTTP/1.1\r";

                            if (rewriteHostHeader)
                            {
                                //jeśli trzeba nadpisać nagłówek "host"
                                for (int q = 0; q < parts.Length || parts[q] == "\r"; q++)
                                {
                                    if (parts[q].StartsWith("Host: "))
                                    {
                                        parts[q] = "Host: " + redirectTo[i].hosts[minConnectioni] + "\r";
                                        break;
                                    }
                                }
                            }

                            request = string.Join("\n", parts);
                            if(xForwarderForHeader)
                            {
                                request =request.Insert(request.IndexOf('\n')+1,"X-Forwarder-For: "+ ((System.Net.IPEndPoint  )(socket.RemoteEndPoint)).Address.ToString()+"\r\n");
                            }

                            NetworkStream stream = client.GetStream();
                            stream.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(request), 0, request.Length);
                            byte[] result = new byte[UInt16.MaxValue];
                            int available;

                            DateTime now = DateTime.Now;

                            while ((DateTime.Now - now).TotalMilliseconds < 2000)
                            {
                                if (stream.DataAvailable)
                                {
                                    available = stream.Read(result, 0, result.Length);
                                    //przekazywanie do naszego gniazda
                                    if (available > 0)
                                    {
                                        socket.Send(result, 0, available, SocketFlags.None);
                                        now = DateTime.Now;
                                    }
                                }

                                System.Threading.Thread.Sleep(1);

                            }
                            client.Close();
                            socket.Close();
                           
                        }
                        catch (Exception)
                        {

                        }
                        redirectTo[i].connections[minConnectioni]--;


                    }
                }
            }
        }

        void IPlugin.PluginLoaded()
        {
            requests = settingGet("requestRegEx", typeof(Class1));
            redirectPath = settingGet("redirectPath", typeof(Class1));
            List<string> redirectToString = settingGet("redirectHostName", typeof(Class1));
            redirectTo = new List<server>();
            for (int i = 0; i < redirectToString.Count; i++)
            {
                string[] servers = redirectToString[i].Split(';');
                redirectTo.Add(new pluginReverseProxy.Class1.server() { hosts = (string[])servers.Clone(), connections=new int[servers.Length]  });
            }

            if (settingGet("hostHeaderRewrite", typeof(Class1)) != null)
            {
                if(settingGet("hostHeaderRewrite", typeof(Class1))[0] == "true")
                {
                    rewriteHostHeader = true;
                }else
                {
                    rewriteHostHeader = false;
                }
            }else
            {
                rewriteHostHeader = false;
            }
            if (settingGet("xForwardedForHeader", typeof(Class1))!=null)
            {
                if (settingGet("xForwardedForHeader", typeof(Class1))[0] == "true")
                {
                    xForwarderForHeader  = true;
                }
                else
                {
                    xForwarderForHeader = false;
                }
            }else
            {
                xForwarderForHeader = false;
            }
        }

        
    }
}
