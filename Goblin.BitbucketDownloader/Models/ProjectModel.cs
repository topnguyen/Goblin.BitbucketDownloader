namespace Goblin.BitbucketDownloader.Models
{
    public class ProjectModel
    {
        public LinksModel LinksModel { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string key { get; set; }
        public string uuid { get; set; }
    }
}