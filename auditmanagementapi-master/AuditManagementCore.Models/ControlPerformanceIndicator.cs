using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuditManagementCore.Models
{
    public class ControlPerformanceIndicator : BaseObjId
    {
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string Id { get; set; }

        public string ControlRating { get; set; }
        public int Weightage { get; set; }
    }
}