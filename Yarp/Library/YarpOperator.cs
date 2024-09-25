using Dapper;
using System.Text.Json;
using Yarp.Data;
using Yarp.Models;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.Library
{
    public class YarpOperator
    {
        private readonly YarpConfigProvider _proxyConfigProvider;
        private readonly DapperDao _dao;
        public YarpOperator(YarpConfigProvider proxyConfigProvider, IConfiguration config)
        {
            _dao = new DapperDao(config);
            _proxyConfigProvider = proxyConfigProvider;
        }

        #region YarpRoute
        public ResponseModel AddRoute(RouteModel route)
        {
            var response = new ResponseModel();
            try
            {
                var check = _proxyConfigProvider.CheckCluster(route.ClusterId);
                if (!check)
                {
                    response.Message = "Cluster not found with Id : " + route.ClusterId;
                    response.Status = false;
                    response.Data = new
                    {
                        ProvidedData = route
                    };
                    return response;
                }
                var myNewRoute = new RouteConfig()
                {
                    RouteId = Guid.NewGuid().ToString(),
                    Match = new RouteMatch()
                    {
                        Methods = route.Match.Method,
                        Path = route.Match.Path,
                    },
                    AuthorizationPolicy = route.AuthPolicy,
                    CorsPolicy = route.CorsPolicy,
                    OutputCachePolicy = route.OutputCachePolicy,
                    RateLimiterPolicy = route.RateLimiterPolicy,
                    TimeoutPolicy = route.TimeOutPolicy,
                    ClusterId = route.ClusterId,
                    Metadata = route.Metadata,
                    Transforms = route.Transforms,
                };
                _proxyConfigProvider.AddRoute(myNewRoute);

                if (myNewRoute.Metadata != null && myNewRoute.Metadata.Count > 0)
                {
                    var checkPlugin = myNewRoute.Metadata.TryGetValue("PluginId", out var pluginId);
                    if (checkPlugin)
                    {
                        var myParams = new DynamicParameters();
                        myParams.Add("@flag", "i");
                        myParams.Add("@pluginId", pluginId);
                        myParams.Add("@routeId", myNewRoute.RouteId);
                        var RouteMap = _dao.ExecuteCommandAsync("proc_pluginRouteMap ", myParams, System.Data.CommandType.StoredProcedure).GetAwaiter().GetResult();
                        if (RouteMap)
                        {
                            response.Message = "Saved Route With Plugin !";
                        }
                        else
                        {
                            response.Message = "Saved Route but failed to save Plugin Route Map !";
                        }
                    }
                }


                response.Status = true;
                response.Message = string.IsNullOrEmpty(response.Message) ? "Added Route Successfully" :response.Message;
                response.Data = myNewRoute;

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured !";
                response.Data = new
                {
                    ErrrorDescription = ex.Message,
                    ProvidedData = route
                };
            }

            return response;
        }


        public ResponseModel GetRouteById(string routeId)
        {
            var response = new ResponseModel();
            try
            {
                var route = _proxyConfigProvider.GetAllRoutes().Where(x => x.RouteId == routeId).FirstOrDefault();
                if (route != null)
                {
                    response.Status = true;
                    response.Message = "Route With Id : " + routeId;
                    response.Data = route;
                }
                else
                {
                    response.Status = false;
                    response.Message = "Route Not Found With Id : " + routeId;
                    response.Data = new
                    {
                        ProvidedRouteId = routeId
                    };
                }
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured !";
                response.Data = new
                {
                    ErrrorDescription = ex.Message,
                    ProvidedRouteId = routeId
                };
            }
            return response;

         
        }

        public ResponseModel DeleteRoute(string routeId)
        {
            var response = new ResponseModel();
            try
            {
                var route = _proxyConfigProvider.GetAllRoutes().Where(x => x.RouteId == routeId).FirstOrDefault();
                if (route != null)
                {
                    var myParams = new DynamicParameters();
                    myParams.Add("@flag", "d");
                    myParams.Add("@routeId", routeId);
                    //myParams.Add("@pluginId", null);
                    var check = _dao.ExecuteCommandAsync("proc_pluginRouteMap ", myParams, System.Data.CommandType.StoredProcedure).GetAwaiter().GetResult();

                    _proxyConfigProvider.RemoveRoute(routeId);
                    response.Status = true;
                    response.Message = "Route Deleted With Id : " + routeId;
                    response.Data = new
                    {
                    
                        ProvidedRouteId = routeId
                    };

                }
                else
                {
                    response.Status = false;
                    response.Message = "Route Not Found With Id : " + routeId;
                    response.Data = new
                    {
                        ProvidedRouteId = routeId
                    };
                }
              
               
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured !";
                response.Data = new
                {
                    ErrrorDescription = ex.Message,
                    ProvidedRouteId = routeId
                };
            }
            return response;

          

        }

        public ResponseModel GetAllRoutes()
        {
            var response = new ResponseModel();

            try
            {
                var routes = _proxyConfigProvider.GetAllRoutes();
                response.Status = true;
                response.Message = "List of Available Routes !";
                response.Data = routes;

            }
            catch (Exception ex) 
            {
                response.Status = false;
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                };
            }


           
            return response;
        }

        public ResponseModel UpdateRoute(string routeId,UpdateRouteMdoel model)
        {
            var response = new ResponseModel();
            try
            {
                var route = _proxyConfigProvider.GetAllRoutes().Where(x => x.RouteId == routeId).FirstOrDefault();
                if (route != null)
                {
                    var copiedRoute = route with
                    {
                        ClusterId = model.ClusterId,
                        AuthorizationPolicy = model.AuthorizationPolicy,
                        TimeoutPolicy = model.TimeoutPolicy,
                        CorsPolicy = model.CorsPolicy,
                        Match = new RouteMatch()
                        {
                            Methods = model.Match.Method,
                            Path = model.Match.Path,
                        },
                        Transforms = model.Transforms,
                        Metadata = model.Metadata,
                    };
                    _proxyConfigProvider.UpdateRoute(copiedRoute);
                    if (copiedRoute.Metadata != null && copiedRoute.Metadata.Count > 0)
                    {
                        var checkPlugin = copiedRoute.Metadata.TryGetValue("PluginId", out var pluginId);
                        if (checkPlugin)
                        {
                            var myParams = new DynamicParameters();
                            myParams.Add("@flag", "u");
                            myParams.Add("@pluginId", pluginId);
                            myParams.Add("@routeId", copiedRoute.RouteId);
                            var check =_dao.ExecuteCommandAsync("proc_pluginRouteMap ", myParams, System.Data.CommandType.StoredProcedure).GetAwaiter().GetResult();
                            response.Message = check ? "Updated Route with plugin" : "Updated Route but failed to update Plugin";
                        }
                    }
                    response.Status = true;
                    response.Message = string.IsNullOrEmpty(response.Message) ? "Updated Route " :response.Message ;
                    response.Data = copiedRoute;
                }
                else
                {
                    response.Status = false;
                    response.Message = "Route Not found With Id : " + routeId;
                    response.Data = new
                    {
                        ProvidedRouteId = routeId,
                    };
                    
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ProvidedRouteId = routeId,
                };
            }
            return response;

         
        }

        public ResponseModel PatchRoute(string routeId, PatchRouteModel model)
        {
            var response = new ResponseModel();
            try
            {
                var route = _proxyConfigProvider.GetAllRoutes().Where(x => x.RouteId == routeId).FirstOrDefault();
                if (route != null)
                {
                    var copiedRoute = route with
                    {
                        ClusterId = string.IsNullOrEmpty(model.ClusterId) ? route.ClusterId : model.ClusterId,
                        AuthorizationPolicy = string.IsNullOrEmpty(model.AuthorizationPolicy) ? route.AuthorizationPolicy : model.AuthorizationPolicy.ToString(),
                        TimeoutPolicy = string.IsNullOrEmpty(model.TimeoutPolicy) ? route.TimeoutPolicy : model.TimeoutPolicy,
                        CorsPolicy = string.IsNullOrEmpty(model.CorsPolicy) ? route.CorsPolicy : model.CorsPolicy,
                        RateLimiterPolicy = string.IsNullOrEmpty(model.RateLimiterPolicy) ? route.RateLimiterPolicy : model.RateLimiterPolicy,
                        OutputCachePolicy = string.IsNullOrEmpty(model.OutputCachePolicy) ? route.OutputCachePolicy : model.OutputCachePolicy,
                        Match = new RouteMatch()
                        {
                            Methods = (model.Match != null && model.Match.Method != null) ? model.Match.Method : route.Match.Methods,
                            Path = (model.Match != null && !string.IsNullOrEmpty(model.Match.Path)) ? model.Match.Path : route.Match.Path,

                        },
                        Transforms = model.Transforms == null ? route.Transforms : model.Transforms,
                        Metadata = model.Metadata == null ? route.Metadata : model.Metadata
                    };
                    _proxyConfigProvider.UpdateRoute(copiedRoute);
                    response.Status = true;
                    response.Message = "Patched Successfully !";
                    response.Data = copiedRoute;
                    if (copiedRoute.Metadata != null && copiedRoute.Metadata.Count > 0)
                    {
                        var checkPlugin = copiedRoute.Metadata.TryGetValue("PluginId", out var pluginId);
                        if (checkPlugin)
                        {
                            var myParams = new DynamicParameters();
                            myParams.Add("@flag", "u");
                            myParams.Add("@pluginId", pluginId);
                            myParams.Add("@routeId", copiedRoute.RouteId);
                            var execute = _dao.ExecuteCommandAsync("proc_pluginRouteMap ", myParams, System.Data.CommandType.StoredProcedure).GetAwaiter().GetResult();
                            response.Message = execute ? "Patched Route Successfully !" : "Patched Route but Failed to Update Plugin !";
                        }
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Route Not found With Id : " + routeId;
                    response.Data = new
                    {
                        ProvidedRouteId = routeId,
                        ProvidedData = model
                    };
                }


            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ProvidedRouteId = routeId,
                    ProvidedData = model
                };

            }

            return response;
        }
        #endregion



        public void UpdateConfigFile()
        {
            YarpModel newYarpConfig = new YarpModel()
            {
                Routes = _proxyConfigProvider.GetAllRoutes().ToDictionary(x => x.RouteId, x => x),
                Clusters = _proxyConfigProvider.GetAllCluster().ToDictionary(x => x.ClusterId, x => x),
            };
            var serialized = JsonSerializer.Serialize(newYarpConfig);
            File.WriteAllText("yarpConfig.json", serialized);

        }

        public IProxyConfig GetYarpConfig()
        {
            
            var data = _proxyConfigProvider.GetConfig();
            return data;
        }


        #region Cluster CRUD


        public ResponseModel GetClusterById(string clusterId)
        {
            var response = new ResponseModel();
            try
            {
                var cluster = _proxyConfigProvider.GetAllCluster().Where(x => x.ClusterId == clusterId).FirstOrDefault();
                if (cluster != null)
                {

                    response.Status = true;
                    response.Message = "Cluster with Id : " + clusterId;
                    response.Data = cluster;
                }
                else
                {
                    response.Status = false;
                    response.Message = "Cluster Not Found With Id : " + clusterId;
                    response.Data = new
                    {
                        ProvidedClusterId = clusterId,
                    };
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ProvidedClusterId = clusterId
                };
            }
            return response;
        }
        public ResponseModel CreateCluster(CreateClusterConfigModel model)
        {

            var response = new ResponseModel();
            try
            {
                var destinationConfig = new Dictionary<string, DestinationConfig>();
                if (model.DestinationAddress != null && model.DestinationAddress.Count > 0)
                {
                    foreach (var destinationAddress in model.DestinationAddress)
                    {
                        destinationConfig.Add(destinationAddress.Key, new DestinationConfig()
                        {
                            Address = destinationAddress.Value,
                        });
                    }
                }
                var clusterConfig = new ClusterConfig()
                {
                    ClusterId = Guid.NewGuid().ToString(),
                    Destinations = destinationConfig,
                    LoadBalancingPolicy = model.LoadBalancingPolicy,
                    HttpClient = new HttpClientConfig()
                    {
                        DangerousAcceptAnyServerCertificate = model.HttpClient != null ? model.HttpClient.DangerousAcceptAnyServerCertificate : false,
                        SslProtocols = model.HttpClient != null ? model.HttpClient.GetProtocolEnum().Aggregate(System.Security.Authentication.SslProtocols.None, (current, protocol) => current | protocol) : System.Security.Authentication.SslProtocols.None

                    }
                };
                _proxyConfigProvider.AddCluster(clusterConfig);
                response.Status = true;
                response.Message = "Cluster Created Successfully";
                response.Data = clusterConfig;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured !";
                response.Data = new
                {
                    ErrrorDescription = ex.Message,
                    ProvidedData = model
                };
            }

            return response;
        }

        public ResponseModel GetAllCluster()
        {
            var response = new ResponseModel();
            try
            {
                response.Status = true;
                response.Message = "List of available Clusters !";
                response.Data = _proxyConfigProvider.GetAllCluster();
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured !";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                };
            }
            return response;
        }

        public ResponseModel UpdateCluster(string clusterId, PutClusterConfigModel model)
        {
            var response = new ResponseModel();
            try
            {
                var cluster = _proxyConfigProvider.GetAllCluster().Where(x => x.ClusterId == clusterId).FirstOrDefault();
                if (cluster != null)
                {
                    var destinationConfig = new Dictionary<string, DestinationConfig>();
                    if (model.DestinationAddress != null && model.DestinationAddress.Count > 0)
                    {
                        foreach (var destinationAddress in model.DestinationAddress)
                        {
                            destinationConfig.Add(destinationAddress.Key, new DestinationConfig()
                            {
                                Address = destinationAddress.Value,
                            });
                        }
                    }
                    var copiedCluster = cluster with
                    {
                        Destinations = destinationConfig,
                        LoadBalancingPolicy = model.LoadBalancingPolicy,
                        HttpClient = new HttpClientConfig()
                        {
                            DangerousAcceptAnyServerCertificate = model.HttpClient != null ? model.HttpClient.DangerousAcceptAnyServerCertificate : false,
                            SslProtocols = model.HttpClient != null ? model.HttpClient.GetProtocolEnum().Aggregate(System.Security.Authentication.SslProtocols.None, (current, protocol) => current | protocol) : System.Security.Authentication.SslProtocols.None
                        }
                    };
                    _proxyConfigProvider.UpdateCluster(copiedCluster);
                    response.Status = true;
                    response.Message = "Updated Successfully !";
                    response.Data = copiedCluster;
                }
                else
                {
                    response.Status = false;
                    response.Message = "Cluster Not found With Id : " + clusterId;
                    response.Data = new
                    {
                        ProvidedClusterId = clusterId,
                        ProvidedData = model
                    };

                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ProvidedClusterId = clusterId,
                    ProvidedData = model
                };

            }

            return response;
        }

        public ResponseModel PatchCluster(string clusterId, PutClusterConfigModel model)
        {
            var response = new ResponseModel();
            try
            {
                var cluster = _proxyConfigProvider.GetAllCluster().Where(x => x.ClusterId == clusterId).FirstOrDefault();
                if (cluster != null)
                {
                    var destinationConfig = new Dictionary<string, DestinationConfig>();
                    if (model.DestinationAddress != null && model.DestinationAddress.Count > 0)
                    {
                        foreach (var destinationAddress in model.DestinationAddress)
                        {
                            destinationConfig.Add(destinationAddress.Key, new DestinationConfig()
                            {
                                Address = destinationAddress.Value,
                            });
                        }

                    }
                    var copiedCluster = cluster with
                    {
                        Destinations = destinationConfig.Count != 0 ? destinationConfig : cluster.Destinations,
                        LoadBalancingPolicy = string.IsNullOrEmpty(model.LoadBalancingPolicy) ? cluster.LoadBalancingPolicy : model.LoadBalancingPolicy,
                        HttpClient = new HttpClientConfig()
                        {
                            DangerousAcceptAnyServerCertificate = model.HttpClient != null ? model.HttpClient.DangerousAcceptAnyServerCertificate : cluster.HttpClient.DangerousAcceptAnyServerCertificate,
                            SslProtocols = model.HttpClient != null ? model.HttpClient.GetProtocolEnum().Aggregate(System.Security.Authentication.SslProtocols.None, (current, protocol) => current | protocol) : System.Security.Authentication.SslProtocols.None
                        },

                    };
                    _proxyConfigProvider.UpdateCluster(copiedCluster);
                    response.Status = true;
                    response.Message = "Patched Successfully !";
                    response.Data = copiedCluster;
                }
                else
                {
                    response.Status = false;
                    response.Message = "Cluster Not found With Id : " + clusterId;
                    response.Data = new
                    {
                        ProvidedClusterId = clusterId,
                        ProvidedData = model
                    };
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ProvidedClusterId = clusterId,
                    ProvidedData = model
                };

            }

            return response;


        }





        #endregion


    }
}
