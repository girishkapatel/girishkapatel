using System.Collections.Generic;

namespace AuditManagementCore.Models
{
    public class BusinessCycle : BaseObjId
    {
        public BusinessCycle()
        {
            this.ProcessL1 = new HashSet<ProcessL1>();
        }

        //[Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; }

        public virtual ICollection<ProcessL1> ProcessL1 { get; set; }
    }
}