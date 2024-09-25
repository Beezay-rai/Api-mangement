//using Microsoft.Extensions.Options;
//using Yarp.ReverseProxy.Transforms;
//using Yarp.ReverseProxy.Transforms.Builder;
//using Yarp.Transformer.Request;

//namespace Yarp.Transformer.Provider
//{
//    public class CustomTransformProvider : ITransformProvider
//    {
//        public void ValidateRoute(TransformRouteValidationContext context)
//        {
//            // Check all routes for a custom property and validate the associated transform data.
//            if (context.Route.Metadata?.TryGetValue("Replace", out var value) ?? false)
//            {
//                if (string.IsNullOrEmpty(value))
//                {
//                    context.Errors.Add(new ArgumentException("A non-empty Replace value is required"));
//                }
//            }
//        }

//        public void ValidateCluster(TransformClusterValidationContext context)
//        {
//            // Check all clusters for a custom property and validate the associated transform data.
//            if (context.Cluster.Metadata?.TryGetValue("Replace", out var value) ?? false)
//            {
//                if (string.IsNullOrEmpty(value))
//                {
//                    context.Errors.Add(new ArgumentException("A non-empty Replace value is required"));
//                }
//            }
//        }

//        public void Apply(TransformBuilderContext transformBuildContext)
//        {

//            // Check all routes for a custom property and add the associated transform.
//            if ((transformBuildContext.Route.Metadata?.TryGetValue("Replace", out var value) ?? false)
//                || (transformBuildContext.Cluster?.Metadata?.TryGetValue("Replace", out value) ?? false))
//            {
//                if (string.IsNullOrEmpty(value))
//                {
//                    throw new ArgumentException("A non-empty Replace value is required");
//                }
//                var requestTransformer = new PluginRequestBodyTransform(null, null);
//                var responseTransformer = new CustomResponseBodyTransform();

//                transformBuildContext.AddRequestTransform(transformContext =>
//                {
//                    transformContext.ProxyRequest.Options.Set(new HttpRequestOptionsKey<string>("Replace"), value);
//                    return default;
//                }).AddRequestTransform(requestTransformer.ApplyAsync)
//                  .AddResponseTransform(responseTransformer.ApplyAsync);
//            }
//        }
//    }
//}
