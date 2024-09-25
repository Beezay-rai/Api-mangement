using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Yarp.Library;
using Yarp.Models;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.Controllers
{
    [Route("config")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly YarpOperator _yarp;
        private readonly IWebHostEnvironment _appEnvironment;

        public AdminController(YarpConfigProvider proxyConfigProvider, IWebHostEnvironment appEnvironment, IConfiguration config)
        {
            _yarp = new YarpOperator(proxyConfigProvider, config);
            _appEnvironment = appEnvironment;
        }

        [HttpGet]
        public IActionResult GetYarpConfig()
        {
            var response = new ResponseModel();
            var config = _yarp.GetYarpConfig();
            var YarpModel = new YarpModel()
            {
                Routes = config.Routes.ToDictionary(x => x.RouteId, x => x),
                Clusters = config.Clusters.ToDictionary(x => x.ClusterId, x => x),

            };
            response.Status = true;
            response.Message = "Config File !";
            response.Data = YarpModel;
            return Ok(response);
        }


        [HttpPost("uploadFile")]
        public async Task<IActionResult> PostFile( IFormFile file)
        {
            var response = new ResponseModel();


            var fileExtension = file.FileName.Substring(file.FileName.IndexOf('.')).Replace(".", "");
            if (fileExtension.ToLower() != "json")
            {
                response.Status = false;
                response.Message = "Only Accept Json File !";
                return Ok(response);
            }
            var path = Path.Combine(_appEnvironment.WebRootPath, "ApiDoc", "openapi.json");
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.Create(path);
            }
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            response.Status = true;
            response.Message = "File uploaded successfully.";

            return Ok(response);
        }

        [HttpPost("adminApi")]
        public async Task<IActionResult> PostFileForAdmin( IFormFile file)
        {
            var response = new ResponseModel();
            var fileExtension = file.FileName.Substring(file.FileName.IndexOf('.')).Replace(".", "");
            if (fileExtension.ToLower() != "json")
            {
                response.Status = false;
                response.Message = "Only Accept Json File !";
                return Ok(response);
            }
            var path = Path.Combine(_appEnvironment.WebRootPath, "admin", "openapi.json");
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.Create(path);
            }
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            response.Status = true;
            response.Message = "File uploaded successfully.";

            return Ok(response);
        }

    }
}
