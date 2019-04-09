using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliBuu.Models.Includes;

namespace AliBuuTest.Tests
{
    public class Products : IEnumerable<AliItem>
    {
        private List<AliItem> items;

        public IEnumerator<AliItem> GetEnumerator()
        {
            foreach (var c in items)
                yield return c;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
