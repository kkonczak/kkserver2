using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace pluginInterface
{
    public delegate void PluginSettingsSet(string key, string value, Type pluginClass);
    public delegate void PluginSettingsRemove(string key, Type pluginClass);
    public delegate List<string> PluginSettingsGet(string key, Type pluginClass);
    public interface IPlugin
    {
        string Name { get; }
        Version Version { get; }
        string Description { get; }
        string Author { get; }
        Version MinKKServerVer { get; }
        PluginSettingsGet SettingsGet { set; }
        PluginSettingsSet SettingsSet { set; }
        PluginSettingsRemove SettingsRemove { set; }
        void PluginLoaded();
    }
    public interface ISettingsInfo
    {
        List<SettingDescriptionBase> SettingsInfo { get; }
    }

   
    public interface IPluginImage
    {
        System.Drawing.Image Image { get; }
    }
    
    public interface IConnectionManage
    {
        void ModulesConnectionManage(System.Net.Sockets.Socket socket);
    }
    public interface IRequestManage
    {
        void ModulesRequestManage(System.Net.Sockets.Socket socket, ref string request);
    }
    public interface IDirectoryDisplay
    {
        void ModulesDirectoryDisplay(System.Net.Sockets.Socket socket, string address);
    }
    [System.AttributeUsage(AttributeTargets.Class)]
    public class KKServerPluginClass : System.Attribute
    {
        readonly bool isPluginClass = true;
    }

    public class SettingDescriptionBase
    {
        public string name;
        public string description;
        public bool isSimple;
    }
    namespace SettingsDescription
    {
        public class SettingString :SettingDescriptionBase
        {   
        }
        public class SettingInt : SettingDescriptionBase
        {
            public int minValue;
            public int maxValue;
        }
        public class SettingIP : SettingDescriptionBase
        {
        }
        public class SettingTrueFalse : SettingDescriptionBase
        {

        }
        public class SettingsRegEx : SettingDescriptionBase
        {
           public List<string> requiredGroups;
        }
    }
}
