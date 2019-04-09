using AliBuu.Logger;
using AliBuu.Models.Includes;
using AliBuu.Models.Includes.Extensions;
using AliBuu.Readers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AliBuu.Base
{
    public class PageItemConverter
    {
        public static AliItem ToAliItem(PageItem item)
        {
            var result = new AliItem();

            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(item.Url);
            result.Url = item.Url.ToString();
            result.Name = item.Name;

            //offer number
            result.OfferNumber = Convert.ToInt64(item.Url.Segments.Last().Split('.')[0]);

            //rating
            var rating = doc.GetInnerText("//span[@class='percent-num']");
            if (rating.HasValue)
            {
                result.Rating = Convert.ToDecimal(rating.Value, CultureInfo.InvariantCulture);
            }
            //votes
            var votes = doc.GetInnerText("//span[@class='rantings-num']");
            if (votes.HasValue)
            {
                result.Votes = Convert.ToInt32(Regex.Match(votes.Value, @"\d+").Value);
            }
            //price symbol
            var priceSymbol = doc.GetInnerText("//span[@class='p-symbol']");
            if (priceSymbol.HasValue)
            {
                result.PriceSymbol = priceSymbol.Value;
            }
            //price
            var price = doc.GetInnerText("//span[@id='j-sku-price']");
            if (price.HasValue)
            {
                if (price.Value.Contains("-"))
                {
                    // cena jest zapisana w postaci "od - do"
                    var splitPrice = price.Value.Split('-');
                    // wez mniejsza cene
                    result.Price = Convert.ToDecimal(splitPrice[0], CultureInfo.InvariantCulture);
                }
                else
                {
                    result.Price = Convert.ToDecimal(price.Value, CultureInfo.InvariantCulture);
                }
            }

            //ordernum
            var orders = doc.GetInnerText("//span[@class='order-num']");
            if (orders.HasValue)
            {
                result.Orders = Convert.ToInt32(Regex.Match(orders.Value, @"\d+").Value);
            }
            //discount price
            var dicountPrice = doc.GetInnerText("//span[@id='j-sku-discount-price']");
            if (dicountPrice.HasValue)
            {
                if (dicountPrice.Value.Contains("-"))
                {
                    // cena jest zapisana w postaci "od - do"
                    var splitPrice = dicountPrice.Value.Split('-');
                    // wez mniejsza cene
                    result.DiscountPrice = Convert.ToDecimal(splitPrice[0], CultureInfo.InvariantCulture);
                }
                else
                {
                    result.DiscountPrice = Convert.ToDecimal(dicountPrice.Value, CultureInfo.InvariantCulture);
                }
            }

            //unit
            var unit = doc.GetInnerText("//span[@class='p-unit']");
            if (unit.HasValue)
            {
                result.PriceUnit = unit.Value;
            }

            //images
            foreach (var image in doc.DocumentNode.SelectNodes("//ul[@class='image-thumb-list']/li"))
            {
                var img = image.SelectSingleNode(".//img");
                if (img != null)
                {
                    var imgUrl = img.Attributes["src"].Value;
                    result.LittleImages.Add(imgUrl);
                    var splitImgUrl = imgUrl.Split('_');
                    result.Images.Add(string.Join("_", splitImgUrl.Take(splitImgUrl.Count() - 1)));
                }
            }

            //memberOfferId
            var rgx = new Regex(@"\&ownerMemberId=(\d+)\&");
            var match = rgx.Match(doc.DocumentNode.InnerHtml);
            if (match.Success)
            {
                result.OwnerMemberId = Convert.ToInt64(match.Groups[1].Value);
            }

            //category 

            foreach (var categoryPart in doc.DocumentNode.SelectNodes("//div[@class='ui-breadcrumb']/div[@class='container']/a"))
            {
                if (categoryPart != null)
                {
                    result.CategoryTree.Add(WebUtility.HtmlDecode(categoryPart.InnerText).Trim());
                }
            }
            var currentCategory = doc.GetInnerText("//div[@class='ui-breadcrumb']/div[@class='container']/h2/a");
            if (currentCategory.HasValue)
            {
                result.CategoryTree.Add(WebUtility.HtmlDecode(currentCategory.Value));
            }
            result.Category = string.Join(" > ", result.CategoryTree);
            //specification
            foreach (var specification in doc.DocumentNode.SelectNodes("//ul[starts-with(@class, 'product-property-list')]/li[@class='property-item']"))
            {
                var key = specification.SelectSingleNode(".//span[@class='propery-title']");
                if (key != null)
                {
                    var value = specification.SelectSingleNode(".//span[@class='propery-des']");
                    if (value != null)
                    {
                        var keyToAdd = WebUtility.HtmlDecode(key.InnerText.Replace(":", "").Replace("\\", " ").ToUpper().Trim());
                        var valueToAdd = WebUtility.HtmlDecode(value.InnerText.Trim(new char[] { ',', ' ' }).ToUpper().Trim());
                        if (!result.Specification.ContainsKey(keyToAdd))
                        {
                            result.Specification.Add(keyToAdd, valueToAdd);
                        }
                    }
                }
            }

            //feedbacks
            var reader = new FeedbackItemsReader(result.OfferNumber, result.OwnerMemberId);
            result.Feedbacks = reader.Get();
            Log.Info("AliBuu", new string[] { "AliItem", "Constructor" }, new Dictionary<string, object>()
            {
                { "PRODUCT_NAME", result.Name },
                { "PRODUCT_URL", result.Url }
            }, "Stworzyło AliItem korzystajac z AliPage");

            return result;
        }
        public static AliItem ToAliItem(string url)
        {
            return ToAliItem(new PageItem() { Url = new Uri(url), Name = "zmockowany" });
        }
    }
}
