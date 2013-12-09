using Newtonsoft.Json;
using PropertyChanged;

namespace ImgurPortable.Entities
{
    [ImplementPropertyChanged]
    public class VoteCounts
    {
        [JsonProperty("ups")]
        public int Ups { get; set; }

        [JsonProperty("downs")]
        public int Downs { get; set; }
    }
}
