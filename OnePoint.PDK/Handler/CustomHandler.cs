using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using OnePoint.PDK.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Transforms;

namespace OnePoint.PDK.Handler
{
    public abstract class CustomHandler
    {
        protected CustomSchema _schema;
        protected CustomHandler(CustomSchema schema)
        {
            _schema = schema;
        }

        public string Version { get; set; }
        public int Priority { get; set; }

        public abstract Task<AuthenticateResult> AuthenticateAsync(HttpContext context);

        public abstract Task PreAsync(HttpContext context);

        public abstract Task RequestTransfromAsync(RequestTransformContext context);
    }
}
