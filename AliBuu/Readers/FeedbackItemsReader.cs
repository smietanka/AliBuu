using AliBuu.Logger;
using AliBuu.Models.Includes;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AliBuu.Readers
{
    public class FeedbackItemsReader
    {
        public HtmlNodeCollection Nodes { get; set; }
        public bool HasItems { get; set; }

        private int currentPage = 1;
        //https://feedback.aliexpress.com/display/productEvaluation.htm?productId=32836243747&ownerMemberId=202507235&withPictures=true&page=9
        private string baseUrl = "https://feedback.aliexpress.com/display/productEvaluation.htm";

        private long productId;
        private long ownerMemberId;
        private bool withPictures;
        public FeedbackItemsReader(long productId, long ownerMemberId, bool withPictures = true)
        {
            this.productId = productId;
            this.ownerMemberId = ownerMemberId;
            this.withPictures = withPictures;
        }

        public List<FeedbackItem> Get()
        {
            var result = new List<FeedbackItem>();
            while(Read())
            {
                foreach(var feedback in Nodes)
                {
                    var tempFeedback = new FeedbackItem();
                    var text = feedback.SelectSingleNode(".//dt[@class='buyer-feedback']");
                    if(text != null)
                    {
                        tempFeedback.Comment = text.InnerText.Trim();
                    }
                    var buyerFeedback = feedback.SelectSingleNode(".//span[@class='user-name']");
                    if(buyerFeedback != null)
                    {
                        tempFeedback.BuyerName = buyerFeedback.InnerText.Trim();
                    }

                    var feedbackLikes = feedback.SelectSingleNode(".//span[@class='r-digg-count']");
                    if(feedbackLikes != null)
                    {
                        tempFeedback.Likes = Convert.ToInt32(feedbackLikes.InnerText.Trim());
                    }
                    var feedbackPhotos = feedback.SelectNodes(".//dd[@class='r-photo-list']/ul/li[@class='pic-view-item']");

                    if (feedbackPhotos != null && feedbackPhotos.Any())
                    {
                        foreach (var images in feedbackPhotos)
                        {
                            var img = images.SelectSingleNode(".//img");
                            if (img != null)
                            {
                                var imgUrl = img.Attributes["src"].Value;
                                if (!string.IsNullOrEmpty(imgUrl))
                                {
                                    tempFeedback.Images.Add(imgUrl);
                                }
                            }
                        }
                    }
                    
                    result.Add(tempFeedback);
                }
            }
            return result;
        }

        public bool Read()
        {
            HtmlWeb web = new HtmlWeb();
            //create url
            var uriBuilder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["productId"] = productId.ToString();
            query["ownerMemberId"] = ownerMemberId.ToString();
            query["withPictures"] = withPictures.ToString();
            query["page"] = currentPage.ToString();
            currentPage++;
            uriBuilder.Query = query.ToString();
            
            var doc = web.Load(uriBuilder.ToString());
            var node = doc.DocumentNode.SelectNodes("//div[@class='feedback-list-wrap']/div[starts-with(@class, 'feedback-item')]");
            
            if(node != null)
            {
                Log.Info("AliBuu", new string[] { "FeedbackItemsReader", "Read" }, new Dictionary<string, object>()
                {
                    { "PAGE", currentPage-1 },
                    {"URL", uriBuilder.ToString() }
                },
                $"Loaded feedbacks");
                HasItems = true;
                Nodes = node;
                return true;
            }
            HasItems = false;
            return false;
        }
    }
}
