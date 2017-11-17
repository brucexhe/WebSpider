using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebSpider
{

    public class Crawler
    {
        private bool RUN_STATUS = false;
        private Queue<string> URLQueue = new Queue<string>();
        private List<string> HistoryUrlList = new List<string>();
        private readonly object LockURL = new object();
        private CrawlerConfig Config;
        private List<CrawlThread> ThreadList = new List<CrawlThread>();
        private AllCrawlCompletedArgs AllCrawlCompletedArgs;

        public delegate bool CanCrawlHandle(string url);
        public delegate bool CanCrawLinksHandle(string url);
        public delegate void PageCrawlCompletedHandle(object sender, PageCrawlCompletedArgs e);
        public delegate void AllCrawlCompletedHandle(object sender, AllCrawlCompletedArgs e);

        public event CanCrawlHandle CanCrawlEvent;
        public event CanCrawLinksHandle CanCrawLinksEvent;
        public event PageCrawlCompletedHandle PageCrawlCompletedEvent;
        public event AllCrawlCompletedHandle AllCrawlCompletedEvent;


        public Crawler(string url)
        {
            this.Config = new CrawlerConfig();
            URLQueue.Enqueue(url);
        }
        public Crawler(CrawlerConfig config, string url)
        {
            this.Config = config;
            URLQueue.Enqueue(url);
        }

        public Crawler(List<string> urls)
        {
            this.Config = new CrawlerConfig();
            foreach (var url in urls)
            {
                URLQueue.Enqueue(url);
            }
        }
        public Crawler(CrawlerConfig config, List<string> urls)
        {
            this.Config = config;
            foreach (var url in urls)
            {
                URLQueue.Enqueue(url);
            }
        }


        private void InitConfig()
        {
            AllCrawlCompletedArgs = new AllCrawlCompletedArgs();

            if (this.CanCrawlEvent == null)
            {
                this.CanCrawlEvent = new CanCrawlHandle((url) => { return true; });
            }
            if (this.PageCrawlCompletedEvent == null)
            {
                this.PageCrawlCompletedEvent = new PageCrawlCompletedHandle((obj, e) => { });
            }
            if (this.CanCrawLinksEvent == null)
            {
                this.CanCrawLinksEvent = new CanCrawLinksHandle((url) => { return false; });
            }
            if (this.AllCrawlCompletedEvent == null)
            {
                this.AllCrawlCompletedEvent = new AllCrawlCompletedHandle((obj, e) =>
                {
                    HistoryUrlList.Clear();

                });
            }

        }

        public void Start()
        {
            InitConfig();
            RUN_STATUS = true;

            for (int i = 0; i < Config.ThreadCount; i++)
            {
                var th = new CrawlThread();
                th.Name = "th_" + i;
                th.RunStatus = true;

                th.Thread = new System.Threading.Thread(new System.Threading.ThreadStart(WorkThread));
                th.Thread.IsBackground = true;
                th.Thread.Name = th.Name;



                ThreadList.Add(th);
                ThreadList[i].Thread.Start();
            }

            Thread threadCheckCompleted = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    if (RUN_STATUS == false)
                    {
                        AllCrawlCompletedEvent(this, AllCrawlCompletedArgs);
                        break;
                    }
                    if (ThreadList.Count(F => F.RunStatus) == 0)
                    {
                        AllCrawlCompletedEvent(this, AllCrawlCompletedArgs);
                        break;
                    }

                }
            }));
            threadCheckCompleted.IsBackground = true;
            threadCheckCompleted.Start();

        }

        public void Stop()
        {
            RUN_STATUS = false;
        }

        private void WorkThread()
        {
            while (RUN_STATUS)
            {
                Thread.Sleep(Config.TimeSpan);

                var url = "";
                lock (LockURL)
                {
                    if (URLQueue.Count > 0)
                    {
                        url = URLQueue.Dequeue();
                    }
                }

                if (!string.IsNullOrWhiteSpace(url))
                {
                    if (this.CanCrawlEvent(url))
                    {
                        CrawlContent(url);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    ThreadList.FirstOrDefault(f => f.Name == Thread.CurrentThread.Name).RunStatus = false;
                    break;
                }

            }

        }

        private void CrawlContent(string url)
        {
            if (HistoryUrlList.Contains(url))
            {
                return;
            }
            else
            {
                HistoryUrlList.Add(url);
            }

            PageCrawlCompletedArgs e = new PageCrawlCompletedArgs();
            e.Url = url;

            try
            {
                WebClient MyWebClient = new WebClient();
                MyWebClient.Encoding = Config.Encoding;
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                Byte[] pageData = MyWebClient.DownloadData(url); //从指定网站下载数据
                string pageHtml = Config.Encoding.GetString(pageData);

                e.PageContent.Bytes = pageData;
                e.PageContent.Text = pageHtml;

                e.CQDocument = CsQuery.CQ.CreateDocument(pageHtml);


            }

            catch (WebException webEx)
            {
                e.WebException = webEx;
            }
            finally
            {
                PageCrawlCompletedEvent(this, e);
                if (this.CanCrawLinksEvent(url))
                {
                    CrawLinks(url, e.PageContent.Text);
                }
            }



        }

        private void CrawLinks(string url, String html)
        {
            try
            {
                Regex reg = new Regex(@"<a\shref\s*=""(?<URL>[^""]*).*?>(?<title>[^<]*)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MatchCollection mc = reg.Matches(html);
                foreach (Match m in mc)
                {
                    var uri = new Uri(url);

                    string link = m.Groups["URL"].Value;
                    if (!link.StartsWith("http"))
                    {
                        reg = new Regex(@"^(http(s|)://.*?/)");
                        var match = reg.Match(url);
                        if (match.Success)
                        {
                            var host = match.Groups[1].Value;
                            link = host + (link.StartsWith("/") ? link.Substring(1) : "/" + link);
                            if (link == host)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            link = url + (link.StartsWith("/") ? link : "/" + link);
                        }
                    }

                    if (link == url)
                    {
                        continue;
                    }

                    lock (LockURL)
                    {
                        URLQueue.Enqueue(link);
                    }
                }
            }

            catch (WebException webEx)
            {
            }
        }
    }
}
