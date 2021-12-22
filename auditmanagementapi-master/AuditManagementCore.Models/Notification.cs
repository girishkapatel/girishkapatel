using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class Notification : BaseObjId
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public bool IsReadable { get; set; }
        public string Module { get; set; }
        public string ReferenceId { get; set; }
        public string DiscussionNumber { get; set; }
        public string UserName { get; set; }
        public string UserId{ get; set; }
    }
}