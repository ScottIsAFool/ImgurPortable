using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class Trophy
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_clean")]
        public string NameClean { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("data_link")]
        public object DataLink { get; set; }

        [JsonProperty("datetime")]
        public int Datetime { get; set; }
    }
    public class TrophyCollection : List<Trophy> { }
}