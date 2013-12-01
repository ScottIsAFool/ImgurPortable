using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class Image : ImgurClass
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public object Title { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("datetime")]
        public int Datetime { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("animated")]
        public bool Animated { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonProperty("bandwidth")]
        public int Bandwidth { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("nsfw")]
        public object Nsfw { get; set; }

        [JsonProperty("section")]
        public object Section { get; set; }

        [JsonProperty("deletehash")]
        public string Deletehash { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }

    public class ImageCollection : List<Image>
    {
        
    }
}
