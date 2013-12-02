using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class TopGalleryComment
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("author_id")]
        public int AuthorId { get; set; }

        [JsonProperty("on_album")]
        public bool OnAlbum { get; set; }

        [JsonProperty("album_cover")]
        public object AlbumCover { get; set; }

        [JsonProperty("ups")]
        public int Ups { get; set; }

        [JsonProperty("downs")]
        public int Downs { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("datetime")]
        public int Datetime { get; set; }

        [JsonProperty("parent_id")]
        public int ParentId { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("vote")]
        public object Vote { get; set; }
    }

    public class TopGalleryCommentCollection : List<TopGalleryComment> { }
}