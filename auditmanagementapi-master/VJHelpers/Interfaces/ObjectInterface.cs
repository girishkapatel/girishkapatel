using System;
using System.Collections.Generic;
using System.Text;

namespace VJLiabraries.Interfaces
{
    public interface IMongoObjWithId
    {
        string Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }


}
