using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using YouTubeLister.Models;

namespace YouTubeLister
{
    public class YouTubeEngine
    {
        private readonly string _uriFormat;

        public YouTubeEngine()
        {
            _uriFormat = ConfigHelper.GetYouTubeUriFormat();
        }

        public IEnumerable<VideoItem> FetchRecentYouTubeVideos(string channelNames, DateTime maxDate)
        {
            XNamespace media = "http://search.yahoo.com/mrss/";
            var youTubeVideos = new List<VideoItem>();

            if (string.IsNullOrEmpty(channelNames)) return youTubeVideos;

            var youTubeChannelNameArray = channelNames.Split('|');            

            foreach (var channelName in youTubeChannelNameArray)
            {
                XmlReader reader = XmlReader.Create(string.Format(_uriFormat, channelName));
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();

                if (feed == null) return youTubeVideos;

                foreach (SyndicationItem item in feed.Items.Where(c => c.PublishDate > maxDate))
                {
                    VideoItem videoItem = new VideoItem();
                    videoItem.ChannelName = channelName;
                    string thumbnailUrl = string.Empty;
                    string duration = string.Empty;

                    foreach (SyndicationElementExtension ext in item.ElementExtensions)
                    {
                        var xElement = ext.GetObject<XElement>();

                        if (xElement.Name.NamespaceName == media.NamespaceName)
                        {
                            var thumbnailElement = xElement.Descendants().FirstOrDefault(item1 => item1.Name.LocalName == "thumbnail");
                            if (thumbnailElement != null) thumbnailUrl = thumbnailElement.FirstAttribute.Value;

                            var contentElement = xElement.Descendants().FirstOrDefault(item1 => item1.Name.LocalName == "content");
                            if (contentElement != null) duration = contentElement.Attribute("duration").Value;
                        }
                    }

                    if (thumbnailUrl != string.Empty)
                    {
                        videoItem.ThumbnailUrl = thumbnailUrl;
                    }

                    var uriElement = item.Links.FirstOrDefault();
                    if (uriElement != null)
                    {
                        videoItem.Title = item.Title.Text;
                        videoItem.Duration = duration;
                        videoItem.Url = uriElement.Uri;
                        videoItem.TimeStamp = item.PublishDate;

                        youTubeVideos.Add(videoItem);
                    }
                }
            }

            return youTubeVideos;
        }
    }
}
