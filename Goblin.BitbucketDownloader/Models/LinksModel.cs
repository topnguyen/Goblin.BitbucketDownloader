using System.Collections.Generic;

namespace Goblin.BitbucketDownloader.Models
{
    public class LinksModel
    {
        public LinkModel watchers { get; set; }
        public LinkModel branches { get; set; }
        public LinkModel tags { get; set; }
        public LinkModel commits { get; set; }
        public List<LinkModel> clone { get; set; }
        public LinkModel self { get; set; }
        public LinkModel source { get; set; }
        public LinkModel html { get; set; }
        public LinkModel avatar { get; set; }
        public LinkModel hooks { get; set; }
        public LinkModel forks { get; set; }
        public LinkModel downloads { get; set; }
        public LinkModel pullrequests { get; set; }
        public LinkModel issues { get; set; }
    }

}