using System.Net;

namespace Yarp.Models
{
    public class CommonResponse<T>
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public T HttpContentBody { get; set; } = default(T);
    }

}
