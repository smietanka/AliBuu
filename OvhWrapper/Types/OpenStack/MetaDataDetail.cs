using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.OpenStack
{
    public class MetaDataDetail
    {
        public int Is_Admin { get; set; }
        public List<string> Roles { get; set; }
    }
}
