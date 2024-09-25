namespace Yarp.Models
{
    public class AccessTokenCommand
    {
        public string grant_type { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        // public string refresh_token { get; set; } 
    }

    public class TokenCommonDetails
    {
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class AccessTokenPayload
    {
        public string user { get; set; }
        public string sub { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public List<string> scope { get; set; } = new List<string>();
        public int nbf { get; set; }
        public int exp { get; set; }
        public string iss { get; set; } = string.Empty;
        public string aud { get; set; } = string.Empty;
    }

}
