using Dapper;
using System.Reflection;
using Yarp.Data;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Repositories
{
    public class ConsumerGroupRepository : IConsumerGroup
    {
        private readonly IConfiguration _config;
        private readonly DapperDao _dao;
        public ConsumerGroupRepository(IConfiguration config)
        {
            _dao = new DapperDao(config);
            _config = config;
        }

        public async Task<ResponseModel> AddConsumerGroup(CreateConsumerGroupModel model)
        {
            var response = new ResponseModel();

            try
            {

                var myParams = new DynamicParameters();
                myParams.Add("@flag", "i");
                myParams.Add("@name", model.Name);

                var check = await _dao.ExecuteNonListAsync<int>("proc_consumerGroup", myParams, System.Data.CommandType.StoredProcedure);
                if (check != 0)
                {
                    response.Status = true;
                    response.Message = "Added Consumer Group !";
                    response.Data = new
                    {
                        ProvidedData = model,
                    };
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to Add Consumer Group!";
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

        public async Task<ResponseModel> GetConsumerGroup(string ConsumerGroupId)
        {
            var response = new ResponseModel();
            try
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@flag", "s");
                sqlparams.Add("@id", ConsumerGroupId);
                var data = await _dao.ExecuteNonListAsync<ConsumerGroupModel>("proc_consumerGroup", sqlparams, System.Data.CommandType.StoredProcedure);
                if (data != null)
                {
                    response.Status = true;
                    response.Message = "Get Consumer Group By Id";
                    response.Data = data;
                }
                else
                {
                    response.Status = false;
                    response.Message = "ConsumerGroupId Not Found With Id : " + ConsumerGroupId;
                    response.Data = new
                    {
                        ProvidedConsumerGroupIdId = ConsumerGroupId
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

        public async Task<ResponseModel> GetConsumerGroup()
        {
            var response = new ResponseModel();
            try
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@flag", "s");
                var data = await _dao.ExecuteQueryAsync<ConsumerGroupModel>("proc_consumerGroup", sqlparams, System.Data.CommandType.StoredProcedure);

                response.Status = true;
                response.Message = "Available Consumer Groups";
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

        public async Task<ResponseModel> PatchConsumerGroup(string ConsumerGroupId, CreateConsumerGroupModel model)
        {
            var response = new ResponseModel();
            try
            {
                var checkExist = await GetConsumerGroup(ConsumerGroupId);
                if (checkExist.Status && checkExist.Data != null)
                {
                    var existedConsumerGroup = checkExist.Data as ConsumerGroupModel;
                    var myParams = new DynamicParameters();

                    myParams.Add("@flag", "u");
                    myParams.Add("@id", ConsumerGroupId);
                    myParams.Add("@name", string.IsNullOrEmpty(model.Name) ? existedConsumerGroup.Name : model.Name);
                    var check = await _dao.ExecuteCommandAsync("proc_consumerGroup", myParams, System.Data.CommandType.StoredProcedure);

                    if (check)
                    {
                        response.Status = true;
                        response.Message = "Patched Successfully !";
                        response.Data = new
                        {
                            PatchedConsumerGroupId = ConsumerGroupId,
                            ProvidedData = model
                        };
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Patch failed  !";
                        response.Data = new
                        {
                            FailedConsumerGroupId = ConsumerGroupId,
                            ProvidedData = model
                        };
                    }
                  

                }
                else
                {
                    response.Status = false;
                    response.Message = "Consumer Group Not Found With Id : " + ConsumerGroupId;
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
                    FailedConsumerGroupId = ConsumerGroupId,
                    ProvidedData = model
                };
                response.Status = false;
            }
            return response;
        }

        public async Task<ResponseModel> RemoveConsumerGroup(string ConsumerGroupId)
        {
            var response = new ResponseModel();
            try
            {
                var checkExist = await GetConsumerGroup(ConsumerGroupId);
                if (checkExist.Status && checkExist.Data != null)
                {
                    var sqlparams = new DynamicParameters();
                    sqlparams.Add("@flag", "d");
                    sqlparams.Add("@id", ConsumerGroupId);
                    var data = await _dao.ExecuteNonListAsync<string>("proc_consumerGroup", sqlparams, System.Data.CommandType.StoredProcedure);

                    response.Status = true;
                    response.Message = "Deleted Consumer Group";
                    response.Data = new
                    {
                        ConsumerGroupId
                    };
                }
                else
                {
                    response.Status = false;
                    response.Message = "Consumer Group Not Found With Id : " + ConsumerGroupId;
                    response.Data = new
                    {
                        ConsumerGroupId
                    };
                }

            }
            catch (Exception ex)
            {
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    ConsumerGroupId,
                };
                response.Status = false;
            }
            return response;
        }

        public async Task<ResponseModel> UpdateConsumerGroup(string ConsumerGroupId, CreateConsumerGroupModel model)
        {
            var response = new ResponseModel();
            try
            {
                var checkExist = await GetConsumerGroup(ConsumerGroupId);
                if (checkExist.Status && checkExist.Data != null)
                {
                  

                    var myParams = new DynamicParameters();
                    myParams.Add("@flag", "u");
                    myParams.Add("@id", ConsumerGroupId);
                    myParams.Add("@name", model.Name);
               
                    var check = await _dao.ExecuteCommandAsync("proc_consumerGroup", myParams, System.Data.CommandType.StoredProcedure);

                    if (check)
                    {
                        response.Status = true;
                        response.Message = "Updated Successfully !";
                        response.Data = new
                        {
                            UpdatedConsumerGroupId = ConsumerGroupId,
                            ProvidedData = model
                        };
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Update failed  !";
                        response.Data = new
                        {
                            FailedConsumerGroupIdId = ConsumerGroupId,
                            ProvidedData = model
                        };
                    }
               
                }
                else
                {
                    response.Status = false;
                    response.Message = "ConsumerGroupId Not Found With Id : " + ConsumerGroupId;
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
                    FailedConsumerGroupIdId = ConsumerGroupId,
                    ProvidedData = model
                };
                response.Status = false;
            }
            return response;
        }
    }
}
