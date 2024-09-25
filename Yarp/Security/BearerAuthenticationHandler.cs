using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Yarp.Models;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Yarp.Security
{
    public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtHelper _tokenService;
        public BearerAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            JwtHelper tokenService)
            : base(options, logger, encoder, clock)
        {
            _tokenService = tokenService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Response.Headers["WWW-Authenticate"] = $"Bearer realm=OnePoint, charset=\"UTF-8\"";
                return await Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            try
            {
                var claimsKey = Request.Headers.Authorization.ToList().SingleOrDefault();
                if (string.IsNullOrWhiteSpace(claimsKey))
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
                }
                var token = claimsKey.Split(' ').LastOrDefault();

                if (token == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
                }

                var isValid = _tokenService.Validate(token);

                if (!isValid)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid Token"));
                }

                var base64String = token.Split('.')[1];
                int mod4 = base64String.Length % 4;
                if (mod4 != 0)
                {
                    int padding = 4 - mod4;
                    base64String += new string('=', padding);
                }

                var payloadBytes = Convert.FromBase64String(base64String);
                var jsonPayload = Encoding.UTF8.GetString(payloadBytes);

                var payloadObject = JsonSerializer.Deserialize<AccessTokenPayload>(jsonPayload);

                var claims = new List<Claim> {
                   new Claim(nameof(payloadObject.user), payloadObject.user),
                };

                if (payloadObject.scope != null && payloadObject.scope.Count > 0)
                    foreach (var payload in payloadObject.scope)
                    {
                        claims.Add(new Claim(nameof(payload), payload));
                    }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch
            {
                return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
        }
    }
}
