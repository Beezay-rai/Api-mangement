using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePoint.PDK.Models
{
    public abstract class CustomModel
    {
        public virtual string? Name { get; set; }
        public virtual string? EndPointKey { get; set; }
        public virtual string? CacheKey { get; set; }
    }
}
