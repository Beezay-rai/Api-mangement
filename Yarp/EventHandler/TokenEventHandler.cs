using System.Net;
using System.Security.Claims;
using Yarp.Models;
using Yarp.Security;

namespace Yarp.EventHandler
{
    public class TokenEventHandler
    {
        private readonly JwtHelper _tokenService;
        private readonly ClientCredentialsSettings _clientCredentials;
        public TokenEventHandler(JwtHelper tokenService, ClientCredentialsSettings settings)
        {
            _clientCredentials = settings;
            _tokenService = tokenService;
        }

        public async Task<CommonResponse<ClientCredentialsTokenResponse>> Handle(AccessTokenCommand request)
        {
            string clientId = _clientCredentials.ClientId;
            string clientSecret = _clientCredentials.ClientSecret;

            if (string.IsNullOrEmpty(request.grant_type))
            {
                return await Task.FromResult(new CommonResponse<ClientCredentialsTokenResponse>
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    HttpContentBody = new ClientCredentialsFailureResponse
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
                        HttpContentBody = new ClientCredentialsFailureResponse
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
                        HttpContentBody = new ClientCredentialsFailureResponse
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
                    HttpContentBody = new ClientCredentialsSuccessResponse
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
            //            HttpContentBody = new ClientCredentialsFailureResponse
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
            //            HttpContentBody = new ClientCredentialsFailureResponse
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
            //        HttpContentBody = new ClientCredentialsSuccessResponse
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
                    HttpContentBody = new ClientCredentialsFailureResponse
                    {
                        error = "invalid_grant",
                        error_description = "The request grant is invalid."
                    }
                });
            }
        }
    }
}
