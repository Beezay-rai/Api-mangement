using OnePoint.PDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OnePoint.APIMSV1.Connector.Models
{
    public class HmacModel : CustomModel
    {
        public int Id { get; set; }

        public string ClientId { get; set; } = null!;

        public string ClientSecret { get; set; } = null!;
    }
}
