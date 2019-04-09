using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.OpenStack
{
    public class AccessDetail
    {
        public TokenDetail Token { get; set; }
        public List<ServiceDetail> ServiceCatalog { get; set; }
        public UserDetail User { get; set; }
        public MetaDataDetail MetaData { get; set; }
    }
}
