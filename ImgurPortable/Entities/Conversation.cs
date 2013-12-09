using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class Conversation
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("account_id")]
        public int AccountId { get; set; }

        [JsonProperty("viewed")]
        public bool Viewed { get; set; }

        [JsonProperty("content")]
        public Message Content { get; set; }
    }

    public class ConversationCollection : List<Conversation> { }
}