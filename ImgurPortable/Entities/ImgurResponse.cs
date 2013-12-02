using Newtonsoft.Json;

namespace ImgurPortable.Entities
{
    public class ImgurResponse<TResponseType> 
    {
        [JsonProperty("data")]
        public TResponseType Response { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the API call was a success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if successful; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        [JsonProperty("status")]
        public StatusCode StatusCode { get; set; }
    }
}
