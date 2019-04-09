using AliBuu.Base;
using AliBuu.Logger;
using AliBuu.Models.Includes;
using AliBuu.Readers;
using OvhWrapper;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AliBuu
{
    public class Crawl
    {
        private const string BASE_URL = "https://www.aliexpress.com/af/category/{0}.html{1}";
        private Collecter _collecter;
        private CrawlOptions Options;
        private Crawl()
        {
            _collecter = new Collecter(new CollecterOptions());
        }
        public Crawl(CrawlOptions options)
        {
            if (options != null)
            {
                Options = options;
                _collecter = new Collecter(new CollecterOptions() { ThreadCount = Options.ThreadCount });
                if(!OvhStorage.IsConnected)
                {
                    OvhStorage.SetupConnection("zJ9FnjuvKFQQ", "vF7Z8Pcb8drCkXGCWxqZPCvTNzfCe8k4", "891a6a019734496cb16e63309f983e54", "WAW1");
                }
            }
            else
            {
                throw new ArgumentNullException("Options is null");
            }
        }

        public void Start()
        {
            var reader = new PageItemsReader(string.Format(BASE_URL, Options.CategoryId, Options.OrderByOrdersDesc ? "?SortType=total_tranpro_desc" : ""), Options.MaxPages);

            while (reader.Read())
            {
                Parallel.ForEach(reader.Nodes, new ParallelOptions() { MaxDegreeOfParallelism = Options.ThreadCount }, node => {
                    var productInfo = node.SelectSingleNode(".//a[contains(@class, 'product')]");
                    var productNameNode = productInfo.SelectSingleNode(".//span");
                    var productName = "";
                    if(productNameNode == null)
                    {
                        productName = productInfo.InnerText;
                    }
                    else
                    {
                        productName = productNameNode.InnerText;
                    }
                    
                    var productUrl = productInfo.Attributes["href"].Value;
                    var tempurl = new Uri($"https://{productUrl.Trim().Remove(0, 2)}");
                    var url = new Uri($"{tempurl.Scheme}{Uri.SchemeDelimiter}{tempurl.Authority}{tempurl.AbsolutePath}");
                    var pageItem = new PageItem()
                    {
                        Name = WebUtility.HtmlDecode(productName).Trim(),
                        Url = url
                    };
                    _collecter.AddItem(pageItem);
                });
            }

            while(_collecter.IsSaverThreadAlive)
            {
                Log.Info("Crawl", new string[] { "Start" }, null, "Czekam 10 sekund");
                Thread.Sleep(10 * 1000);
            }
            Log.Info("Crawl", new string[] { "Start" }, null, "kończę skanowanie");
        }
    }
}