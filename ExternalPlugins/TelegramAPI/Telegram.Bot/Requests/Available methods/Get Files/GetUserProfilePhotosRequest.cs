// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get a list of profile pictures for a user.<para>Returns: A <see cref="UserProfilePhotos"/> object.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetUserProfilePhotosRequest : RequestBase<UserProfilePhotos>, IUserTargetable
    {
        /// <summary>Use this method to get a list of profile pictures for a user.<para>Returns: A <see cref="UserProfilePhotos"/> object.</para></summary>
        public GetUserProfilePhotosRequest() : base("getUserProfilePhotos")
        {
        }

        /// <summary>Unique identifier of the target user</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>Sequential number of the first photo to be returned. By default, all photos are returned.</summary>
        public int? Offset { get; set; }

        /// <summary>Limits the number of photos to be retrieved. Values between 1-100 are accepted. Defaults to 100.</summary>
        public int? Limit { get; set; }
    }
}
