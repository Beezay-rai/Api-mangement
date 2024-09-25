using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Controllers
{
    [Route("setup")]
    [ApiController]
    public class ConsumerGroupController : ControllerBase
    {
        private readonly IConsumerGroup _repo;
        public ConsumerGroupController(IConsumerGroup consumerGroup)
        {
            _repo = consumerGroup;
        }

        [HttpPost("consumerGroup")]
        public async Task<IActionResult> CreateConsumerGroup(CreateConsumerGroupModel model)
        {
            var response = await _repo.AddConsumerGroup(model);
            return Ok(response);
        }

        [HttpPut("consumerGroup/{consumerGroupId}")]
        public async Task<IActionResult> UpdateConsumerGroup(string consumerGroupId,[FromBody]CreateConsumerGroupModel model)
        {
            var response = await _repo.UpdateConsumerGroup(consumerGroupId, model);
            return Ok(response);
        }

        [HttpPatch("consumerGroup/{consumerGroupId}")]
        public async Task<IActionResult> PatchConsumerGroup(string consumerGroupId, [FromBody] CreateConsumerGroupModel model)
        {
            var response = await _repo.PatchConsumerGroup(consumerGroupId, model);
            return Ok(response);
        }
        [HttpGet("consumerGroup")]
        public async Task<IActionResult> GetConsumerGroup()
        {
            var response = await _repo.GetConsumerGroup();
            return Ok(response);
        }
  
        [HttpGet("consumerGroup/{consumerGroupId}")]
        public async Task<IActionResult> GetConsumerGroup(string consumerGroupId)
        {
            var response = await _repo.GetConsumerGroup(consumerGroupId);
            return Ok(response);
        }
        [HttpDelete("consumerGroup/{consumerGroupId}")]
        public async Task<IActionResult> DeleteConsumerGroup(string consumerGroupId)
        {
            var response = await _repo.RemoveConsumerGroup(consumerGroupId);
            return Ok(response);
        }
  

    }
}
