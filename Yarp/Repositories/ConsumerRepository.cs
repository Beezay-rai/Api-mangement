using Dapper;
using System.Reflection;
using Yarp.Data;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Repositories
{
    public class ConsumerRepository : IConsumer
    {
        private readonly IConfiguration _config;
        private readonly DapperDao _dao;
        public ConsumerRepository(IConfiguration config)
        {
            _dao = new DapperDao(config);
            _config = config;
        }

        public async Task<ResponseModel> AddConsumer(CreateConsumerModel model)
        {
            var response = new ResponseModel();

            try
            {

                var myParams = new DynamicParameters();
                myParams.Add("@flag", "i");
                myParams.Add("@name", model.Name);
                myParams.Add("@consumerGroupId", model.ConsumerGroupId);

                var check = await _dao.ExecuteNonListAsync<int>("proc_consumer", myParams, System.Data.CommandType.StoredProcedure);
                if (check != 0)
                {
                    response.Status = true;
                    response.Message = "Added Consumer  !";
                    response.Data = new
                    {
                        ProvidedData = model,
                    };
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to Add Consumer !";
                    response.Data = new
                    {
                        ProvidedData = model,
                    };
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured !";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ProvidedData = model,
                };
            }

            return response;
        }

        public async Task<ResponseModel> GetConsumer(string ConsumerId)
        {
            var response = new ResponseModel();
            try
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@flag", "s");
                sqlparams.Add("@id", ConsumerId);
                var data = await _dao.ExecuteNonListAsync<ConsumerModel>("proc_consumer", sqlparams, System.Data.CommandType.StoredProcedure);
                if (data != null)
                {
                    response.Status = true;
                    response.Message = "Get Consumer  By Id";
                    response.Data = data;
                }
                else
                {
                    response.Status = false;
                    response.Message = "Consumer Not Found With Id : " + ConsumerId;
                    response.Data = new
                    {
                        ProvidedConsumerIdId = ConsumerId
                    };
                }

            }
            catch (Exception ex)
            {
                response.Message = "Error Occured";
                response.Data = ex.Message;
                response.Status = false;
            }
            return response;
        }

        public async Task<ResponseModel> GetConsumer()
        {
            var response = new ResponseModel();
            try
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@flag", "s");
                var data = await _dao.ExecuteQueryAsync<ConsumerModel>("proc_consumer", sqlparams, System.Data.CommandType.StoredProcedure);

                response.Status = true;
                response.Message = "Available Consumer ";
                response.Data = data;
            }
            catch (Exception ex)
            {
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message
                };
                response.Status = false;
            }
            return response;
        }

        public async Task<ResponseModel> PatchConsumer(string ConsumerId, PatchConsumerModel model)
        {
            var response = new ResponseModel();
            try
            {
                var checkExist = await GetConsumer(ConsumerId);
                if (checkExist.Status && checkExist.Data != null)
                {
                    var existedConsumer = checkExist.Data as ConsumerModel;
                    var myParams = new DynamicParameters();

                    myParams.Add("@flag", "u");
                    myParams.Add("@id", ConsumerId);
                    myParams.Add("@name", string.IsNullOrEmpty(model.Name) ? existedConsumer.Name : model.Name);
                    myParams.Add("@consumerGroupId", string.IsNullOrEmpty(model.ConsumerGroupId) ? existedConsumer.ConsumerGroupId : model.ConsumerGroupId);
                    var check = await _dao.ExecuteCommandAsync("proc_consumer", myParams, System.Data.CommandType.StoredProcedure);

                    if (check)
                    {
                        response.Status = true;
                        response.Message = "Patched Successfully !";
                        response.Data = new
                        {
                            PatchedConsumerId = ConsumerId,
                            ProvidedData = model
                        };
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Patch failed  !";
                        response.Data = new
                        {
                            FailedConsumerId = ConsumerId,
                            ProvidedData = model
                        };
                    }


                }
                else
                {
                    response.Status = false;
                    response.Message = "Consumer  Not Found With Id : " + ConsumerId;
                    response.Data = new
                    {
                        ProvidedData = model
                    };
                }
            }
            catch (Exception ex)
            {
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescirption = ex.Message,
                    FailedConsumerId = ConsumerId,
                    ProvidedData = model
                };
                response.Status = false;
            }
            return response;
        }

        public async Task<ResponseModel> RemoveConsumer(string ConsumerId)
        {
            var response = new ResponseModel();
            try
            {
                var checkExist = await GetConsumer(ConsumerId);
                if (checkExist.Status && checkExist.Data != null)
                {
                    var sqlparams = new DynamicParameters();
                    sqlparams.Add("@flag", "d");
                    sqlparams.Add("@id", ConsumerId);
                    var data = await _dao.ExecuteNonListAsync<string>("proc_consumer", sqlparams, System.Data.CommandType.StoredProcedure);

                    response.Status = true;
                    response.Message = "Deleted Consumer ";
                    response.Data = new
                    {
                        ConsumerId
                    };
                }
                else
                {
                    response.Status = false;
                    response.Message = "Consumer  Not Found With Id : " + ConsumerId;
                    response.Data = new
                    {
                        ConsumerId
                    };
                }
            }
            catch (Exception ex)
            {
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ConsumerId,
                };
                response.Status = false;
            }
            return response;
        }

        public async Task<ResponseModel> UpdateConsumer(string ConsumerId, CreateConsumerModel model)
        {
            var response = new ResponseModel();
            try
            {
                var checkExist = await GetConsumer(ConsumerId);
                if (checkExist.Status && checkExist.Data != null)
                {


                    var myParams = new DynamicParameters();
                    myParams.Add("@flag", "u");
                    myParams.Add("@id", ConsumerId);
                    myParams.Add("@name", model.Name);
                    myParams.Add("@consumerGroupId", model.ConsumerGroupId);

                    var check = await _dao.ExecuteCommandAsync("proc_consumer", myParams, System.Data.CommandType.StoredProcedure);

                    if (check)
                    {
                        response.Status = true;
                        response.Message = "Updated Successfully !";
                        response.Data = new
                        {
                            UpdatedConsumerId = ConsumerId,
                            ProvidedData = model
                        };
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Update failed  !";
                        response.Data = new
                        {
                            FailedConsumerIdId = ConsumerId,
                            ProvidedData = model
                        };
                    }

                }
                else
                {
                    response.Status = false;
                    response.Message = "ConsumerId Not Found With Id : " + ConsumerId;
                    response.Data = new
                    {
                        ProvidedData = model
                    };
                }
            }
            catch (Exception ex)
            {
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescirption = ex.Message,
                    FailedConsumerIdId = ConsumerId,
                    ProvidedData = model
                };
                response.Status = false;
            }
            return response;
        }
    }
}
