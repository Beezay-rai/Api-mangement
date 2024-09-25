namespace Yarp.Models
{
    public class PluginModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string NameSpace { get; set; }
        public int Version { get; set; }
        public string TimeStamp { get; set; }
        public IFormFile DLLFile {  get; set; }    
    }
    public class CreatePluginModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string NameSpace { get; set; }
        public int Version { get; set; }
        public string TimeStamp { get; set; }
        public IFormFile DLLFile {  get; set; }    
    }
    public class GETPluginModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Config { get; set; }
        public string _NameSpace { get; set; }
        public int Version { get; set; }
        public string TimeStamp { get; set; }
        public string DLLFile {  get; set; }    
    }
}
