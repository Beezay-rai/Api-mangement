using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using OnePoint.APIMSV1.Connector.Helper;
using OnePoint.APIMSV1.Connector.Schema;
using OnePoint.PDK.Handler;
using OnePoint.PDK.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Transforms;

namespace OnePoint.APIMSV1.Connector.Handler
{
    public class APIMSV1EnquiryHandler : CustomHandler
    {
        private APIMSV1Schema configModel;

        public APIMSV1EnquiryHandler(CustomSchema schema) : base(schema)
        {
            configModel = (APIMSV1Schema)schema;
            Version = "1.0.0";
            Priority = 10;
        }

        public override async Task<AuthenticateResult> AuthenticateAsync(HttpContext context)
        {
            var authHelper = new HmacHelper(configModel);

            var authResult = await authHelper.HandleAuthenticateAsync(context);

            return authResult;
        }

        public override Task PreAsync(HttpContext context)
        {
            var proxyFeature = context.GetReverseProxyFeature();
            Console.WriteLine($"incomming request path {context.Request.Path.ToString()}");
            return Task.CompletedTask;
        }

        public override async Task RequestTransfromAsync(RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Remove("Authorization");
            context.ProxyRequest.Headers.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{configModel.Username}:{configModel.Password}")));

            var claims = context.HttpContext?.User?.Claims ?? null;

            //var proxyFeature = context.HttpContext.GetReverseProxyFeature();

            using var reader = new StreamReader(context.HttpContext.Request.Body);

            var body = await reader.ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                string timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");

                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(body));

                var model = await JsonSerializer.DeserializeAsync<object>(ms);

                var signatureModel = new { Model = model, TimeStamp = timeStamp };
                var reqString = body;
                byte[] reqBytes = Encoding.UTF8.GetBytes(reqString);
                byte[] signaturebytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(signatureModel));
                var rsa = new RsaHelper(configModel.PrivateKey);
                var signature = rsa.Sign(signaturebytes);

                var req = new
                {
                    FunctionName = "AccountValidation",
                    Data = Convert.ToBase64String(reqBytes),
                    Signature = signature,
                    TimeStamp = timeStamp
                };

                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(req));
                context.HttpContext.Request.Body = new MemoryStream(bytes);
                context.ProxyRequest.Content.Headers.ContentLength = bytes.Length;
            }
        }


    }
}
