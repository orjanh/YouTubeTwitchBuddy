using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using YouTubeLister.Models;

namespace YouTubeLister
{
    public partial class FeedCustomizerApp
    {
        protected TwitchEngine TwitchEngine;
        protected YouTubeEngine YouTubeEngine;

        public FeedCustomizerApp()
        {
            InitializeComponent();

            TwitchEngine = new TwitchEngine();
            YouTubeEngine = new YouTubeEngine();

            tbTwitchChannels.Text = ConfigHelper.GetTwitchChannels();
            tbYouTubeChannels.Text = ConfigHelper.GetYouTubeChannelNames();

            Refresh();
        }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(tbNumDaysBack.Text)) return;

            var maxDate = GetMaxDate();
            var youTubeVideos = YouTubeEngine.FetchRecentYouTubeVideos(tbYouTubeChannels.Text, maxDate);
            var twitchVideos = TwitchEngine.FetchRecentTwitchVideos(tbTwitchChannels.Text, maxDate);

            var sortedVideos = new List<VideoItem>();
            sortedVideos.AddRange(youTubeVideos);
            sortedVideos.AddRange(twitchVideos);

            AddVideoElements(sortedVideos.OrderByDescending(x => x.TimeStamp));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbResult.Inlines.Clear();
            Refresh();
        }

        private void AddVideoElements(IEnumerable<VideoItem> videos)
        {
            foreach (var video in videos)
            {
                if (!string.IsNullOrEmpty(video.ThumbnailUrl))
                {
                    Image thumbnailImage = new Image();
                    BitmapImage bitImg = new BitmapImage(new Uri(video.ThumbnailUrl));
                    thumbnailImage.Width = 325;
                    thumbnailImage.Source = bitImg;
                    tbResult.Inlines.Add(thumbnailImage);
                    tbResult.Inlines.Add(new LineBreak());
                }
                var formattedTitle = GetFormattedTitle(video.Title, video.Duration);
                Hyperlink hyperlink = new Hyperlink(new Run(formattedTitle)) { NavigateUri = video.Url };
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                tbResult.Inlines.Add(hyperlink);
                tbResult.Inlines.Add(new LineBreak());

                var serviceLogo = video.IsTwitch ? "Images/icon-twitch.png" : "Images/youtube.png";
                Image imgLogo = new Image();
                BitmapImage biLogo = new BitmapImage(new Uri(serviceLogo, UriKind.Relative));
                imgLogo.Width = 16;
                imgLogo.Height = 16;
                imgLogo.Margin = new Thickness(0, 4, 5, -3);
                imgLogo.Source = biLogo;
                tbResult.Inlines.Add(imgLogo);

                tbResult.Inlines.Add(string.Format("Posted {0} by {1}", GetDatePosted(video.TimeStamp), video.ChannelName));
                tbResult.Inlines.Add(new LineBreak());
                tbResult.Inlines.Add(new LineBreak());
            }
        }

        private static string GetDatePosted(DateTimeOffset date)
        {
            if (date.Date == DateTime.Today.Date)
            {
                return "today";
            }

            if (date.Date == DateTime.Today.Date.AddDays(-1))
            {
                return "yesterday";
            }

            return date.ToString("dd.MM.yy");
        }

        private static string GetFormattedTitle(string videoTitle, string duration)
        {
            var durationString = string.Empty;

            if (!string.IsNullOrEmpty(duration))
            {
                var durat = TimeSpan.FromSeconds(Convert.ToInt32(duration));

                durationString = string.Format("({0})", GetFormattedTimeString(durat));
            }

            return string.Format("{0} {1}", videoTitle, durationString);
        }

        private static string GetFormattedTimeString(TimeSpan durat)
        {
                return string.Format("{0:D2}:{1:D2}:{2:D2}",
                    durat.Hours,
                    durat.Minutes,
                    durat.Seconds);
        }

        private DateTime GetMaxDate()
        {
            return DateTime.Now.AddDays(-Convert.ToInt32(tbNumDaysBack.Text));
        }

        private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void BtnIncreaseDays(object sender, RoutedEventArgs e)
        {
            if (tbNumDaysBack.Text == string.Empty || Convert.ToInt32(tbNumDaysBack.Text) < 1) tbNumDaysBack.Text = "1";

            int daysBack = Convert.ToInt32(tbNumDaysBack.Text) + 1;
            tbNumDaysBack.Text = daysBack.ToString(CultureInfo.InvariantCulture);
        }

        private void BtnDecreaseDays(object sender, RoutedEventArgs e)
        {
            if (tbNumDaysBack.Text == string.Empty || Convert.ToInt32(tbNumDaysBack.Text) <= 1)
            {
                tbNumDaysBack.Text = "1";
                return;
            }

            int daysBack = Convert.ToInt32(tbNumDaysBack.Text) - 1;
            tbNumDaysBack.Text = daysBack.ToString(CultureInfo.InvariantCulture);
        }
    }
}
