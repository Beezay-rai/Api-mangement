using Microsoft.AspNetCore.Mvc;
using Yarp.Library;
using Yarp.Models;

namespace Yarp.Controllers
{
    [Route("config")]
    [ApiController]
    public class ClusterController : ControllerBase
    {
        private readonly YarpOperator _yarp;
        public ClusterController(YarpConfigProvider proxyConfigProvider, IConfiguration config)
        {
            _yarp = new YarpOperator(proxyConfigProvider, config);
        }
        #region Cluster API

        [HttpPost("clusters")]
        public IActionResult CreateCluster([FromBody] CreateClusterConfigModel model)
        {
            var response = _yarp.CreateCluster(model);
            return Ok(response);
        }


            
        [HttpGet("clusters")]
        public IActionResult GetClustersList()
        {
            var response = _yarp.GetAllCluster();
            return Ok(response);
        }

        [HttpGet("clusters/{clusterId}")]
        public IActionResult GetClusterById(string clusterId)
        {
            var response = _yarp.GetClusterById(clusterId);
            return Ok(response);
        }





        [HttpPut("clusters/{clusterId}")]
        public IActionResult UpdateCluster([FromQuery] string clusterId, [FromBody] PutClusterConfigModel model)
        {
            var response = _yarp.UpdateCluster(clusterId, model);
            return Ok(response);
        }


        [HttpPatch("clusters/{clusterId}")]
        public IActionResult PatchCluster(string clusterId, PutClusterConfigModel model)
        {
           var response = _yarp.PatchCluster(clusterId,model);
            return Ok(response);
        }

        #endregion
    }
}
