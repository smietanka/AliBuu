using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.OpenStack
{
    public class ObjectMetaData
    {
        public string ContentType { get; set; }
        public string Name { get; set; }
        public ParameterType Type { get; set; }
        public object Value { get; set; }
    }
}
