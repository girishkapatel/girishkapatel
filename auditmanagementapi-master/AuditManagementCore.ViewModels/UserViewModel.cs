using System;
using System.Collections.Generic;

namespace AuditManagementCore.ViewModels
{
    public class UserViewModel
    {
        public string EmailId { get; set; }
        public string Password { get; set; }
    }
    public class ForgotPasswordModel
    {
        public string EmailId { get; set; }
        public string ResetURL { get; set; }

    }
    public class Employee
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public int EmployeesCount { get; set; }
        public List<EmployeeDetails> Employees { get; set; }
    }
    public class EmployeeDetails
    {
        public string EmployeeCode { get; set; }
        public int EmployeeID { get; set; }
        public string Salutation { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FatherName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Mobile { get; set; }
        public string DateofBirth { get; set; }
        public string DateOfJoining { get; set; }
        public string Dateofconfirmation { get; set; }
        public string DateOfLeaving { get; set; }
        public string EmployeeStatus { get; set; }
        public string Age { get; set; }
        public string PFAccountNumber { get; set; }
        public string ESICAccountNumber { get; set; }
        public string ReportingManagerName { get; set; }
        public string ReportingManagerCode { get; set; }
        public string LastModified { get; set; }
        public string CreatedDate { get; set; }
        public string EmpFlag { get; set; }
        public string DateofResignation { get; set; }
        public string ExitDate { get; set; }
        public string GroupDOJ { get; set; }
        public string ExitTypeName { get; set; }
        public string ExitReason1 { get; set; }
        public string ExitReason2 { get; set; }
        public string DomainID { get; set; }
        public string PaymentDescription { get; set; }
        public string PTApplicable { get; set; }
        public string PFApplicable { get; set; }
        public string ESICApplicable { get; set; }
        public string LwfApplicable { get; set; }
        public string PfDenotion { get; set; }
        public string EmploymentType { get; set; }
        public string FNFProcessedMonth { get; set; }
        public string NetPay { get; set; }
        public string FNFStatus { get; set; }
        public string RecoveryStatus { get; set; }
        public string EcodeGeneratedBy { get; set; }
        public string OfferLetterReferenceNo { get; set; }
        public string AttendanceManagerStatus { get; set; }
        public string Nationality { get; set; }
        public string MaritalStatus { get; set; }
        public string OldEmployeeCode { get; set; }
        public string AlternateMobileNo { get; set; }
        public string ReportingManagerEmail { get; set; }
        public List<CardCodeDetails> CardCodeDetails { get; set; }
        public List<Attributes> Attributes { get; set; }
        public List<ApproverDetails> ApproverDetails { get; set; }
        public List<ClaimApproverDetails> ClaimApproverDetails { get; set; }
    }
    public class CardCodeDetails
    {
        public string CardCodeNumber { get; set; }
        public string AttendanceMode { get; set; }
    }
    public class ApproverDetails
    {
        public string Module { get; set; }
        public string ApproverName { get; set; }
        public string ApproverEmployeeCode { get; set; }
        public string ApprovalLevel { get; set; }
    }
    public class Attributes
    {
        public int AttributeTypeID { get; set; }
        public string AttributeTypeDesc { get; set; }
        public string AttributeTypeCode { get; set; }
        public int AttributeTypeUnitID { get; set; }
        public string AttributeTypeUnitDesc { get; set; }
        public string AttributeTypeUnitCode { get; set; }
        public string EffectiveDate { get; set; }

    }
    public class ClaimApproverDetails
    {
        public string Module { get; set; }
        public string ClaimType { get; set; }
        public string ApproverEmployeeCode { get; set; }
        public string ApproverName { get; set; }
        public string ClaimFromLimit { get; set; }
        public string ClaimToLimit { get; set; }
        public string ApproverLevel { get; set; }
    }
}
