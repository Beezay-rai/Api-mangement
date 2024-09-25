using Microsoft.AspNetCore.Mvc;
using Yarp.Library;
using Yarp.Models;

namespace Yarp.Controllers
{
    [Route("config")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly YarpOperator _yarp;

        public RouteController(YarpConfigProvider proxyConfigProvider, IConfiguration config)
        {
            _yarp = new YarpOperator(proxyConfigProvider, config);
        }
        #region Yarp Route API

        // Add a new route
        [HttpPost("routes")]
        public IActionResult AddRoute([FromBody] RouteModel route)
        {
            var response = _yarp.AddRoute(route);
            return Ok(response);
        }

        // Get route by ID
        [HttpGet("routes/{routeId}")]
        public IActionResult GetRouteById(string routeId)
        {
            var response = _yarp.GetRouteById(routeId);
            return Ok(response);
        }

        // Get all routes
        [HttpGet("routes")]
        public IActionResult GetRoutesList()
        {
            var data = _yarp.GetAllRoutes();
            return Ok(data);
        }

        [HttpPut("routes/{routeId}")]
        public IActionResult UpdateRoute(string routeId, [FromBody] UpdateRouteMdoel route)
        {

            var response = _yarp.UpdateRoute(routeId,route);
            return Ok(response);



        }

        // Update a route
        [HttpPatch("routes/{routeId}")]
        public IActionResult PatchRoute(string routeId, [FromBody] PatchRouteModel route)
        {
            var response = _yarp.PatchRoute(routeId, route);
            return Ok(response);
        }



        // Delete a route by ID
        [HttpDelete("routes/{routeId}")]
        public IActionResult DeleteRoute(string routeId)
        {
        
            var response =_yarp.DeleteRoute(routeId);
            return Ok(response);
        }

        #endregion
    }
}
