namespace Goblin.BitbucketDownloader.Models
{
    public class OwnerModel
    {
        public string username { get; set; }
        public string display_name { get; set; }
        public string type { get; set; }
        public string uuid { get; set; }
        public LinksModel links { get; set; }
    }
}