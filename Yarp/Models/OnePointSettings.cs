using System.Reflection;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.Models
{
    public class OnePointSettings
    {
        public SecuritySettings SecuritySettings { get; set; }
        public PluginSettings PluginSettings { get; set; }
    }

    public class PluginConfig
    {
        public bool PluginEnabled { get; set; }
        public IList<Plugins> PluginList { get; set; }
        public Dictionary<string, Assembly> Plugins { get; set; }
        public Dictionary<string, RouteConfig> Routes { get; set; }
        public List<PluginsRouteMap> PluginRouteMap { get; set; }
    }

    public class SecuritySettings
    {
        public ClientCredentialsSettings ClientCredentialsSettings { get; set; }
        public BasicAuthSettings BasicAuthSettings { get; set; }
        public JwtSettings JwtSettings { get; set; }
    }
    public class ClientCredentialsSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class BasicAuthSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenValidityInMinutes { get; set; }
        public int RefreshTokenValidityInDays { get; set; }
    }

    public class PluginSettings
    {
        public string DllPath { get; set; }
    }
}
