using System;
using System.Collections.Generic;
using System.Text;

namespace AuditManagementCore.Models
{
    public static class AuditConstants
    {
        public static class Status
        {
            public const string ACTIVE = "ACTIVE";
            public const string INACTIVE = "INACTIVE";
            public const string EFFECTIVE = "EFFECTIVE";
            public const string INEFFECTIVE = "INEFFECTIVE";
            public const string FINAL = "FINAL";
            public const string DRAFT = "DRAFT";
            public const string PENDING = "PENDING";
            public const string INPROGRESS = "INPROGRESS";
            public const string COMPLETED = "COMPLETED";
            public const string INREVIEW = "INREVIEW";
            public const string NOTSTRATED = "NOTSTARTED";
        }
        public static class CommonStatus
        {
            public const string APPROVED = "APPROVED";
            public const string REJECTED = "REJECTED";
            public const string SAVETODRAFT = "SAVE TO DRAFT";
            public const string SAVETOFINALREPORT= "SAVE TO FINAL REPORT";
        }
        public static class ServiceConstant
        {
            public const string REMINDER = "reminder";
            public const string REJECTED = "REJECTED";
            public const string SAVETODRAFT = "SAVE TO DRAFT";
            public const string SAVETOFINALREPORT = "SAVE TO FINAL REPORT";
        }

        public static class UserScopeTemplate
        {
            public const string COMMON = "COMMON";
        }
        public static class BackgroundColor
        {
            public const string partiallyreceived = "#FFC200";
            public const string received = "#2c973e";
            public const string pending = "#f04c3e";
        }
    }
}
