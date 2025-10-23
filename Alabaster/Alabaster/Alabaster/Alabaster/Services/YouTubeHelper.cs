using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Linq;
using System.Threading.Tasks;


namespace Alabaster.Services
{

    public class YouTubeHelper
    {
        private readonly string _apiKey;
        private readonly string _channelId;

        public YouTubeHelper(string apiKey, string channelId)
        {
            _apiKey = apiKey;
            _channelId = channelId;
        }

     
        /// Returns the videoId of the currently live stream if active; otherwise the most recent uploaded stream/video.
       
        public async Task<string?> GetLatestOrLiveAsync()
        {
            var youtube = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = _apiKey,
                ApplicationName = "CharityWebsite"
            });

            // 1) Try live
            var liveReq = youtube.Search.List("id");
            liveReq.ChannelId = _channelId;
            liveReq.EventType = SearchResource.ListRequest.EventTypeEnum.Live;
            liveReq.Type = "video";
            liveReq.MaxResults = 1;
            var liveResp = await liveReq.ExecuteAsync();

            if (liveResp.Items?.Any() == true)
                return liveResp.Items[0].Id.VideoId;

            // 2) Fallback to most‑recent
            var recentReq = youtube.Search.List("id");
            recentReq.ChannelId = _channelId;
            recentReq.Type = "video";
            recentReq.Order = SearchResource.ListRequest.OrderEnum.Date;
            recentReq.MaxResults = 1;
            var recentResp = await recentReq.ExecuteAsync();

            if (recentResp.Items?.Any() == true)
                return recentResp.Items[0].Id.VideoId;

            // 3) Nothing found
            return null;
        }


    }
}