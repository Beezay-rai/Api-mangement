using Dapper;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Yarp.Data;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Repositories
{
    public class ConsumerCredentialRepository : IConsumerCredential
    {
        private readonly IConfiguration _config;
        private readonly IConsumer _consumer;
        private readonly DapperDao _dao;

        public ConsumerCredentialRepository(IConfiguration config,IConsumer consumer)
        {
            _consumer = consumer;
            _dao = new DapperDao(config);
            _config = config;
        }
        public async Task<ResponseModel> AddBasicCred(BasicCredModel model)
        {
            var response = new ResponseModel();
            try
            {
                var checkConsumer = await _consumer.GetConsumer(model.ConsumerId);
                if (!checkConsumer.Status)
                {
                    return checkConsumer;
                }
                var myParams = new DynamicParameters();
                myParams.Add("@flag", "i");
                myParams.Add("@username", model.Username);  
                myParams.Add("@consumerId", model.ConsumerId);
                myParams.Add("@password", model.Password);
                var Dbresponse = await _dao.ExecuteNonListAsync<int>("proc_basicCred", myParams,System.Data.CommandType.StoredProcedure);
                if(Dbresponse > 0)
                {
                    response.Status = true;
                    response.Message = "Added Basic Cred For Consumer : " + model.ConsumerId;
                    response.Data = new
                    {
                        ProvidedData = model,
                        AddedBasicCredId = Dbresponse
                    };
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to add Basic Cred For Consumer : " + model.ConsumerId;
                    response.Data = new
                    {
                        ProvidedData = model
                    };
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Failed to add Basic Cred For Consumer : " + model.ConsumerId;
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ProvidedData = model
                };
            }
            return response;
        }

        public async Task<ResponseModel> GetBasicCred()
        {

            var response = new ResponseModel();
            try
            {
                var myParams = new DynamicParameters();
                myParams.Add("@flag", "s");
                var Dbresponse = await _dao.ExecuteQueryAsync<GetBasicCredModel>("proc_basicCred", myParams, System.Data.CommandType.StoredProcedure);

       



                response.Status = true;
                response.Message = "Available Basic Cred";
                response.Data = Dbresponse;

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured ! ";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                };
            }
            return response;
        }

        public async Task<ResponseModel> UpdateBasicCred(string basicCredId, BasicCredModel model)
        {
            var response = new ResponseModel();
            try
            {
                var myParams = new DynamicParameters();
                myParams.Add("@flag", "u");
                myParams.Add("@id", basicCredId);
                myParams.Add("@username", model.Username);
                myParams.Add("@consumerId", model.ConsumerId);
                myParams.Add("@password", model.Password);
                var Dbresponse = await _dao.ExecuteNonListAsync<int>("proc_basicCred", myParams, System.Data.CommandType.StoredProcedure);
                if (Dbresponse == 0)
                {
                    response.Status = true;
                    response.Message = "Updated Basic Cred For Consumer : " + model.ConsumerId;
                    response.Data = new
                    {
                        ProvidedData = model
                    };
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to Update Basic Cred For Consumer : " + model.ConsumerId;
                    response.Data = new
                    {
                        ProvidedData = model
                    };
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Failed to Update Basic Cred For Consumer : " + model.ConsumerId;
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ProvidedData = model
                };
            }
            return response;
        }
    }
}
