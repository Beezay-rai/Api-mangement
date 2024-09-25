using Yarp.Models;

namespace Yarp.Interfaces
{
    public interface IConsumerCredential
    {
        Task<ResponseModel> AddBasicCred(BasicCredModel model);
        Task<ResponseModel> GetBasicCred();
        Task<ResponseModel> UpdateBasicCred(string basicCredId, BasicCredModel model);
    }
}
