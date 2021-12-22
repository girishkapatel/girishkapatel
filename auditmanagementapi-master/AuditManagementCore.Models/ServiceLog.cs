using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ServiceLog : BaseObjId
    {
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public string ExceptionMsg { get; set; }
        public string ExceptionSource { get; set; }
        public string MethodName { get; set; }
        public DateTime LogDate { get; set; }
    }
}