using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvhWrapper.Types.OpenStack
{
    public class UserDetail
    {
        public string UserName { get; set; }
        public string Id { get; set; }
        public List<string> Roles_Links { get; set; }
        public string Name { get; set; }
        public List<RoleDetail> Roles { get; set; }
    }
}
