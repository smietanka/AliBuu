using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.OpenStack
{
    public class ContainerItem
    {
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }
        public int Bytes { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
    }
}
