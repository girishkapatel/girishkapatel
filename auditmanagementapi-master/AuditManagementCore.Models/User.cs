using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class User : BaseObjId
    {
        //[RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Email is invalid.")]
        [Required(ErrorMessage = "{0} is required")]
        public string EmailId { get; set; }

        //[RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "First name is invalid.")]
        public string FirstName { get; set; }

        //[RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Middle name is invalid.")]
        public string MiddleName { get; set; }

        //[RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Last name is invalid.")]
        public string LastName { get; set; }

        //[RegularExpression(@"^[0-9]*$", ErrorMessage = "Mobile no. is invalid.")]
        public string Mobile { get; set; }

        //[RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Designation is invalid.")]
        [Required(ErrorMessage = "{0} is required")]
        public string Designation { get; set; }

        //[RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Qualification is invalid.")]
        public string Qualification { get; set; }

        public bool StakeHolder { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "{0} is required")]

        public int Experiance { get; set; }

        /*
            Breaking down the regex for password:
            ^                                            Match the beginning of the string
            (?=.*[0-9])                                  Require that at least one digit appear anywhere in the string
            (?=.*[a-z])                                  Require that at least one lowercase letter appear anywhere in the string
            (?=.*[A-Z])                                  Require that at least one uppercase letter appear anywhere in the string
            (?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\])        Require that at least one special character appear anywhere in the string
            .{8,32}                                      The password must be at least 8 characters long, but no more than 32
            $                                            Match the end of the string.
         */

        //[RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\]).{8,32}$", ErrorMessage = "Password is invalid.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoleId { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //[Required(ErrorMessage = "Company is required")]
        //public string CompanyId { get; set; }

        [ForeignKey("RoleId")]

        public Role Role { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ReportToId { get; set; }
        public User ReportTo { get; set; }
        public string UserType { get; set; }
        public string EmployeeCode { get; set; }
        //[ForeignKey("CompanyId")]
        //public Company Company { get; set; }
    }

    public class Role : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public List<string> Scopes { get; set; }
        public List<UIScope> UIScopes { get; set; }
    }

    public class UIScope
    {
        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; }
        public bool Access { get; set; }
        public bool isAdd { get; set; }
        public bool isEdit { get; set; }
        public bool isDelete { get; set; }
        public List<SubModules> Submodules { get; set; }
    }

    public class SubModules
    {
        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; }
        public bool Access { get; set; }
        public bool isAdd { get; set; }
        public bool isEdit { get; set; }
        public bool isDelete { get; set; }
        public List<SubModule> Submodules { get; set; }
    }

    public class SubModule
    {
        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; }
        public bool Access { get; set; }
        public bool isAdd { get; set; }
        public bool isEdit { get; set; }
        public bool isDelete { get; set; }
    }
}