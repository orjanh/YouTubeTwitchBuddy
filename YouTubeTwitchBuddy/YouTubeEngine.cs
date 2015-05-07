using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using YouTubeLister.Models;

namespace YouTubeTwitchBuddy
{
    public class YouTubeEngine
    {
        private readonly string _uriFormat;

        public YouTubeEngine()
        {
            _uriFormat = ConfigHelper.GetYouTubeUriFormat();
        }

        private static void WriteChannelIdsToAppSettings(IEnumerable<string> channelIds)
        {
            var sb = new StringBuilder();

            foreach (var channelId in channelIds)
            {
                sb.Append(channelId + "|");
            }

            var channelsString = sb.ToString();

            if (channelsString.EndsWith("|"))
            {
                channelsString = channelsString.Remove(channelsString.Length - 1);
            }

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["YouTubeChannelIDs"].Value = channelsString;
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }

        public string GetData(string uri)
        {
            var client = new WebClient { Proxy = null };
            using (var data = client.OpenRead(uri))
            {
                var reader = new StreamReader(data);
                return reader.ReadToEnd();
            }
        }

        public IEnumerable<VideoItem> FetchRecentYouTubeVideos(string channelNames, DateTime maxDate)
        {
            XNamespace media = "http://search.yahoo.com/mrss/";
            var youTubeVideos = new List<VideoItem>();

            if (string.IsNullOrEmpty(channelNames)) return youTubeVideos;

            var youTubeChannelNameArray = channelNames.Split('|');

            var youTubeChannelIds = ConfigHelper.GetYouTubeChannelIds();

            var youTubeChannelIdArray = NumIdsNotEqualToNumChannels(youTubeChannelIds, youTubeChannelNameArray) 
                ? GetYouTubeChannelIds(youTubeChannelNameArray) 
                : youTubeChannelIds.Split('|');

            int channelIndex = 0;

            foreach (var channelId in youTubeChannelIdArray)
            {
                XmlReader reader = XmlReader.Create(string.Format(_uriFormat, channelId));
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();

                if (feed == null) return youTubeVideos;


                foreach (SyndicationItem item in feed.Items.Where(c => c.PublishDate > maxDate))
                {
                    VideoItem videoItem = new VideoItem();
                    videoItem.ChannelName = youTubeChannelNameArray[channelIndex];
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
                            if (contentElement != null && contentElement.Attribute("duration") != null) duration = contentElement.Attribute("duration").Value;
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

                channelIndex++;
            }


            return youTubeVideos;
        }

        private static bool NumIdsNotEqualToNumChannels(string youTubeChannelIds, ICollection<string> youTubeChannelNameArray)
        {
            return string.IsNullOrEmpty(youTubeChannelIds) || youTubeChannelIds.Split('|').Length != youTubeChannelNameArray.Count;
        }

        private string[] GetYouTubeChannelIds(IEnumerable<string> youTubeChannelNameArray)
        {
            var youtubeChannelUriFormat = ConfigHelper.GetYouTubeChannelUriFormat();

            var channelIds = new List<string>();

            foreach (var youTubeChannelName in youTubeChannelNameArray)
            {
                //scrape the id for each channel

                var channelPageMarkup = GetData(string.Format(youtubeChannelUriFormat, youTubeChannelName));

                var channelPageMarkupDoc = new HtmlDocument();
                channelPageMarkupDoc.LoadHtml(channelPageMarkup);
                var channelIdNodes = channelPageMarkupDoc.DocumentNode.SelectNodes("//button[@data-channel-external-id]");

                if (channelIdNodes != null)
                {
                    var channelId = channelIdNodes.FirstOrDefault();

                    if (channelId != null)
                    {
                        channelIds.Add(channelId.Attributes["data-channel-external-id"].Value);
                    }
                }
            }

            WriteChannelIdsToAppSettings(channelIds);

            return channelIds.ToArray();
        }
    }
}
