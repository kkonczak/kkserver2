using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using pluginInterface; //Add reference to file "pluginInterface"

namespace pluginFilter
{
    [pluginInterface.KKServerPluginClass] //Your main class
    public class Class1 : pluginInterface.IPlugin, pluginInterface.IRequestManage, pluginInterface.ISettingsInfo, pluginInterface.IPluginImage
    {
        PluginSettingsGet settingsGet;          //delegate to function
        PluginSettingsRemove settingsRemove;    
        PluginSettingsSet settingsSet;

        string IPlugin.Author                   //author this plugin
        {
            get
            {
                return "Krzysztof Kończak";
            }
        }

        string IPlugin.Description             //description this plugin
        {
            get
            {
                return "Block access some ip address to page.";
            }
        }

        Version IPlugin.MinKKServerVer          //min. version of KK Server
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }

        string IPlugin.Name                     //Name of plugin (show on plugin list)
        {
            get
            {
                return "IP access control";
            }
        }
        //callback to setting-manipulation functions
        PluginSettingsGet IPlugin.SettingsGet
        {
            set
            {
                settingsGet = value;
            }
        }

        PluginSettingsRemove IPlugin.SettingsRemove
        {
            set
            {
                settingsRemove = value;
            }
        }

        PluginSettingsSet IPlugin.SettingsSet
        {
            set
            {
                settingsSet = value;
            }
        }

        Version IPlugin.Version
        {
            get
            {
                return new Version(1, 0, 0, 0);
            }
        }

        //optional: set icon for plugin
        Image IPluginImage.Image
        {
            get
            {
                return Properties.Resources.pluginFilterImage;
            }
        }

        //return description for all settings
        List<SettingDescriptionBase> ISettingsInfo.SettingsInfo
        {
            get
            {

                List <pluginInterface.SettingDescriptionBase> list = new List<pluginInterface.SettingDescriptionBase>();
                list.Add(new pluginInterface.SettingsDescription.SettingIP() { name = "blockedIP", description="Blocked IP Address" });
                 return list;
            }
        }

        //manipulation after get headers;
        void IRequestManage.ModulesRequestManage(Socket socket, ref string request)
        {
            string addressip = (socket.RemoteEndPoint as System.Net.IPEndPoint).Address.ToString();
            if(blockedIPs!=null && blockedIPs.Count > 0)
            {
                if(blockedIPs.Contains(addressip))
                {
                    socket.Close();
                }
            }
           
        }
        List<string> blockedIPs;

        //is calling, when plugin is loaded
        void IPlugin.PluginLoaded()
        {
            blockedIPs = settingsGet("blockedIP", typeof(Class1));
        }
    }
}
