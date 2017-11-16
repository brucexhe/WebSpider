using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace WebSpider
{
    public class PageCrawlCompletedArgs
    {
        public PageCrawlCompletedArgs()
        {
            Url = String.Empty;
            PageContent = new PageContent() { };
            CQDocument = new CsQuery.CQ();
            WebException = new WebException() { };
        }

        public string Url { get; set; }
        public PageContent PageContent { get; set; }
        public CsQuery.CQ CQDocument { get; set; }
        public WebException WebException { get; set; }
    }
}
