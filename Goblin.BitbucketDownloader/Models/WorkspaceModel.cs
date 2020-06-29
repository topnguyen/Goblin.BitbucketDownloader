namespace Goblin.BitbucketDownloader.Models
{
    public class Workspace
    {
        public string slug { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public LinksModel LinksModel { get; set; }
        public string uuid { get; set; }
    }
}