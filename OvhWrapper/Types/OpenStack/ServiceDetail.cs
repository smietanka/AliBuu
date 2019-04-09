using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.OpenStack
{
    public class ServiceDetail
    {
        public List<EndpointDetail> Endpoints { get; set; }
        public List<string> Endpoints_Links { get; set; }
        public string Type { get; set; }
        public string Nova { get; set; }
    }
}
