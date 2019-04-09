using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.Cloud.Storage
{
    public class Container
    {
        public long StoredBytes { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string Id { get; set; }
        public long StoredObjects { get; set; }
    }
}
