namespace Yarp.Models
{
    public class ClientCredentialsTokenResponse
    {

    }

    public class ClientCredentialsSuccessResponse : ClientCredentialsTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    public class ClientCredentialsFailureResponse : ClientCredentialsTokenResponse
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string error_uri { get; set; }
    }
}
