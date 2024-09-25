using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Controllers
{
    [Route("credential")]
    [ApiController]
    public class CredentialController : ControllerBase
    {
        private readonly IConsumerCredential _repo;

        public CredentialController(IConsumerCredential repo)
        {
            _repo = repo;
        }

        [HttpPost("basic")]
        public async Task<IActionResult> BasicCred(BasicCredModel model)
        {
            var response = await _repo.AddBasicCred(model);
            return Ok(response);
        }

        [HttpGet("basic")]
        public async Task<IActionResult> GetBasicCred()
        {
            var response = await _repo.GetBasicCred();
            return Ok(response);
        }
        [HttpPut("basic/{basicCredId}")]
        public async Task<IActionResult> UpdateBasicCred(string basicCredId, [FromBody] BasicCredModel model)
        {
            var response = await _repo.UpdateBasicCred(basicCredId,model);
            return Ok(response);
        }
    }
}
