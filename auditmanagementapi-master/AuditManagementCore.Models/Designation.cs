namespace AuditManagementCore.Models
{
    public class Designation : BaseObjId
    {
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string Id { get; set; }

        public string Name { get; set; }
    }
}