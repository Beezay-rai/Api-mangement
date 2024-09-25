using FluentMigrator.Runner;
using Microsoft.AspNetCore.Mvc;
using OnePoint.PDK.Migrations;
using OnePoint.PDK.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Controllers
{
    [Route("config")]
    [ApiController]
    public class SchemaController : ControllerBase
    {
        private readonly IPlugin _plugin;
        private readonly OnePointSettings _settings;
        private readonly IConfiguration _configuration;

        public SchemaController(IPlugin plugin, OnePointSettings settings, IConfiguration configuration)
        {
            _plugin = plugin;
            _settings = settings;
            _configuration = configuration;
        }

        [HttpPost("schema/{pluginId}")]
        public async Task<IActionResult> CreateSchema(string pluginId, object model)
        {
            var pluginResponse = await _plugin.GetPlugin(pluginId);
            if (!pluginResponse.Status)
            {
                return NotFound();
            }
            var pluginConfigresponse = await _plugin.CreateConfig(pluginId, model);
            return Ok(pluginConfigresponse);
        }
        [HttpGet("schema/{pluginId}")]
        public async Task<IActionResult> GetSchema(string pluginId)
        {
            var pluginResponse = await _plugin.GetPluginConfig(pluginId);
            return Ok(pluginResponse);

        }


    }
}
