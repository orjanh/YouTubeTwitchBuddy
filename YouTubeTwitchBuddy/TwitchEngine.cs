using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;
using YouTubeLister.Models;
using YouTubeLister.Models.Twitch;

namespace YouTubeLister
{
    public class TwitchEngine
    {
        private readonly string _uriFormat;
        private readonly string _uriParameters;

        public TwitchEngine()
        {
            _uriFormat = ConfigHelper.GetTwitchUriFormat();
            _uriParameters = ConfigHelper.GetTwitchUriParameters();
        }

        public IEnumerable<VideoItem> FetchRecentTwitchVideos(string channelNames, DateTime maxDate)
        {
            var videoCollection = new List<VideoItem>();

            if (string.IsNullOrEmpty(channelNames)) return videoCollection;

            var twitchChannelsNameArray = channelNames.Split('|');
            
            foreach (var channel in twitchChannelsNameArray)
            {
                var json = new WebClient().DownloadString(string.Format(_uriFormat, channel) + _uriParameters);

                RootObject deserializedProduct = JsonConvert.DeserializeObject<RootObject>(json);

                var twitchVideos = FindPastBroadcastsByStreamer(deserializedProduct, maxDate);

                videoCollection.AddRange(twitchVideos);
            }

            return videoCollection;
        }

        private static IEnumerable<VideoItem> FindPastBroadcastsByStreamer(RootObject deserializedProduct, DateTime maxDate)
        {
            var twitchVideos = new List<VideoItem>();

            foreach(var item in deserializedProduct.videos)
            {
                var date = DateTime.Parse(item.recorded_at);

                if (date > maxDate)
                {
                    var videoitem = new VideoItem
                    {
                        Url = new Uri(item.url),
                        Title = item.title,
                        ChannelName = item.channel.display_name,
                        ThumbnailUrl = item.preview,
                        TimeStamp = date,
                        Duration = item.length.ToString(CultureInfo.InvariantCulture),
                        IsTwitch = true
                    };

                    twitchVideos.Add(videoitem);
                }
            }

            return twitchVideos;
        }

    }
}
