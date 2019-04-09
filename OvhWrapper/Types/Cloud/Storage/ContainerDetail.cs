using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.Cloud.Storage
{
    public class ContainerDetail
    {
        public bool Archive { get; set; }
        public long StoredBytes { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string StaticUrl { get; set; }
        public List<string> Cors { get; set; }
        public List<ContainerObject> Objects { get; set; }
        public bool Public { get; set; }
        public long StoredObjects { get; set; }
    }
}
