using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WebSpider
{
    public class CrawlThread
    {
        public Thread Thread { get; set; }
        public String Name { get; set; }
        public bool RunStatus { get; set; }
    }
}
