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

namespace YouTubeLister
{
    public class YouTubeEngine
    {
        private readonly string _uriFormat;

        public YouTubeEngine()
        {
            _uriFormat = ConfigHelper.GetYouTubeUriFormat();
        }

        private void WriteChannelIdsToAppSettings(IEnumerable<string> channelIds)
        {
            var sb = new StringBuilder();

            foreach (var channelId in channelIds)
            {
                sb.Append(channelId + "|");
            }

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["YouTubeChannelIDs"].Value = sb.ToString();
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
            string[] youTubeChannelIdArray;

            if (!string.IsNullOrEmpty(youTubeChannelIds))
            {
                youTubeChannelIdArray = youTubeChannelIds.Split('|');

                if (youTubeChannelIdArray.Length < youTubeChannelNameArray.Length)
                {
                    GetYouTubeChannelIds(youTubeChannelNameArray);
                }
            }
            else
            {
                youTubeChannelIdArray = GetYouTubeChannelIds(youTubeChannelNameArray);
            }



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

        private string[] GetYouTubeChannelIds(string[] youTubeChannelNameArray)
        {
            var youtubeChannelUriFormat = ConfigHelper.GetYouTubeChannelUriFormat();

            var channelIds = new List<string>();

            foreach (var youTubeChannelName in youTubeChannelNameArray)
            {
                //scrape the id for each channel

                var channelPageMarkup = GetData(string.Format(youtubeChannelUriFormat, youTubeChannelName));

                var channelPageMarkupDoc = new HtmlDocument();
                channelPageMarkupDoc.LoadHtml(channelPageMarkup);
                var channelIdNodes =
                    channelPageMarkupDoc.DocumentNode.SelectNodes("//button[@data-channel-external-id]");

                if (channelIdNodes != null)
                {
                    var channelId = channelIdNodes.FirstOrDefault();

                    if (channelId != null)
                        channelIds.Add(channelId.Attributes["data-channel-external-id"].Value);
                }
            }

            WriteChannelIdsToAppSettings(channelIds);

            return channelIds.ToArray();
        }
    }
}
