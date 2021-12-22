using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class AuditApprovalMapping : BaseObjId
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string AuditId { get; set; }

        [ForeignKey("AuditId")]
        public virtual Audit Audit { get; set; }

        public List<UserData> UserData { get; set; }
    }

    public class UserData : BaseObjId
    {
        public string Responsibility { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}