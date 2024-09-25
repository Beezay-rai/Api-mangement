using OnePoint.PDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePoint.PDK.Repository
{
    public abstract class CustomRepository
    {
        private readonly OnePointDao _dao;
        public CustomRepository(OnePointDao dao)
        {
            _dao = dao;
        }
    }
}
