using System.Collections.Generic;

namespace AuditManagementCore.Models
{
    public class UserScopeTemplate : BaseObjId
    {
        public string ScopeName { get; set; }
        public List<string> Scopes { get; set; }
        public bool IsActive { get; set; }
        public int Sequence { get; set; }
    }
}