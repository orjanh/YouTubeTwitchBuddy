using System.Collections.Generic;

namespace YouTubeLister.Models.Twitch
{
    public class RootObject
    {
        public int _total { get; set; }
        public Links _links { get; set; }
        public List<Video> videos { get; set; }
    }
}
