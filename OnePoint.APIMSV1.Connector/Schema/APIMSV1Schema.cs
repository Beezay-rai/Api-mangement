using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnePoint.PDK.Schema;

namespace OnePoint.APIMSV1.Connector.Schema
{
    public class APIMSV1Schema : CustomSchema
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PrivateKey { get; set; }
        [Required]
        public string ApiKey { get; set; }
        [Required]
        public string ApiSecret { get; set; }
        public string Description { get; set; }
    }
}
