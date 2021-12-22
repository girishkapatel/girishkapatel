using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using VJLiabraries.Interfaces;

namespace AuditManagementCore.Models
{
    public class BaseObjId : IMongoObjWithId
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    //public class BaseStringId : IMongoObjWithId
    //{
    //    [BsonId]
    //    [BsonRepresentation(BsonType.String)]
    //    public string Id { get; set; }
    //}
}