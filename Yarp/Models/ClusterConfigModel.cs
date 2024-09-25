using System.Security.Authentication;
using Yarp.Library;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.Models
{
    public class ClusterConfigModel
    {
        public string ClusterId { get; set; }
        public Dictionary<string, string> DestinationAddress { get; set; }
        public string LoadBalancingPolicy { get; set; }
        public ClusterHttpClientConfigModel HttpClient { get; set; }
    }

    public class ClusterHttpClientConfigModel
    {
        [SslProtocolValidator]
        public string[] SSLProtocols { get; set; }
        public bool DangerousAcceptAnyServerCertificate { get; set; }

        public SslProtocols[] GetProtocolEnum()
        {
          
            List<SslProtocols> returnVal = new List<SslProtocols>();

            // Supported SSL protocols
            string[] supportedProtocols = { SslProtocols.Tls12.ToString(), SslProtocols.Tls13.ToString() };

       
            foreach (var protocol in SSLProtocols)
            {
           
                if (Array.Exists(supportedProtocols, p => p.Equals(protocol, StringComparison.OrdinalIgnoreCase)))
                {
                    
                    if (Enum.TryParse(protocol, true, out SslProtocols parsedProtocol))
                    {
                        returnVal.Add(parsedProtocol);
                    }
                }
            }

            return returnVal.ToArray();
        }
    }

    public class CreateClusterConfigModel
    {
        public Dictionary<string, string> DestinationAddress { get; set; }
        [LoadBalancingPolicy]
        public string LoadBalancingPolicy { get; set; }
        public ClusterHttpClientConfigModel HttpClient { get; set; }
    }
    




    public class PutClusterConfigModel
    {
        public Dictionary<string, string> DestinationAddress { get; set; }
        public string LoadBalancingPolicy { get; set; }
        public ClusterHttpClientConfigModel HttpClient { get; set; }
    }

}
