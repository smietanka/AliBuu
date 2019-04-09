using System.Collections.Generic;

namespace AliBuu.Models.Includes
{
    public class FeedbackItem
    {
        public string BuyerName { get; set; }
        public string Comment { get; set; }
        public int Likes { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}
