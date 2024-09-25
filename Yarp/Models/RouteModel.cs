using System.ComponentModel.DataAnnotations;
using Yarp.Library;

namespace Yarp.Models
{
    public class RouteModel
    {
        [Required]
        public string ClusterId { get; set; }
       
        public RouteMatchModel Match { get; set; }      
        public string AuthPolicy { get; set; }
        public string CorsPolicy { get; set; }
        public string RateLimiterPolicy { get; set; }
        public string? OutputCachePolicy { get; set; }

        public string TimeOutPolicy { get; set; }
        public Dictionary<string, string> Metadata { get; set; } 
        public List<Dictionary<string, string>> Transforms { get; set; } 

    }



    public class RouteMatchModel
    {
        public string Path { get; set; }
        [AllowedMethods]
        public string[] Method { get; set; }
    }

    public class PatchRouteMatchModel
    {
        public string? Path { get; set; }
        [AllowedMethods]
        public string[]? Method { get; set; }

    }

    public class UpdateRouteMdoel
    {
        [Required]
        public string ClusterId { get; set; }
      
        public RouteMatchModel Match { get; set; }
        public string? AuthorizationPolicy { get; set; }
        public string? CorsPolicy { get; set; }
        public string? RateLimiterPolicy { get; set; }
        public string? TimeoutPolicy { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public List<Dictionary<string, string>> Transforms { get; set; }

    }
    public class PatchRouteModel
    {
        public string? ClusterId { get; set; }
        public PatchRouteMatchModel Match { get; set; }
        public string? AuthorizationPolicy { get; set; }
        public string? CorsPolicy { get; set; }
        public string? OutputCachePolicy { get; set; }
        public string? RateLimiterPolicy { get; set; }
        public string? TimeoutPolicy { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public List<Dictionary<string, string>> Transforms { get; set; }
    }




    public class CreateRouteModel
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }

    }
}
