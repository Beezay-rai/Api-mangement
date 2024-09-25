using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yarp.Data;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Controllers
{
    [Route("config")]
    [ApiController]
    public class PluginController : ControllerBase
    {
        private readonly IPlugin _plugin;
        public PluginController(IPlugin plugin)
        {
            _plugin = plugin;
        }

        [HttpPost("plugin")]
        public async Task<IActionResult> PostFile([FromForm] CreatePluginModel model)
        {
            var response = new ResponseModel();


            if (model.DLLFile == null || model.DLLFile.Length == 0)
            {
                response.Status = false;
                response.Message = "No file uploaded or the file is empty.";
                return Ok(response);
            }


            response= await _plugin.AddPlugin(model);
            return Ok(response);
           
          
        }

        [HttpPut("plugin")]
        public async Task<IActionResult> UpdatePlugin([FromForm] PluginModel model)
        {
            var response = new ResponseModel();
            if (model.DLLFile == null || model.DLLFile.Length == 0)
            {
                response.Status = false;
                response.Message = "No file uploaded or the file is empty.";
                response.Data = new
                {
                    ProvidedData = model
                };
                return Ok(response);
            }
            response = await _plugin.UpdatePlugin(model);
            return Ok(response);
        }
        [HttpGet("plugin")]
        public async Task<IActionResult>GetPlugin()
        {
            var response = await _plugin.GetPlugin();
            return Ok(response);
        }
        [HttpGet("plugin/{pluginId}")]
        public async Task<IActionResult>GetPlugin(string pluginId)
        {
            var response = await _plugin.GetPlugin(pluginId);
            return Ok(response);
        }

        [HttpPatch("plugin")]
        public async Task<IActionResult> PatchPlugin([FromForm] PluginModel model)
        {
            var response = new ResponseModel();
            response = await _plugin.PatchPlugin( model);
            return Ok(response);
        }

    }
}
