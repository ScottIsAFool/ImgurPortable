using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class Notification
    {
        [JsonProperty("replies")]
        public ReplyCollection Replies { get; set; }

        [JsonProperty("messages")]
        public MessageCollection Messages { get; set; }
    }
}
