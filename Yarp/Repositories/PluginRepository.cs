using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using OnePoint.PDK.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Yarp.Data;
using Yarp.Interfaces;
using Yarp.Models;

namespace Yarp.Repositories
{
    public class PluginRepository : IPlugin
    {
        private readonly IConfiguration _config;
        private readonly DapperDao _dao;
        private readonly OnePointSettings _settings;

        public PluginRepository(IConfiguration config, OnePointSettings settings)
        {
            _dao = new DapperDao(config);
            _config = config;
            _settings = settings;
        }
        public async Task<ResponseModel> AddPlugin(CreatePluginModel model)
        {
            var response = new ResponseModel();

            try
            {
                var allowedExtensions = new[] { ".dll" };
                String timeStamp = DateTime.Now.ToString();
                var ext = Path.GetExtension(model.DLLFile.FileName);
                string datet = timeStamp.Replace('/', '-').Replace(':', '-');
                string fileName = model.Name + datet + ext;
                //string fileName = model.DLLFile.FileName;
                var myParams = new DynamicParameters();
                myParams.Add("@flag", "i");
                myParams.Add("@id", null);
                myParams.Add("@name", model.Name);
                myParams.Add("@namespace", model.NameSpace);
                myParams.Add("@description", model.Description);
                myParams.Add("@dllFile", fileName);
                myParams.Add("@version", model.Version);
                var check = await _dao.ExecuteNonListAsync<int>("proc_plugins", myParams, System.Data.CommandType.StoredProcedure);
                if (check != 0)
                {
                    var onePointSettings = new OnePointSettings();

                    _config.GetSection(nameof(OnePointSettings)).Bind(onePointSettings);

                    var pathToSave = Path.Combine(onePointSettings.PluginSettings.DllPath, fileName);

                    using (var stream = new FileStream(pathToSave, FileMode.Create))
                    {
                        await model.DLLFile.CopyToAsync(stream);
                    }
                    response.Status = true;
                    response.Message = "Uploaded Plugin sucessfully !";
                    response.Data = new
                    {
                        PluginModel = model,
                        AddedPluginId = check
                    };
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to Upload Plugin!";
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

        public async Task<ResponseModel> GetPlugin(string pluginId)
        {
            var response = new ResponseModel();
            try
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@flag", "s");
                sqlparams.Add("@id", pluginId);
                var data = await _dao.ExecuteNonListAsync<GETPluginModel>("proc_plugins", sqlparams, System.Data.CommandType.StoredProcedure);
                if (data != null)
                {
                    response.Status = true;
                    response.Message = "Get Plugin By Id";
                    response.Data = data;
                }
                else
                {
                    response.Status = false;
                    response.Message = "Plugin Not Found With Id : " +pluginId.ToString();
                    response.Data = new
                    {
                        ProvidedPluginId= pluginId
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

        public async Task<ResponseModel> GetPlugin()
        {
            var response = new ResponseModel();
            try
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@flag", "s");
                var data = await _dao.ExecuteQueryAsync<GETPluginModel>("proc_plugins", sqlparams, System.Data.CommandType.StoredProcedure);

                response.Status = true;
                response.Message = "Available Plugin";
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

        public async Task<ResponseModel> PatchPlugin( PluginModel model)
        {
            var response = new ResponseModel();
            try
            {
                var checkExist = await GetPlugin(model.Id.ToString());
                if (checkExist.Status && checkExist.Data != null)
                {
                    var existedPlugin = checkExist.Data as GETPluginModel;
                    var myParams = new DynamicParameters();

                    string fileName = "";
                    if (model.DLLFile != null)
                    {
                        var allowedExtensions = new[] { ".dll" };
                        String timeStamp = DateTime.Now.ToString();
                        var ext = Path.GetExtension(model.DLLFile.FileName);
                        string datet = timeStamp.Replace('/', '-').Replace(':', '-');
                         fileName =  string.IsNullOrEmpty(model.Name)? existedPlugin.Name :model.Name + datet + ext;
                       
                    }
                    else
                    {
                        fileName= existedPlugin.DLLFile;
                    }
               
                    //string fileName = model.DLLFile.FileName;

               
                    myParams.Add("@flag", "u");
                    myParams.Add("@id", model.Id);
                    myParams.Add("@name", string.IsNullOrEmpty( model.Name)?existedPlugin.Name:model.Name);
                    myParams.Add("@namespace", string.IsNullOrEmpty(model.NameSpace) ? existedPlugin._NameSpace : model.NameSpace);
                    myParams.Add("@description", string.IsNullOrEmpty(model.Description) ? existedPlugin.Description : model.Description);
                    myParams.Add("@dllFile", fileName);
                    myParams.Add("@version", string.IsNullOrEmpty(model.Version.ToString()) ? existedPlugin.Version : model.Version);
                    var check = await _dao.ExecuteCommandAsync("proc_plugins", myParams, System.Data.CommandType.StoredProcedure);

                    if (check)
                    {
                        response.Status = true;
                        response.Message = "Patched Successfully !";
                        response.Data = new
                        {
                            UpdatedPluginId = model.Id,
                            ProvidedData = model
                        };
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Patch failed  !";
                        response.Data = new
                        {
                            FailedPluginId = model.Id,
                            ProvidedData = model
                        };
                    }
                    if (check && model.DLLFile != null)
                    {
                        var onePointSettings = new OnePointSettings();

                        _config.GetSection(nameof(OnePointSettings)).Bind(onePointSettings);

                        var pathToSave = Path.Combine(onePointSettings.PluginSettings.DllPath, fileName);

                        using (var stream = new FileStream(pathToSave, FileMode.Create))
                        {
                            await model.DLLFile.CopyToAsync(stream);
                        }
                        response.Status = true;
                        response.Message = "Patched Successfully !";
                        response.Data = new
                        {
                            UpdatedPluginId = model.Id,
                            ProvidedData = model
                        };

                    }
                
                }
                else
                {
                    response.Status = false;
                    response.Message = "Plugin Not Found With Id : " + model.Id;
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
                    FailedPluginId = model.Id,
                    ProvidedData = model
                };
                response.Status = false;
            }
            return response;
        }

        public Task<ResponseModel> RemovePlugin(string pluginId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel> UpdatePlugin(PluginModel model)
        {
            var response = new ResponseModel();
            try
            {
                var checkExist = await GetPlugin(model.Id.ToString());
                if (checkExist.Status && checkExist.Data !=null)
                {
                    var allowedExtensions = new[] { ".dll" };
                    String timeStamp = DateTime.Now.ToString();
                    var ext = Path.GetExtension(model.DLLFile.FileName);
                    string datet = timeStamp.Replace('/', '-').Replace(':', '-');
                    string fileName = model.Name + datet + ext;
                    //string fileName = model.DLLFile.FileName;

                    var myParams = new DynamicParameters();
                    myParams.Add("@flag", "u");
                    myParams.Add("@id", model.Id);
                    myParams.Add("@name", model.Name);
                    myParams.Add("@namespace", model.NameSpace);
                    myParams.Add("@description", model.Description);
                    myParams.Add("@dllFile", fileName);
                    myParams.Add("@version", model.Version);
                    var check = await _dao.ExecuteCommandAsync("proc_plugins", myParams, System.Data.CommandType.StoredProcedure);

                    if (check)
                    {
                        response.Status = true;
                        response.Message = "Updated Successfully !";
                        response.Data = new
                        {
                            UpdatedPluginId = model.Id,
                            ProvidedData = model
                        };
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Update failed  !";
                        response.Data = new
                        {
                            FailedPluginId = model.Id,
                            ProvidedData = model
                        };
                    }
                    if (check && model.DLLFile.Length > 0)
                    {
                        var onePointSettings = new OnePointSettings();

                        _config.GetSection(nameof(OnePointSettings)).Bind(onePointSettings);

                        var pathToSave = Path.Combine(onePointSettings.PluginSettings.DllPath, fileName);

                        using (var stream = new FileStream(pathToSave, FileMode.Create))
                        {
                            await model.DLLFile.CopyToAsync(stream);
                        }
                        response.Status = true;
                        response.Message = "Updated Successfully !";
                        response.Data = new
                        {
                            UpdatedPluginId = model.Id,
                            ProvidedData = model
                        };

                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Update failed  !";
                        response.Data = new
                        {
                            FailedPluginId = model.Id,
                            ProvidedData = model
                        };

                    }
                }
                else
                {
                    response.Status = false;
                    response.Message= "Plugin Not Found With Id : " +model.Id;
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
                    FailedPluginId = model.Id,
                    ProvidedData = model
                };
                response.Status = false;
            }
            return response;
        }



        public async Task<ResponseModel> CreateConfig(string pluginId,object model)
        {
            var response = new ResponseModel();
            try
            {
                var pluginResponse = await GetPlugin(pluginId);
                if (!pluginResponse.Status)
                {
                    return response;
                }
                var plugin = pluginResponse.Data as GETPluginModel;
                var (validate,validationResult) = ValidateSchema(model, plugin);
                if (validate)
                {
                    var myParams = new DynamicParameters();
                    myParams.Add("@flag", "uc");
                    myParams.Add("@id", pluginId);
                    myParams.Add("@config", JsonSerializer.Serialize( model));
                    var check = await _dao.ExecuteCommandAsync("proc_plugins", myParams, System.Data.CommandType.StoredProcedure);
                    if (check)
                    {



                        response.Status = true;
                        response.Message = "Config Added Successfully !";
                        response.Data = new
                        {
                            UpdatedPluginId = pluginId,
                            ProvidedData = model
                        };
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Config Add failed  !";
                        response.Data = new
                        {
                            FailedPluginId = pluginId,
                            ProvidedData = model
                        };
                    }
                }
            
                else
                {
                    response.Status = false;
                    response.Message = "Validation Failed ! " ;
                    response.Data = new
                    {
                        FailedPluginId = pluginId,
                        ProvidedData = model,
                        ErrorDescription = validationResult.Select(x=>x.ErrorMessage).ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                response.Message = "Error Occured";
                response.Data = new
                {
                    ErrorDescirption = ex.Message,
                    FailedPluginId = pluginId,
                    ProvidedData = model
                };
                response.Status = false;
            }
            return response;
        }




        public async Task<ResponseModel> GetPluginConfig(string pluginId)
        {
            var response = new ResponseModel();
            try
            {
                response = await GetPlugin(pluginId);
                if (response.Status)
                {
                    var plugin = (GETPluginModel)response.Data;

                    if (plugin.Config != null)
                    {
                        response.Message = "Schema of Plugin : " + pluginId;
                        var assembly = Assembly.LoadFrom(_settings.PluginSettings.DllPath + plugin.DLLFile);

                        var type = assembly.GetTypes()
                         .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomSchema)))
                         .FirstOrDefault();


                        response.Data =JsonSerializer.Deserialize<object>(  plugin.Config);
                    }
                    else
                    {
                        response.Message = "Empty Schema of Plugin : " + pluginId;
                        response.Data = new
                        {
                            pluginId
                        };
                    }
                
                }
            
               

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Error Occured ! ";
                response.Data = new
                {
                    ErrorDescription = ex.Message,
                    pluginId
                };

            }
            return response;
        }

        public  (bool valid, List<ValidationResult> result) ValidateSchema(object model,GETPluginModel plugin)
        {
       
            var assembly = Assembly.LoadFrom(_settings.PluginSettings.DllPath + plugin.DLLFile);

            var type = assembly.GetTypes()
             .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomSchema)))
             .FirstOrDefault();

            var deserializedObject = JsonSerializer.Deserialize(JsonSerializer.Serialize(model), type);

            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(deserializedObject, null, null);

            bool isValid = Validator.TryValidateObject(deserializedObject, context, validationResults, true);
            if (isValid) 
            {
                var serviceProvider = CreateServices(assembly, _config);
                using (var scope = serviceProvider.CreateScope())
                {
                    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                    runner.MigrateUp();
                }
            }

            return (isValid, validationResults);
        }

        private IServiceProvider CreateServices(Assembly migrationAssembly, IConfiguration configuration)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSqlServer()
                    .WithGlobalConnectionString(configuration.GetConnectionString("DefaultConnection"))
                    .ScanIn(migrationAssembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider(false);
        }
    }
}
