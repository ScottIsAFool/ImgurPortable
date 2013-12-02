using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class BlockedUser
    {
        [JsonProperty("blocked_id")]
        public int BlockedId { get; set; }

        [JsonProperty("blocked_url")]
        public string BlockedUrl { get; set; }
    }

    [ImplementPropertyChanged]
    public class AccountSettings
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("high_quality")]
        public bool HighQuality { get; set; }

        [JsonProperty("public_images")]
        public bool PublicImages { get; set; }

        [JsonProperty("album_privacy")]
        public string AlbumPrivacy { get; set; }

        [JsonProperty("pro_expiration")]
        public bool ProExpiration { get; set; }

        [JsonProperty("accepted_gallery_terms")]
        public bool AcceptedGalleryTerms { get; set; }

        [JsonProperty("active_emails")]
        public object[] ActiveEmails { get; set; }

        [JsonProperty("messaging_enabled")]
        public bool MessagingEnabled { get; set; }

        [JsonProperty("blocked_users")]
        public List<BlockedUser> BlockedUsers { get; set; }
    }

}
