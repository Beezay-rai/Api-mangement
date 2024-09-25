using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Yarp.Models;

namespace Yarp.Security
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly BasicAuthSettings _settings;
        public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, OnePointSettings settings)
            : base(options, logger, encoder)
        {
            _settings = settings.SecuritySettings.BasicAuthSettings;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            // If authorization header doesn't start with basic, throw no result.
            if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Invalid Header"));
            }
            var authBase64Decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Replace("Basic ", "", StringComparison.OrdinalIgnoreCase)));
            var array = authBase64Decoded.Split(":");
            if (array.Length != 2 || array[0] != _settings.Username || array[1] != _settings.Password)
            {
                return await Task.FromResult(AuthenticateResult.Fail("Invalid Credentials"));

            }
            var claims = new[] {
                new Claim("user", _settings.Username)
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "BasicAuth"));
            return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, "BasicAuth")));
        }
    }
}
