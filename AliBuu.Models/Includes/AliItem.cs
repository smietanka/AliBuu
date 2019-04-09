using System.Collections.Generic;

namespace AliBuu.Models.Includes
{
    public class AliItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Rating { get; set; }
        public int Votes { get; set; }
        public decimal DiscountPrice { get; set; }
        public string Url { get; set; }
        public long OfferNumber { get; set; }
        public long OwnerMemberId { get; set; }
        public int Orders { get; set; }

        public decimal Price { get; set; }
        public string PriceUnit { get; set; }
        public string PriceSymbol { get; set; }
        
        public List<string> Images { get; set; } = new List<string>();
        public List<string> LittleImages { get; set; } = new List<string>();

        public List<FeedbackItem> Feedbacks { get; set; } = new List<FeedbackItem>();

        public Dictionary<string, string> Specification { get; set; } = new Dictionary<string, string>();
        
        public string Category { get; set; }
        public List<string> CategoryTree { get; set; } = new List<string>();
        public int CategoryId { get; set; }
    }
}
