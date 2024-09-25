using Azure;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using OnePoint.PDK.CustomAttribute;
using OnePoint.PDK.Data;
using OnePoint.PDK.Enpoint;
using OnePoint.PDK.Handler;
using OnePoint.PDK.Migrations;
using OnePoint.PDK.Repository;
using OnePoint.PDK.Schema;
using Serilog;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.RateLimiting;
using Yarp.Data;
using Yarp.Interfaces;
using Yarp.Library;
using Yarp.Models;
using Yarp.Repositories;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;
using Yarp.Security;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

var onePointSettings = new OnePointSettings();
builder.Configuration.GetSection(nameof(OnePointSettings)).Bind(onePointSettings);
builder.Services.AddSingleton(onePointSettings);

var jwtHelper = new JwtHelper(onePointSettings.SecuritySettings.JwtSettings);
builder.Services.AddSingleton(jwtHelper);

builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>("BearerAuthentication", null)
    .AddScheme<AuthenticationSchemeOptions, PluginAuthenticationHandler>("PluginAuthentication", null);


builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Bearer", policy =>
    {
        policy.AddAuthenticationSchemes("BearerAuthentication");
        policy.RequireAuthenticatedUser();
    });

    opt.AddPolicy("Custom", policy =>
    {
        policy.AddAuthenticationSchemes("PluginAuthentication");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddScoped<IPlugin, PluginRepository>();
builder.Services.AddScoped<IConsumer, ConsumerRepository>();
builder.Services.AddScoped<IConsumerGroup, ConsumerGroupRepository>();
builder.Services.AddScoped<IConsumerCredential, ConsumerCredentialRepository>();


builder.Configuration.AddJsonFile("yarpConfig.json", optional: false, reloadOnChange: true);
builder.Services.AddSingleton<YarpConfigProvider>();
builder.Services.AddSingleton<IProxyConfigProvider>(provider =>
    provider.GetRequiredService<YarpConfigProvider>());


builder.Services.AddReverseProxy().AddTransforms(context =>
{
    var serviceProvider = context.Services;
    var logger = serviceProvider.GetRequiredService<ILogger<PluginRequestBodyTransform>>();
    var pluginReuqestTransformer = new PluginRequestBodyTransform(context, onePointSettings, builder.Configuration, logger);
    context.AddRequestTransform(pluginReuqestTransformer.ApplyAsync);
});


//builder.Services.AddOutputCache(options =>
//{
//    options.AddPolicy("cachePolicy", builder => builder.Expire(TimeSpan.FromSeconds(5)));
//});

//builder.Services.AddRequestTimeouts(options =>
//{
//    options.DefaultPolicy = new RequestTimeoutPolicy()
//    {
//        TimeoutStatusCode = StatusCodes.Status408RequestTimeout
//    };
//    options.AddPolicy("timeOutPolicy", new RequestTimeoutPolicy()
//    {
//        Timeout = TimeSpan.FromSeconds(100),
//        TimeoutStatusCode = StatusCodes.Status408RequestTimeout
//    });
//});


//builder.Services.AddRateLimiter(options =>
//{
//    options.AddFixedWindowLimiter("ratePolicy", opt =>
//    {
//        opt.PermitLimit = 2;
//        opt.Window = TimeSpan.FromSeconds(60);
//        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//        opt.QueueLimit = 1;
//    });
//});

//builder.Services.AddCors(options =>
//{
//    options.DefaultPolicyName = "corsPolicy";
//    options.AddPolicy("corsPolicy", policy =>
//    {
//        policy.WithMethods(["POST"])
//              .WithOrigins("http://localhost:5128");
//    });

//});


var app = builder.Build();


app.UseStaticFiles();


app.UseRouting();

app.UseCors();


app.UseAuthentication();
app.UseAuthorization();


//app.UseRequestTimeouts();
//app.UseOutputCache();
//app.UseRateLimiter();


app.MapControllers();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<PluginRequestBodyTransform>>();

        var pluginMiddleWare = new PluginPreMiddleware(onePointSettings, builder.Configuration, logger);

        await pluginMiddleWare.PreAsync(context, next);

        await next();
    });
});//.RequireCors("corsPolicy");

app.UseMiddleware<PluginApiMiddleware>();

try
{
    Log.Information("Application Starting Up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application Failed To Start Correctly");
}
finally
{
    Log.CloseAndFlush();
}

public class StateMachine
{
    IConfiguration Configuration;
    OnePointSettings OnePointSettings;
    public StateMachine(IConfiguration configuration, OnePointSettings onePointSettings)
    {
        Configuration = configuration;
        OnePointSettings = onePointSettings;
    }

    public async Task<PluginConfig> GetState()
    {
        var dao = new DapperDao(Configuration);
        var pluginSettings = new PluginConfig();
        pluginSettings.PluginList = await dao.ExecuteQueryAsync<Plugins>("select * from dbo.Plugins", null, System.Data.CommandType.Text);
        var pluginRouteMapList = await dao.ExecuteQueryAsync<PluginsRouteMap>("select * from dbo.PluginRouteMap", null, System.Data.CommandType.Text);
        pluginSettings.PluginRouteMap = pluginRouteMapList.ToList();

        pluginSettings.Plugins = new Dictionary<string, Assembly>();
        var configJson = File.ReadAllText("yarpConfig.json");
        pluginSettings.Routes = JsonSerializer.Deserialize<YarpModel>(configJson).Routes;

        if (pluginSettings.PluginList != null && pluginSettings.PluginList.Count > 0)
        {
            foreach (var plugin in pluginSettings.PluginList)
            {
                try
                {
                    var asm = Assembly.LoadFile(OnePointSettings.PluginSettings.DllPath + plugin.DLLFile);
                    pluginSettings.Plugins.Add(plugin.Id, asm);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            pluginSettings.PluginEnabled = true;
        }
        return pluginSettings;
    }
}

public class PluginRequestBodyTransform : RequestTransform
{
    private readonly TransformBuilderContext context;
    private readonly OnePointSettings onePointSettings;
    private readonly IConfiguration configuration;
    private readonly ILogger<PluginRequestBodyTransform> logger;

    public PluginRequestBodyTransform(TransformBuilderContext context, OnePointSettings onePointSettings, IConfiguration configuration, ILogger<PluginRequestBodyTransform> logger)
    {
        this.context = context;
        this.onePointSettings = onePointSettings;
        this.configuration = configuration;
        this.logger = logger;
    }


    public override async ValueTask ApplyAsync(RequestTransformContext requestContext)
    {
        logger.LogInformation("From PluginRequestBodyTransform");

        var proxyFeature = requestContext.HttpContext.GetReverseProxyFeature();

        var state = new StateMachine(configuration, onePointSettings);

        var pluginSettings = await state.GetState();

        if (pluginSettings.PluginEnabled)
        {
            string customTransformPluginId = null;

            var customTransformPlugin = context.Route.Metadata?.TryGetValue(StaticValues.PluginId, out customTransformPluginId) ?? false;
            var customTransformEnabled = context.Route.Metadata?.TryGetValue(StaticValues.CustomRequestTransformationEnabled, out _) ?? false;

            if (customTransformPlugin && customTransformEnabled)
            {

                var plugin = pluginSettings.PluginList.FirstOrDefault(x => x.Id == customTransformPluginId);

                var assembly = pluginSettings.Plugins[plugin.Id];

                var requestTransformerType = assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomHandler)))
                     .FirstOrDefault();

                var requestSchema = assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomSchema)))
                     .FirstOrDefault();

                var requestSchemaInstance = Activator.CreateInstance(requestSchema) as CustomSchema;

                var schemaObj = JsonSerializer.Deserialize(plugin.Config, requestSchemaInstance.GetType());

                var requestTransformerInstance = Activator.CreateInstance(requestTransformerType, new object[] { schemaObj }) as CustomHandler;

                await requestTransformerInstance.RequestTransfromAsync(requestContext);
            }
        }
    }
}

public class PluginAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly OnePointSettings _settings;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PluginAuthenticationHandler> _log;
    public PluginAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, OnePointSettings settings, IConfiguration configuration, ILogger<PluginAuthenticationHandler> log) : base(options, logger, encoder, clock)
    {
        _settings = settings;
        _configuration = configuration;
        _log = log;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        _log.LogInformation("From PluginAuthenticationHandler");

        var state = new StateMachine(_configuration, _settings);

        var pluginSettings = await state.GetState();

        if (pluginSettings.PluginEnabled && pluginSettings.PluginRouteMap != null && pluginSettings.PluginRouteMap.Count > 0)
        {
            Assembly assembly = null;
            string pluginId = null;

            foreach (var map in pluginSettings.PluginRouteMap)
            {
                RouteConfig routeConfig;

                var route = pluginSettings.Routes.TryGetValue(map.RouteId, out routeConfig);
                if (route && routeConfig.Match.Path.Equals(Request.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase))
                {
                    var customPluginEnabled = routeConfig.Metadata.TryGetValue(StaticValues.CustomAuthenticationHandlerEnabled, out _);

                    if (customPluginEnabled)
                    {
                        var assemblyAvialable = pluginSettings.Plugins.TryGetValue(map.PluginId, out assembly);
                        if (assemblyAvialable)
                        {
                            pluginId = map.PluginId;
                            break;
                        }
                    }
                }
            }

            if (assembly != null)
            {
                var authenticationHandlerType = assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomHandler)))
                     .FirstOrDefault();

                var plugin = pluginSettings.PluginList.FirstOrDefault(x => x.Id == pluginId);

                var requestSchema = assembly.GetTypes()
                          .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomSchema)))
                          .FirstOrDefault();

                var requestSchemaInstance = Activator.CreateInstance(requestSchema) as CustomSchema;

                var schemaObj = JsonSerializer.Deserialize(plugin.Config, requestSchemaInstance.GetType());

                var authenticationHandlerInstance = Activator.CreateInstance(authenticationHandlerType, new object[] { schemaObj }) as CustomHandler;

                var res = await authenticationHandlerInstance.AuthenticateAsync(Request.HttpContext);

                if (res.Succeeded == true)
                {
                    return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(res.Principal, "PluginAuthentication")));
                }
                return res;
            }

        }

        return AuthenticateResult.Fail("Authorization Plugin Not Configured");
    }
}

public class PluginPreMiddleware
{
    private readonly OnePointSettings onePointSettings;
    private readonly IConfiguration configuration;
    private readonly Microsoft.Extensions.Logging.ILogger logger;

    public PluginPreMiddleware(OnePointSettings onePointSettings, IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
    {
        this.onePointSettings = onePointSettings;
        this.configuration = configuration;
        this.logger = logger;

    }

    public async Task PreAsync(HttpContext context, Func<Task> next)
    {
        logger.LogInformation("From Plugin Middleware");
        var proxyFeature = context.GetReverseProxyFeature();

        var state = new StateMachine(configuration, onePointSettings);
        var pluginSettings = await state.GetState();
        if (pluginSettings.PluginEnabled)
        {
            var cluster = proxyFeature.Cluster;
            var destinations = proxyFeature.AvailableDestinations;
            var route = proxyFeature.Route;

            string customMiddleWarePluginId = null;

            var customMiddleWarePlugin = route.Config.Metadata?.TryGetValue(StaticValues.PluginId, out customMiddleWarePluginId) ?? false;
            var customMiddleWareEnabled = route.Config.Metadata?.TryGetValue(StaticValues.CustomRequestMiddleWareEnabled, out _) ?? false;

            if (customMiddleWarePlugin && customMiddleWareEnabled)
            {
                var plugin = pluginSettings.PluginList.FirstOrDefault(x => x.Id == customMiddleWarePluginId);

                var assembly = pluginSettings.Plugins[plugin.Id];

                var requestTransformerType = assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomHandler)))
                     .FirstOrDefault();

                var requestSchema = assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomSchema)))
                     .FirstOrDefault();

                var requestSchemaInstance = Activator.CreateInstance(requestSchema) as CustomSchema;

                var schemaObj = JsonSerializer.Deserialize(plugin.Config, requestSchemaInstance.GetType());

                var requestTransformerInstance = Activator.CreateInstance(requestTransformerType, new object[] { schemaObj }) as CustomHandler;

                await requestTransformerInstance.PreAsync(context);
            }
        }
    }


}

public class PluginApiMiddleware
{
    private readonly RequestDelegate next;
    private readonly OnePointSettings onePointSettings;
    private readonly IConfiguration configuration;

    public PluginApiMiddleware(RequestDelegate next, OnePointSettings onePointSettings, IConfiguration configuration)
    {
        this.onePointSettings = onePointSettings;
        this.configuration = configuration;
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            await next(context);
            return;
        }
        await Proccess(context);

        if (!context.Response.HasStarted)
        {
            await next(context);
        }
    }

    private async Task Proccess(HttpContext context)
    {
        var path = context.Request.Path.Value.Split('/');
        if (path.Length == 3)
        {
            var state = new StateMachine(configuration, onePointSettings);
            var pluginSettings = await state.GetState();
            if (pluginSettings.PluginEnabled)
            {
                var pluginId = path[1];
                var pluginPath = "/" + path[2];
                Assembly assembly;
                var isAssemblyAvailable = pluginSettings.Plugins.TryGetValue(pluginId, out assembly);
                if (isAssemblyAvailable)
                {
                    var endpointTypes = assembly.GetTypes()
                          .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomEndpoint)))
                          .ToList();

                    foreach (var endpointType in endpointTypes)
                    {
                        var pathInfos = endpointType.GetCustomAttributes<PathAttribute>();
                        var pathInfo = pathInfos.SingleOrDefault(x => x.Method.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase) && x.Path.Equals(pluginPath, StringComparison.OrdinalIgnoreCase));
                        if (pathInfo != null)
                        {
                            var dao = new OnePointDao(configuration.GetConnectionString("DefaultConnection"));

                            var requestRepo = assembly.GetTypes()
                             .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomRepository)))
                             .FirstOrDefault();

                            var requestRepoInstance = Activator.CreateInstance(requestRepo, new object[] { dao }) as CustomRepository;

                            var endpoint = Activator.CreateInstance(endpointType, new object[] { requestRepoInstance }) as CustomEndpoint;
                            await endpoint.Execute(context);
                        }
                    }
                }
            }
        }
    }
}