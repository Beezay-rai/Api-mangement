using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using OnePoint.APIMSV1.Connector.Schema;
using OnePoint.PDK.Schema;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OnePoint.APIMSV1.Connector.Helper
{
    public class HmacHelper
    {
        private APIMSV1Schema configModel;
        public HmacHelper(CustomSchema schema)
        {
            configModel = (APIMSV1Schema)schema;
        }
        public async Task<AuthenticateResult> HandleAuthenticateAsync(HttpContext context)
        {
            var headers = context.Request.Headers;

            if (headers.ContainsKey("AUTHORIZATION"))
            {
                var authValue = headers["AUTHORIZATION"].ToString();
                var splitAuthValue = authValue.Split(":");
                string nonceValue = "";
                string providedApiKey = "";
                if (splitAuthValue != null && splitAuthValue.Length == 3)
                {
                    var splitAlgoKey = splitAuthValue[0].Split(" ");
                    if (splitAlgoKey.Length == 2 && splitAlgoKey != null)
                    {
                        var alorithmName = splitAlgoKey[0];
                        providedApiKey = splitAlgoKey[1];
                    }
                    nonceValue = splitAuthValue[1].ToString();
                    var providedSignature = splitAuthValue[2].ToString();
                    if (Guid.TryParse(nonceValue, out Guid nonce))
                    {
                        context.Request.EnableBuffering(); 
                        context.Request.Body.Position = 0;

                        var reader = new StreamReader(context.Request.Body);
                        var requestBody = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;
                        try
                        {
                            var isValid = ValidateSignature(providedSignature, providedApiKey, requestBody, nonce);
                            if (!isValid)
                            {
                                return await Task.FromResult(AuthenticateResult.Fail("Invalid authorization data"));
                            }

                            var claims = new[] {
                                    new Claim("key",  configModel.ApiKey)
                                };
                            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "BasicAuth"));

                            return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, "HmacAuth")));
                        }
                        catch
                        {
                            return await Task.FromResult(AuthenticateResult.Fail("Invalid authorization data"));
                        }

                    }
                    else
                    {
                        return await Task.FromResult(AuthenticateResult.Fail("Invalid authorization data"));
                    }
                }
            }

            return await Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }

        public bool ValidateSignature(string signature, string ApiKey, string requestBody, Guid nonce)
        {
            var generatedSignature = GenerateSignature(ApiKey, requestBody, nonce);
            return string.Equals(signature, generatedSignature, StringComparison.OrdinalIgnoreCase);
        }

        public string GenerateSignature(string apiKey, string requestBody, Guid nonce)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(" ");
            stringBuilder.Append(apiKey);
            stringBuilder.Append(" ");
            stringBuilder.Append(nonce);
            stringBuilder.Append(" ");
            stringBuilder.Append(requestBody);
            stringBuilder.Append(" ");

            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(configModel.ApiSecret)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
