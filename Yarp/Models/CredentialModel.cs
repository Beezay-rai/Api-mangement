using System.Security;

namespace Yarp.Models
{
    public class BasicCredModel
    {
        public string ConsumerId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class GetBasicCredModel
    {
        public int Id { get; set; }
        public string ConsumerId { get; set; }
        public string Username { get; set; }
        public byte[] Password { get; set; }
    }
}
