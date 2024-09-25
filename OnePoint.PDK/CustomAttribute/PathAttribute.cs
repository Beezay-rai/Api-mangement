using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OnePoint.PDK.CustomAttribute
{
    public class PathAttribute : Attribute
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public PathAttribute(string method, string path)
        {
            Method = method;
            Path = path;
        }
    }
}
