using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class ErrorInfo
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("request")]
        public string Request { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("parameters")]
        public string Parameters { get; set; }
    }

    [ImplementPropertyChanged]
    public class Error
    {
        [JsonProperty("error")]
        public ErrorInfo ErrorData { get; set; }
    }
}
