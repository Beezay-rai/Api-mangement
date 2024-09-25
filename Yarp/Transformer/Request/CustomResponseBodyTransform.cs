using System.Text;
using Yarp.ReverseProxy.Transforms;

namespace Yarp.Transformer.Request
{
    public class CustomResponseBodyTransform : ResponseTransform
    {
        public override async ValueTask ApplyAsync(ResponseTransformContext responseContext)
        {
            var user = responseContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user").Value ?? "";
            var stream = await responseContext.ProxyResponse.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            // TODO: size limits, timeouts
            var body = await reader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(body))
            {
                responseContext.SuppressResponseBody = true;

                body = body.Replace("dog", "cat");
                var bytes = Encoding.UTF8.GetBytes(body);
                // Change Content-Length to match the modified body, or remove it.
                responseContext.HttpContext.Response.ContentLength = bytes.Length;
                // Response headers are copied before transforms are invoked, update any needed headers on the HttpContext.Response.
                await responseContext.HttpContext.Response.Body.WriteAsync(bytes);
            }
        }
    }
}
