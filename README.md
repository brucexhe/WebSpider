# WebSpider
My first open source project,WebSpider ! welcome to commit ,issue
我的第一个开源项目，网站蜘蛛！欢迎提问，改进！

How to use?
怎么使用？

#demo

		static void Main(string[] args)
        {
            CrawlerConfig config = new CrawlerConfig();
            config.Encoding = Encoding.GetEncoding("GB2312");

            Crawler spider = new Crawler(config,"http://www.baidu.com/s?wd=webspider");
            spider.CanCrawlEvent += Spider_CanCrawlEvent;
            spider.CanCrawLinksEvent += Spider_CanCrawLinksEvent;
            spider.PageCrawlCompletedEvent += Spider_PageCrawlCompletedEvent;
            spider.AllCrawlCompletedEvent += Spider_AllCrawlCompletedEvent;
            
            spider.Start();

            Console.Read();

        }

        //All url crawl completed
        //所有URL执行结束
        private static void Spider_AllCrawlCompletedEvent(object sender, AllCrawlCompletedArgs e)
        {
            Console.WriteLine("completed");
        }

        //a url crawl completed,Support csquery what's a framwork of operating dom like jquery
        //当一个Url抓取完成时执行，支持csquery，一个可以像JQUERY一样操作dom的框架
        private static void Spider_PageCrawlCompletedEvent(object sender, PageCrawlCompletedArgs e)
        {
            var title = e.CQDocument.Select(".art_h1").Text();
        }

        private static bool Spider_CanCrawLinksEvent(string url)
        {
            return true;
        }

      
        private static bool Spider_CanCrawlEvent(string url)
        {
            return true;
        }
        
        
        #AND other
        if u have any questioin ,welcome to issue.
