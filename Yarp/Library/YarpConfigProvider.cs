using Microsoft.Extensions.Primitives;
using System.Text.Json;
using Yarp.Models;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.Library
{
    public class YarpConfigProvider : IProxyConfigProvider
    {
        private InMemoryConfig _config;
        private CancellationTokenSource _cts;


        public YarpConfigProvider()
        {
            _config = new InMemoryConfig(new List<RouteConfig>(), new List<ClusterConfig>());
            _cts = new CancellationTokenSource();
        }

        public IProxyConfig GetConfig() => _config;

        #region CRUD For Routes 

        public List<RouteConfig> GetAllRoutes()
        {
            return _config.Routes.ToList();
        }
        public void AddRoute(RouteConfig route)
        {
            var oldRoutes = _config.Routes.ToList();
            oldRoutes.Add(route);
            var oldCluster = _config.Clusters.ToList();
            UpdateConfig(oldRoutes, oldCluster);

        }

        public void RemoveRoute(string routeId)
        {
            var oldRoutes = _config.Routes.ToList();
            var toRemoveRoute = oldRoutes.Where(x => x.RouteId == routeId).FirstOrDefault();
            if (toRemoveRoute != null)
            {
                oldRoutes.Remove(toRemoveRoute);
            }
            var oldCluster = _config.Clusters.ToList();
            UpdateConfig(oldRoutes, oldCluster);

        }

        public void UpdateRoute(RouteConfig route)
        {
            var oldRoutes = _config.Routes.ToList();
            var toRemoveRoute = oldRoutes.Where(x => x.RouteId == route.RouteId).FirstOrDefault();
            if (toRemoveRoute != null)
            {
                oldRoutes.Remove(toRemoveRoute);
            }
            oldRoutes.Add(route);
            var oldCluster = _config.Clusters.ToList();
            UpdateConfig(oldRoutes, oldCluster);

        }
        #endregion

        #region CRUD for Cluster
        public List<ClusterConfig> GetAllCluster()
        {
            return _config.Clusters.ToList();
        }
      
        public void AddCluster(ClusterConfig cluster)
        {
            var oldCluster = _config.Clusters.ToList();
            oldCluster.Add(cluster);
            var oldRoutes = _config.Routes.ToList();
            UpdateConfig(oldRoutes, oldCluster);

        }
        public void RemoveCluster(string ClusterId)
        {
            var oldClusters = _config.Clusters.ToList();
            var toRemoveCluster = oldClusters.Where(x => x.ClusterId == ClusterId).FirstOrDefault();
            if (toRemoveCluster != null)
            {
                oldClusters.Remove(toRemoveCluster);
            }
            var oldRoutes = _config.Routes.ToList();
            UpdateConfig(oldRoutes, oldClusters);

        }

        public void UpdateCluster(ClusterConfig Cluster)
        {
            var oldClusters = _config.Clusters.ToList();
            var toRemoveCluster = oldClusters.Where(x => x.ClusterId == Cluster.ClusterId).FirstOrDefault();
            if (toRemoveCluster != null)
            {
                oldClusters.Remove(toRemoveCluster);
            }
            oldClusters.Add(Cluster);
            var oldRoute = _config.Routes.ToList();
            UpdateConfig(oldRoute, oldClusters);

        }

        #endregion





        public void UpdateConfig(List<RouteConfig> routes, List<ClusterConfig> clusters)
        {
            var oldConfig = _config; 
            _config = new InMemoryConfig(routes, clusters);
            oldConfig.SignalChange(routes,clusters);

        }


        public bool CheckCluster(string clusterId)
        {
            return _config.Clusters.Any(x => x.ClusterId == clusterId);
        }



    }

    public class InMemoryConfig : IProxyConfig
    {
        private List<RouteConfig> _routes;
        private List<ClusterConfig> _clusters;
        private CancellationTokenSource _cts;
        private readonly string _configFilePath = "yarpConfig.json";

        public InMemoryConfig(List<RouteConfig> routes, List<ClusterConfig> clusters)
        {
            FillYarpConfigFromConfigFile(ref routes, ref clusters);
            _routes = routes;
            _clusters = clusters;
            _cts = new CancellationTokenSource();
        }

        private YarpModel LoadConfigFromFile()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                    return new YarpModel(); 
                var configJson = File.ReadAllText(_configFilePath);
                return JsonSerializer.Deserialize<YarpModel>(configJson) ?? new YarpModel(); 
            }
            catch (Exception ex)
            {
                return new YarpModel(); 
            }
        }



        private void FillYarpConfigFromConfigFile(ref List<RouteConfig> routes, ref List<ClusterConfig> clusters)
        {
            var newConfig = LoadConfigFromFile();

            if (newConfig.Clusters != null || newConfig.Routes != null)
            {
                if (!routes.Any(x => newConfig.Routes.Select(x => x.Value.RouteId).ToList().Contains(x.RouteId)) && !clusters.Any(x => newConfig.Clusters.Select(x => x.Value.ClusterId).ToList().Contains(x.ClusterId)))
                {
                    routes.AddRange(newConfig.Routes.Values.ToList());
                    clusters.AddRange(newConfig.Clusters.Values.ToList());

                }
            }
        }

        public IReadOnlyList<RouteConfig> Routes => _routes;

        public IReadOnlyList<ClusterConfig> Clusters => _clusters;

        public IChangeToken ChangeToken => new CancellationChangeToken(_cts.Token);
        internal void SignalChange(IReadOnlyList<RouteConfig> Routes, IReadOnlyList<ClusterConfig> Clusters)
        {
            YarpModel newYarpConfig = new YarpModel()
            {
                Routes = Routes.ToDictionary(x =>  x.RouteId, x => x),
                Clusters = Clusters.ToDictionary(x => x.ClusterId, x => x),
            };
            var serialized = JsonSerializer.Serialize(newYarpConfig);
            File.WriteAllText("yarpConfig.json", serialized);
            _cts.Cancel();
        }


    }
}
