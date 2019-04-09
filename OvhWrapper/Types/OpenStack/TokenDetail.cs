using System;
using System.Collections.Generic;

namespace OvhWrapper.Types.OpenStack
{
    public class TokenDetail
    {
        public DateTime Issued_At { get; set; }
        public DateTime Expires { get; set; }
        public string Id { get; set; }
        public TenantDetail Tenant { get; set; }
        public List<string> Audit_Ids { get; set; }
    }
}
