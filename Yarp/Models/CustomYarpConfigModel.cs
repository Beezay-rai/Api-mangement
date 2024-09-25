using Yarp.ReverseProxy.Configuration;

namespace Yarp.Models
{
    public class CustomYarpConfigModel
    {
        public List<RouteModel> Routes { get; set; }
        public List<ClusterConfigModel> Clusters { get; set; }
    }
    public class YarpModel
    {
        public Dictionary<string, RouteConfig> Routes { get; set; }
        public Dictionary<string, ClusterConfig> Clusters { get; set; }
    }
}
