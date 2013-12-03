using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class Album
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("datetime")]
        public DateTime Datetime { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty("account_url")]
        public string AccountUrl { get; set; }

        [JsonProperty("privacy")]
        public string Privacy { get; set; }

        [JsonProperty("layout")]
        public string Layout { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("ups")]
        public int Ups { get; set; }

        [JsonProperty("downs")]
        public int Downs { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("is_album")]
        public bool IsAlbum { get; set; }

        [JsonProperty("vote")]
        public string Vote { get; set; }

        [JsonProperty("images_count")]
        public int ImagesCount { get; set; }

        [JsonProperty("deletehash")]
        public string DeleteHash { get; set; }

        [JsonProperty("images")]
        public ImageCollection Images { get; set; }
    }

    public class AlbumCollection : List<Album>{}
}
