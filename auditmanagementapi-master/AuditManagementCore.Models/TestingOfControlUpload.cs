using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AuditManagementCore.Models
{
    public class TestingOfControlUpload : BaseObjId
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string TestingOfCountrolId { get; set; }
        public string OriginalFileName { get; set; }
        public string UploadedFileName { get; set; }

        public DateTime UploadedDatetime { get; set; }
    }
}