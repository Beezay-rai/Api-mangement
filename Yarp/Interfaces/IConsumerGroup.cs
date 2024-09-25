using Yarp.Models;

namespace Yarp.Interfaces
{
    public interface IConsumerGroup
    {
        Task<ResponseModel> AddConsumerGroup(CreateConsumerGroupModel model);
        Task<ResponseModel> UpdateConsumerGroup(string ConsumerGroupId, CreateConsumerGroupModel model);
        Task<ResponseModel> GetConsumerGroup(string ConsumerGroupId);
        Task<ResponseModel> PatchConsumerGroup(string ConsumerGroupId, CreateConsumerGroupModel model);
        Task<ResponseModel> GetConsumerGroup();
        Task<ResponseModel> RemoveConsumerGroup(string ConsumerGroupId);
    }
}
