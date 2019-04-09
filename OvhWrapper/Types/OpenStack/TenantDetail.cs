using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.OpenStack
{
    public class TenantDetail
    {
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
