using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuditManagementCore.Models
{
    public class Sector : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; }
    }
}
