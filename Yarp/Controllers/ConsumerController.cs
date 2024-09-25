using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Controllers
{
    [Route("setup")]
    [ApiController]
    public class ConsumerController : ControllerBase
    {
        private readonly IConsumer _repo;
        public ConsumerController(IConsumer consumer)
        {
            _repo = consumer;
        }

        [HttpPost("consumer")]
        public async Task<IActionResult> CreateConsumer(CreateConsumerModel model)
        {
            var response = await _repo.AddConsumer(model);
            return Ok(response);
        }

        [HttpPut("consumer/{consumerId}")]
        public async Task<IActionResult> UpdateConsumer(string consumerId, [FromBody] CreateConsumerModel model)
        {
            var response = await _repo.UpdateConsumer(consumerId, model);
            return Ok(response);
        }

        [HttpPatch("consumer/{consumerId}")]
        public async Task<IActionResult> PatchConsumer(string consumerId, [FromBody] PatchConsumerModel model)
        {
            var response = await _repo.PatchConsumer(consumerId, model);
            return Ok(response);
        }
        [HttpGet("consumer")]
        public async Task<IActionResult> GetConsumer()
        {
            var response = await _repo.GetConsumer();
            return Ok(response);
        }

        [HttpGet("consumer/{consumerId}")]
        public async Task<IActionResult> GetConsumer(string consumerId)
        {
            var response = await _repo.GetConsumer(consumerId);
            return Ok(response);
        }
        [HttpDelete("consumer/{consumerId}")]
        public async Task<IActionResult> DeleteConsumer(string consumerId)
        {
            var response = await _repo.RemoveConsumer(consumerId);
            return Ok(response);
        }


    }
}
