using System.Configuration;

namespace YouTubeLister
{
    public class ConfigHelper
    {

        public static string GetTwitchUriFormat()
        {
            return ConfigurationManager.AppSettings["TwitchUriFormat"];
        }

        public static string GetTwitchUriParameters()
        {
            return ConfigurationManager.AppSettings["TwitchUriParameters"];
        }

        public static string GetYouTubeUriFormat()
        {
            return ConfigurationManager.AppSettings["YouTubeUriFormat"];
        }

        public static string GetYouTubeChannelUriFormat()
        {
            return ConfigurationManager.AppSettings["YouTubeChannelUriFormat"];
        }

        public static string GetYouTubeChannelIds()
        {
            return ConfigurationManager.AppSettings["YouTubeChannelsIDs"];
        }

        public static string GetYouTubeChannelNames()
        {
            return ConfigurationManager.AppSettings["YouTubeChannels"];
        }

        public static string GetTwitchChannels()
        {
            return ConfigurationManager.AppSettings["TwitchChannels"];
        }

    }
}
