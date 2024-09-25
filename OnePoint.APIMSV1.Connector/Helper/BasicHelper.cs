using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using OnePoint.APIMSV1.Connector.Schema;
using OnePoint.PDK.Schema;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OnePoint.APIMSV1.Connector.Helper
{
    public class BasicHelper
    {
        private APIMSV1Schema configModel;
        public BasicHelper(CustomSchema schema)
        {
            configModel = (APIMSV1Schema)schema;
        }

        public async Task<AuthenticateResult> HandleAuthenticateAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }
            var authorizationHeader = context.Request.Headers["Authorization"].ToString();

            // If authorization header doesn't start with basic, throw no result.
            if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Invalid Header"));
            }
            var authBase64Decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Replace("Basic ", "", StringComparison.OrdinalIgnoreCase)));
            var array = authBase64Decoded.Split(":");
            if (array.Length != 2 || array[0] != configModel.Username || array[1] != configModel.Password)
            {
                return await Task.FromResult(AuthenticateResult.Fail("Invalid Credentials"));

            }
            var claims = new[] {
                new Claim("user",  configModel.Username)
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "BasicAuth"));
            return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, "BasicAuth")));
        }
    }
}
