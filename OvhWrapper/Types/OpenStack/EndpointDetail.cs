using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.OpenStack
{
    public class EndpointDetail
    {
        public string AdminUrl { get; set; }
        public string Region { get; set; }
        public string InternalUrl { get; set; }
        public string Id { get; set; }
        public string PublicUrl { get; set; }
    }
}
