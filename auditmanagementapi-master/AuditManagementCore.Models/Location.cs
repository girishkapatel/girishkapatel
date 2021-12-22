using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class Location : BaseObjId
    {
        //[Required(ErrorMessage = "{0} is required")]
        public string Sector { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        public string Division { get; set; }

        public string DivisionDescription { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CountryID { get; set; }

        [ForeignKey("CountryID")]
        public virtual Country Country { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StateID { get; set; }

        [ForeignKey("StateID")]
        public virtual State State { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string CityOrTownID { get; set; }
        //public virtual CityOrTown CityOrTown { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CompanyID { get; set; }

        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }

        public string ProfitCenterCode { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string LocationID { get; set; }

        public string LocationDescription { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        public string RiskIndex { get; set; }

        [ForeignKey("CityId")]
        public CityOrTown CityOrTown { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CityId { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Countrycode { get; set; }
    }
}