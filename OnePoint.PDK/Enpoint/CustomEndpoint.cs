using Microsoft.AspNetCore.Http;
using OnePoint.PDK.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePoint.PDK.Enpoint
{
    public abstract class CustomEndpoint
    {
        protected CustomEndpoint(CustomRepository repository)
        {

        }
        public abstract Task Execute(HttpContext context);
    }
}
