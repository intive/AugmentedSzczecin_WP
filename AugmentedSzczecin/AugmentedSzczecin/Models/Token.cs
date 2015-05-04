using Newtonsoft.Json;

namespace AugmentedSzczecin.Models
{
    public class Token
    {
        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string TokenString { get; set; }
    }
}