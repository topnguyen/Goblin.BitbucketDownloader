using System;

namespace Goblin.BitbucketDownloader.Models
{
    public class RepositoryModel
    {
        public string scm { get; set; }
        public string website { get; set; }
        public bool has_wiki { get; set; }
        public string uuid { get; set; }
        public LinksModel links { get; set; }
        public string fork_policy { get; set; }
        public string full_name { get; set; }
        public string name { get; set; }
        public ProjectModel project { get; set; }
        public string language { get; set; }
        public DateTime created_on { get; set; }
        public Mainbranch mainbranch { get; set; }
        public Workspace workspace { get; set; }
        public bool has_issues { get; set; }
        public OwnerModel owner { get; set; }
        public DateTime updated_on { get; set; }
        public int size { get; set; }
        public string type { get; set; }
        public string slug { get; set; }
        public bool is_private { get; set; }
        public string description { get; set; }
    }
}