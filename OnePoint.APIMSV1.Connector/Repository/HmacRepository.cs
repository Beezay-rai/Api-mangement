using Dapper;
using OnePoint.APIMSV1.Connector.Models;
using OnePoint.PDK.Data;
using OnePoint.PDK.Repository;
using System.Data;

namespace OnePoint.APIMSV1.Connector.Repository
{
    public class HmacRepository : CustomRepository
    {
        private readonly OnePointDao _dao;
        public HmacRepository(OnePointDao dao) : base(dao)
        {
            _dao = dao;
        }

        public async Task<List<HmacModel>> Get()
        {
            var sql = "select * from dbo.HmacAuthentication";
            var result = await _dao.ExecuteListAsync<HmacModel>(sql, null, CommandType.Text);
            return result;
        }

        public async Task<HmacModel> Get(int Id)
        {
            var param = new DynamicParameters();
            param.Add("@Id", Id, DbType.Int32);
            var sql = " select * from dbo.HmacAuthentication where Id = @Id ";
            var result = await _dao.ExecuteNonListAsync<HmacModel>(sql, param, CommandType.Text);
            return result;
        }

        public async Task<int> Create(HmacModel model)
        {
            var param = new DynamicParameters();
            param.Add("@Id", 0, DbType.Int32, ParameterDirection.Output);
            param.Add("@ClientId", model.ClientId);
            param.Add("@ClientSecret", model.ClientSecret);
            var sql = " insert into dbo.HmacAuthentication (ClientId,ClientSecret) values (@ClientId,@ClientSecret) select @Id = @@IDENTITY ";
            var result = await _dao.ExecuteCommandAsync(sql, param, CommandType.Text);
            int newIdentity = param.Get<int>("@Id");
            return newIdentity;
        }

        public async Task<int> Update(HmacModel model)
        {
            var param = new DynamicParameters();
            param.Add("@Id", model.Id);
            param.Add("@ClientId", model.ClientId);
            param.Add("@ClientSecret", model.ClientSecret);
            var sql = " update dbo.HmacAuthentication Set ClientId = @ClientId, ClientSecret = @ClientSecret where Id = @Id ";
            var result = await _dao.ExecuteCommandAsync(sql, param, CommandType.Text);
            return result;
        }

        public async Task<int> Delete(int Id)
        {
            var param = new DynamicParameters();
            param.Add("@Id", Id, DbType.Int32);
            var sql = " delete from dbo.HmacAuthentication where Id = @Id ";
            var result = await _dao.ExecuteCommandAsync(sql, param, CommandType.Text);
            return result;
        }
    }
}
