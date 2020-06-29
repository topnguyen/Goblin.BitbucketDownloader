using System.Collections.Generic;

namespace Goblin.BitbucketDownloader.Models
{
    public class RepositoriesModel
    {
        public int pagelen { get; set; }
        public List<RepositoryModel> values { get; set; }
        public string next { get; set; }
    }
}