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

        public static string GetYouTubeChannels()
        {
            return ConfigurationManager.AppSettings["YouTubeChannels"];
        }

        public static string GetTwitchChannels()
        {
            return ConfigurationManager.AppSettings["TwitchChannels"];
        }

    }
}
