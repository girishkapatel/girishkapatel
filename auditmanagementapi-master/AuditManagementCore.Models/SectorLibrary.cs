using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AuditManagementCore.Models
{
    public class SectorLibrary : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; }
    }
}
