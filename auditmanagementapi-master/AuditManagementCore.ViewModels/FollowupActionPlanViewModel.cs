using AuditManagementCore.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace AuditManagementCore.ViewModels
{
    public class FollowupActionPlanViewModel
    {
        public string Id { get; set; }
        public string ActionPlan { get; set; }

        public string ImplementationOwnerId { get; set; }

        public User ImplementationOwner { get; set; }

        public string RevisedDate { get; set; }

        public string Comments { get; set; }

        public string FollowupId { get; set; }

        public FollowUp FollowUp { get; set; }

        public IFormFile[] Files { get; set; }
    }

    public class APFiles
    {
        public string AuditId { get; set; }
        public string OriginalFileName { get; set; }
        public string UploadedFileName { get; set; }
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }

        public DateTime UploadedDatetime { get; set; }

        public byte[] FileContent { get; set; }
    }
}