using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AliBuu.Readers
{
    public class PageItemsReader
    {
        public HtmlNodeCollection Nodes { get; set; }
        public bool HasItems { get; set; }

        private int currentPage = 1;
        private (decimal minPrice, decimal maxPrice) currentPrices = (0, 0.1m);

        private string baseUrl;
        private int maxPages;
        public PageItemsReader(string url, int maxPages)
        {
            baseUrl = url;
            this.maxPages = maxPages;
        }

        /// <summary>
        /// zwraca max 48 itemków dla kazdej strony
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            try
            {
                if (currentPage > maxPages) return false;
                HtmlWeb web = new HtmlWeb();
                SetPricesForCurrentUrl();
                var url = GetCurrentUrl();
                currentPage++;
                // pobierz dla podanego urla produkty
                Trace.WriteLine($"Try to get page items from {url}");
                var doc = web.Load(url);

                // pobierz noda produktow i zwroc true jesli istnieja
                var node = doc.DocumentNode.SelectNodes("//ul[@id='list-items']/li[starts-with(@class, 'list-item')]");
                if (node != null && node.Any())
                {
                    HasItems = true;
                    Nodes = node;
                    return true;
                }
                else
                {
                    //moze z diva?
                    node = doc.DocumentNode.SelectNodes("//div[@id='list-items']/ul/li[starts-with(@class, 'list-item')]");
                    if (node != null && node.Any())
                    {
                        HasItems = true;
                        Nodes = node;
                        return true;
                    }
                    else
                    {
                        currentPage = 1;
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            
            
            HasItems = false;
            return false;
        }

        private string GetCurrentUrl()
        {
            // wygeneruj url dla podanej strony
            var url = new UriBuilder(baseUrl);
            var splittedAbs = url.Path.Split('.');
            var newPath = $"{splittedAbs[0]}/{currentPage}.{splittedAbs[1]}";
            
            var query = HttpUtility.ParseQueryString(url.Query);
            query["minPrice"] = currentPrices.minPrice.ToString();
            query["maxPrice"] = currentPrices.maxPrice.ToString().Replace(",", ".");

            url.Query = query.ToString();
            url.Path = newPath;

            return WebUtility.UrlDecode(url.ToString());
        }

        private void SetPricesForCurrentUrl()
        {
            HtmlWeb web = new HtmlWeb();
            int pageSearchResults = 0;
            while(pageSearchResults < 4750)
            {
                var doc = web.Load(GetCurrentUrl());
                var checkcountResults = doc.DocumentNode.SelectSingleNode("//strong[@class='search-count']");
                if (checkcountResults != null)
                {
                    pageSearchResults = Convert.ToInt32(checkcountResults.InnerText.Replace(",", ""));

                    if (currentPrices.minPrice == 0)
                    {
                        if (pageSearchResults > 4750)
                        {
                            // zmniejsz
                            currentPrices.maxPrice -= 0.1m;
                        }
                        else
                        {
                            //zwieksz
                            currentPrices.maxPrice += 0.1m;
                        }
                    }
                }
            }
        }
    }
}
