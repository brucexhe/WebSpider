using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSpider
{
    public class PageContent
    {
        public PageContent() { }

        //
        // 摘要:
        //     The raw data bytes taken from the web response
        public byte[] Bytes { get; set; }
        //
        // 摘要:
        //     String representation of the charset/encoding
        public string Charset { get; set; }
        //
        // 摘要:
        //     The encoding of the web response
        public Encoding Encoding { get; set; }
        //
        // 摘要:
        //     The raw text taken from the web response
        public string Text { get; set; }
    }
}
