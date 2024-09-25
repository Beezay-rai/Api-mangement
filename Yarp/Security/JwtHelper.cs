using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Yarp.Models;

namespace Yarp.Security
{
    public class JwtHelper
    {
        private readonly JwtSettings _settings;
        public JwtHelper(JwtSettings settings)
        {
            _settings = settings;
        }

        public TokenCommonDetails GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var response = new TokenCommonDetails
            {
                CreatedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddMinutes(_settings.AccessTokenValidityInMinutes)
            };
            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
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
                ExpiryDate = DateTime.Now.AddMinutes(_settings.RefreshTokenValidityInDays)
            };
            response.Token = GeneratToken();
            return response;

            string GeneratToken()
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim("user", loginId),
                    new Claim("session_id", Id),
                };
                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
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
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
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
                    ValidIssuer = _settings.Issuer,
                    ValidAudience = _settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
}
