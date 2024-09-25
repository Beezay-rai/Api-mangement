using Yarp.Models;

namespace Yarp.Interfaces
{
    public interface IConsumer
    {
        Task<ResponseModel> AddConsumer(CreateConsumerModel model);
        Task<ResponseModel> UpdateConsumer(string ConsumerId, CreateConsumerModel model);
        Task<ResponseModel> GetConsumer(string ConsumerId);
        Task<ResponseModel> PatchConsumer(string ConsumerId, PatchConsumerModel model);
        Task<ResponseModel> GetConsumer();
        Task<ResponseModel> RemoveConsumer(string ConsumerId);
    }
}

