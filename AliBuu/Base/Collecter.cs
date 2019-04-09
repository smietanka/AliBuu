using AliBuu.DataAccess;
using AliBuu.Logger;
using AliBuu.Models.Includes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AliBuu.Base
{
    public class Collecter
    {
        private List<PageItem> pageItems = new List<PageItem>();
        private List<AliItem> aliItems = new List<AliItem>();
        private int BulkSizeToSave = 50;

        private readonly object _lock = new object();
        private Thread SaverThread { get; set; }
        public bool IsSaverThreadAlive { get { return SaverThread.IsAlive; } }
        private CollecterOptions _options;
        public Collecter(CollecterOptions options)
        {
            _options = options;
            // odpal nowy watek w ktorym, produkty beda zapisywane do bazy
            SaverThread = new Thread(new ThreadStart(Saver));
            SaverThread.Start();
        }

        private void Saver()
        {
            int sleepCounter = 0;
            while (true)
            {
                Log.Info("AliBuu", new string[] { "Collecter", "Saver" }, new Dictionary<string, object>()
                {
                    { "SLEEP_COUNTER", sleepCounter}
                }, $"Probuje zapisac dane do bazy");
                if (aliItems.Any())
                {
                    sleepCounter = 0;
                    //wez bulka
                    var itemsToSave = aliItems.Take(BulkSizeToSave).ToList();
                    // skasuj bulka
                    aliItems.RemoveRange(0, itemsToSave.Count());
                    // dla kazdego produktu

                    Log.Info("AliBuu", new string[] { "Collecter", "Saver" }, null, $"Zapisuję do bazy {string.Join(", ", itemsToSave.Select(q => q.OfferNumber))}");

                    //Parallel.ForEach(itemsToSave, new ParallelOptions() { MaxDegreeOfParallelism = _options.ThreadCount }, item => 
                    foreach(var item in itemsToSave)
                    {
                        // zapisz produkt do bazy
                        var productId = SqlDataAccess.SaveProduct(item);
                        Log.Info("AliBuu", new string[] { "Collecter", "Saver" }, null, $"Zapisało do bazy {productId}");
                    }                    
                    Thread.Sleep(1000);
                }
                else
                {
                    sleepCounter++;
                    Thread.Sleep(5 * 1000);
                    if (sleepCounter > 10) break;
                }
            }
            Log.Info("AliBuu", new string[] { "Collecter", "Saver" }, new Dictionary<string, object>()
                {
                    { "SLEEP_COUNTER", sleepCounter}
                }, $"Zamykam zapisywanie produktow");

        }

        public void AddItems(List<PageItem> items)
        {
            if(items.Any())
            {
                var aliItem = items.Select(z => PageItemConverter.ToAliItem(z)).ToList();
                lock (_lock)
                {
                    pageItems.AddRange(items);
                    aliItems.AddRange(aliItem);
                }
            }
        }
        public void AddItem(PageItem item)
        {
            if(item != null)
            {
                var aliItem = PageItemConverter.ToAliItem(item);
                lock (_lock)
                {
                    pageItems.Add(item);
                    aliItems.Add(aliItem);
                }
            }
        }
    }
}
