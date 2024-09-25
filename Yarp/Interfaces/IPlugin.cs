using Yarp.Models;

namespace Yarp.Interfaces
{
    public interface IPlugin
    {
        Task<ResponseModel> AddPlugin(CreatePluginModel model);
        Task<ResponseModel> UpdatePlugin(PluginModel model);
        Task<ResponseModel> GetPlugin(string pluginId);
        Task<ResponseModel> PatchPlugin(PluginModel model);
        Task<ResponseModel> GetPlugin();
        Task<ResponseModel> RemovePlugin(string pluginId);


        Task<ResponseModel> CreateConfig(string pluginId, object model);
        Task<ResponseModel> GetPluginConfig(string pluginId);


    }
}
