namespace Yarp.Models
{
    public class Plugins
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Config { get; set; }
        public string NameSpace { get; set; }
        public string DLLFile { get; set; }
        public string Version { get; set; }
        public string TimeStamp { get; set; }
      
    }

    public class PluginsRouteMap
    {
        public string Id { get; set; }
        public string PluginId { get; set; }
        public string RouteId { get; set; }
    }
}
