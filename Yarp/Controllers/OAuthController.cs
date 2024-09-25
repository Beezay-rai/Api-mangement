using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yarp.EventHandler;
using Yarp.Models;
using Yarp.Security;

namespace Yarp.Controllers
{
    [AllowAnonymous]
    [Route("connect")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly OnePointSettings _settings;
        private readonly JwtHelper _jwtHelper;

        public OAuthController(OnePointSettings settings, JwtHelper jwtHelper)
        {
            _settings = settings;
            _jwtHelper = jwtHelper;
        }

        [HttpPost, Route("token")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Token([FromForm] AccessTokenCommand request)
        {
            var eventHandler = new TokenEventHandler(_jwtHelper, _settings.SecuritySettings.ClientCredentialsSettings);
            var response = await eventHandler.Handle(request);
            return StatusCode((int)response.HttpStatusCode, response.HttpContentBody);
        }
    }
}
