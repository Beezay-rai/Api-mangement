using Microsoft.AspNetCore.Http;
using OnePoint.APIMSV1.Connector.Models;
using OnePoint.APIMSV1.Connector.Repository;
using OnePoint.PDK.CustomAttribute;
using OnePoint.PDK.Enpoint;
using OnePoint.PDK.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePoint.APIMSV1.Connector.Endpoint
{
    [Path("get", "/hmac")]
    public class HmacGet : CustomEndpoint
    {
        private readonly HmacRepository _repository;
        public HmacGet(CustomRepository repository) : base(repository)
        {
            _repository = (HmacRepository)repository;
        }

        public override async Task Execute(HttpContext context)
        {
            var list = await _repository.Get();
            context.Response.StatusCode = 200;
            await context.Response.WriteAsJsonAsync(list);
        }
    }

    [Path("post", "/hmac")]
    public class HmacPost : CustomEndpoint
    {
        private readonly HmacRepository _repository;
        public HmacPost(CustomRepository repository) : base(repository)
        {
            _repository = (HmacRepository)repository;
        }

        public override async Task Execute(HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body);
            var stringData = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(stringData))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { status = false, error = "body is required" });
            }

            HmacModel model = null;

            try
            {
                model = JsonSerializer.Deserialize<HmacModel>(stringData, options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { status = false, error = "invalid json" });
            }

            var result = await _repository.Create(model);

            if (result > 0)
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsJsonAsync(new { status = true, id = result });
            }

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { status = false, error = "some went wrong" });
        }
    }
}
