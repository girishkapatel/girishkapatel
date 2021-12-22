using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuditManagementCore.Models
{
    public class Country : BaseObjId
    {
        public Country()
        {
            this.State = new HashSet<State>();
        }

        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; }

        public virtual ICollection<State> State { get; set; }
    }
}