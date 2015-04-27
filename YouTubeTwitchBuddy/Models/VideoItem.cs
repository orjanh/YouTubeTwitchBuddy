using System;

namespace YouTubeLister.Models
{
    public class VideoItem
    {
        public string Title { get; set; }
        public string ChannelName { get; set; }
        public string ThumbnailUrl { get; set; }
        public Uri Url { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Duration { get; set; }
        public bool IsTwitch { get; set; }
    }
}
