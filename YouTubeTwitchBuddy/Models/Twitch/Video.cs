namespace YouTubeLister.Models.Twitch
{
    public class Video
    {
        public string title { get; set; }
        public string description { get; set; }
        public long broadcast_id { get; set; }
        public string _id { get; set; }
        public string recorded_at { get; set; }
        public string game { get; set; }
        public string length { get; set; }
        public string preview { get; set; }
        public string url { get; set; }
        public int views { get; set; }
        public Links2 _links { get; set; }
        public Channel channel { get; set; }
    }
}
