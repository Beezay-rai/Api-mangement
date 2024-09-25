using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Identity.Controllers
{
    [ApiController]
    [Route("connect")]
    public class OAuthController : ControllerBase
    {
        [HttpPost, Route("token")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Token([FromForm] AccessTokenCommand request)
        {
            var response = await Handle(request);
            return StatusCode((int)response.HttpStatusCode, response.Data);
        }

        private async Task<CommonResponse<ClientCredentialsTokenResponse>> Handle(AccessTokenCommand request)
        {
            var _tokenService = new JwtService();
            string clientId = "admin";
            string clientSecret = "pass";

            if (string.IsNullOrEmpty(request.grant_type))
            {
                return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Data = new ClientCredentialsFailureResponse
                    {
                        error = "invalid_request",
                        error_description = "The request is missing a parameter so the server can’t proceed with the request."
                    }
                });
            }

            if (request.grant_type == "client_credentials")
            {
                if (string.IsNullOrEmpty(request.client_id) || string.IsNullOrEmpty(request.client_secret))
                {
                    return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
                    {
                        HttpStatusCode = HttpStatusCode.BadRequest,
                        Data = new ClientCredentialsFailureResponse
                        {
                            error = "invalid_request",
                            error_description = "The request is missing a parameter so the server can’t proceed with the request."
                        }
                    });
                }

                if (clientId != request.client_id || clientSecret != request.client_secret)
                {
                    return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
                    {
                        HttpStatusCode = HttpStatusCode.Unauthorized,
                        Data = new ClientCredentialsFailureResponse
                        {
                            error = "invalid_client",
                            error_description = "The request contains an invalid client ID or secret."
                        }
                    });
                }

                var claims = new[]
                {
                   new Claim("user", request.client_id),
                   new Claim("scope", "read"),
                   new Claim("scope", "write"),
                   new Claim("scope", "trust")
                };

                var accessTokenResponse = _tokenService.GenerateAccessToken(claims);
                var refreshTokenResponse = _tokenService.GenerateRefreshToken(request.client_id, Guid.NewGuid().ToString());
                var expiry = (int)(accessTokenResponse.ExpiryDate - accessTokenResponse.CreatedDate).TotalSeconds;

                return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    Data = new ClientCredentialsSuccessResponse
                    {
                        access_token = accessTokenResponse.Token,
                        refresh_token = refreshTokenResponse.Token,
                        token_type = "Bearer",
                        expires_in = expiry,
                        scope = "read write trust"
                    }
                });
            }

            //else if (request.grant_type == "refresh_token")
            //{
            //    if (string.IsNullOrEmpty(request.refresh_token))
            //    {
            //        return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
            //        {
            //            HttpStatusCode = HttpStatusCode.BadRequest,
            //            Data = new ClientCredentialsFailureResponse
            //            {
            //                error = "invalid_request",
            //                error_description = "The request is missing a parameter so the server can’t proceed with the request."
            //            }
            //        });
            //    }

            //    var isValid = _tokenService.Validate(request.refresh_token);

            //    if (!isValid)
            //    {
            //        return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
            //        {
            //            HttpStatusCode = HttpStatusCode.BadRequest,
            //            Data = new ClientCredentialsFailureResponse
            //            {
            //                error = "invalid_request",
            //                error_description = "The request has invalid refresh token."
            //            }
            //        });
            //    }

            //    var principal = _tokenService.GetPrincipalFromToken(request.refresh_token);
            //    var client = principal.Claims.Single(x => x.Type == "user").Value;
            //    var claims = new[]
            //    {
            //       new Claim("user", client),
            //       new Claim("scope", "read"),
            //       new Claim("scope", "write"),
            //       new Claim("scope", "trust")
            //    };

            //    var accessTokenResponse = _tokenService.GenerateAccessToken(claims);
            //    var refreshTokenResponse = _tokenService.GenerateRefreshToken(client, Guid.NewGuid().ToString());
            //    var expiry = (int)(accessTokenResponse.ExpiryDate - accessTokenResponse.CreatedDate).TotalSeconds;

            //    return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
            //    {
            //        HttpStatusCode = HttpStatusCode.OK,
            //        Data = new ClientCredentialsSuccessResponse
            //        {
            //            access_token = accessTokenResponse.Token,
            //            refresh_token = refreshTokenResponse.Token,
            //            token_type = "Bearer",
            //            expires_in = expiry,
            //            scope = "read write trust"
            //        }
            //    });
            //}

            else
            {
                return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Data = new ClientCredentialsFailureResponse
                    {
                        error = "invalid_grant",
                        error_description = "The request grant is invalid."
                    }
                });
            }
        }
    }

    public class CommonResponse<T>
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public T Data { get; set; } = default(T);
    }

    public class ClientCredentialsTokenResponse
    {

    }

    public class ClientCredentialsSuccessResponse : ClientCredentialsTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    public class ClientCredentialsFailureResponse : ClientCredentialsTokenResponse
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string error_uri { get; set; }
    }

    public class AccessTokenCommand
    {
        public string grant_type { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
       // public string refresh_token { get; set; } 
    }

    public class TokenCommonDetails
    {
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    

    public class JwtService 
    {
        public const string Key = "not_too_short_secret_otherwise_it_might_error";
        public const string Issuer = "https://onepointfinserv.com/";
        public const string Audience = "https://onepointfinserv.com/";
        public const int AccessTokenValidityInMinutes = 1;
        public const int RefreshTokenValidityInDays = 1;
 
        public TokenCommonDetails GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var response = new TokenCommonDetails
            {
                CreatedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddMinutes(AccessTokenValidityInMinutes)
            };
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                notBefore: response.CreatedDate,
                expires: response.ExpiryDate,
                signingCredentials: signingCredentials
            );
            response.Token = new JwtSecurityTokenHandler().WriteToken(token);
            return response;
        }

        public TokenCommonDetails GenerateRefreshToken(string loginId, string Id)
        {
            var response = new TokenCommonDetails
            {
                CreatedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddMinutes(RefreshTokenValidityInDays)
            };
            response.Token = GeneratToken();
            return response;

            string GeneratToken()
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim("user", loginId),
                    new Claim("session_id", Id),
                };
                var token = new JwtSecurityToken(
                    issuer: Issuer,
                    audience: Audience,
                    claims: claims,
                    notBefore: response.CreatedDate,
                    expires: response.ExpiryDate,
                    signingCredentials: signingCredentials
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                ClockSkew = TimeSpan.Zero
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        public bool Validate(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
