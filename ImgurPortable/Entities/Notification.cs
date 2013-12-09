using System.Collections.Generic;
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
        public ConversationCollection Conversations { get; set; }
    }

    public class NotificationCollection : List<Notification>{}
}
