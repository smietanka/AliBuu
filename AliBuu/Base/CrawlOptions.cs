namespace AliBuu.Base
{
    public class CrawlOptions
    {
        /// <summary>
        /// Category id to crawl
        /// </summary>
        public long CategoryId { get; set; }
        public bool OrderByOrdersDesc { get; set; } = true;
        public int MaxPages { get; set; } = int.MaxValue - 1;

        public bool FeedbackWithPictures { get; set; } = true;
        public int ThreadCount { get; set; } = 8;
        private CrawlOptions()
        {

        }
        public CrawlOptions(long catId)
        {
            CategoryId = catId;
        }
    }
}
