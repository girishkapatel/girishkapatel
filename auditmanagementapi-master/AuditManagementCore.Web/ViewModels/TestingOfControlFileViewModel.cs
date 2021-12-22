using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditManagementCore.Web.ViewModels
{
    public class TestingOfControlFileViewModel
    {
        public List<IFormFile> files { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TestingOfCountrolId { get; set; }
    }
}
