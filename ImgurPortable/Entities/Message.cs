using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class Message
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("account_id")]
        public int AccountId { get; set; }

        [JsonProperty("recipient_account_id")]
        public int RecipientAccountId { get; set; }

        [JsonProperty("subject")]
        public object Subject { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("parent_id")]
        public int ParentId { get; set; }
    }


    public class MessageCollection : List<Message>{}
}