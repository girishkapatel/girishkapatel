using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ExceptionLog : BaseObjId
    {
        public string ExceptionMsg { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionSource { get; set; }
        public string ExceptionURL { get; set; }
        public DateTime LogDate { get; set; }
    }
}