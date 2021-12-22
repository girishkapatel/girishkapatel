using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ActivityLog : BaseObjId
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public string ReferenceId { get; set; }
        public string Reference { get; set; }
        public string Type { get; set; }
        public string Event { get; set; }
        public string History { get; set; }
        public DateTime LogDate { get; set; }
    }
}