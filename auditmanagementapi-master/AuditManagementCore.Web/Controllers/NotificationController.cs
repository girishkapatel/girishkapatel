using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : VJBaseGenericAPIController<Notification>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;

        public NotificationController(IMongoGenericRepository<Notification> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }
        [HttpGet("GetNotification/{Id}")]
        public IActionResult GetNotification(string Id)
        {
            var repoDiscussionNoteHistory = new MongoGenericRepository<DiscussionNoteHistory>(_dbsetting);
            var repoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var repoDraftReportHistory = new MongoGenericRepository<DraftReportHistory>(_dbsetting);
            var repoDraftReport = new MongoGenericRepository<DraftReport>(_dbsetting);
            var repoTestingofControlHistory = new MongoGenericRepository<TestingofControlHistory>(_dbsetting);
            var repoRACMD = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
            var repoRACMAuditProcedure = new MongoGenericRepository<RACMAuditProcedure>(_dbsetting);
            var repoUser = new MongoGenericRepository<User>(_dbsetting);

            //var lstNotication = new List<Notification>();
            var lstNotication = _api.GetWithInclude<Role, Company, User>(p => p.UserId != null && p.UserId != Id).OrderByDescending(p => p.CreatedOn); 
            foreach (var item in lstNotication)
            {
                if (item.CreatedOn== DateTime.Now.Date)
                {
                    var objUser= repoUser.GetFirst(p => p.Id == Id);
                    item.UserName= objUser != null ? objUser.FirstName + " " + objUser.LastName : "";
                    
                }
            }

            //var lstDraftReportHistory = repoDraftReportHistory.GetWithInclude<DraftReportHistory>(p => p.UserId != Id);
            //foreach (var item in lstDraftReportHistory)
            //{
            //    if (item.DraftReportDate.Date == DateTime.Now.Date)
            //    {
            //        var objtDraftReport = repoDraftReport.GetFirst(p => p.Id == item.DraftReportID);
            //        if (objtDraftReport != null)
            //        {
            //            var objdiscucussion = repoDiscussionNote.GetFirst(p => p.Id == objtDraftReport.DiscussionNoteID);
            //            var objNotification = new Notification()
            //            {
            //                Status = item.Status,
            //                Message = objtDraftReport != null ? objdiscucussion.ObservationHeading + " Draft Report" : "",
            //                Module = "DraftReport",
            //                ReferenceId = objtDraftReport.AuditId
            //            };
            //            lstNotication.Add(objNotification);
            //        }
            //    }
            //}

            //var lstTestingofControlHistory = repoTestingofControlHistory.GetWithInclude<TestingofControlHistory>(p => p.UserId != Id);
            //foreach (var item in lstTestingofControlHistory)
            //{
            //    if (item.TestingOfControlDate.Date == DateTime.Now.Date)
            //    {
            //        var objRACMD = repoRACMD.GetFirst(p => p.Id == item.RACMAuditProcedureDetailsId);
            //        if (objRACMD != null || objRACMD.Procedure != null)
            //        {
            //            var objNotification = new Notification()
            //            {
            //                Status = item.Status,
            //                Message = objRACMD.Procedure.ProcedureTitle + " Procedure",
            //                Module = "testingofConrol"
            //            };
            //            lstNotication.Add(objNotification);
            //        }
            //    }
            //}
            return ResponseOK(lstNotication);
        }

        [HttpGet("updateNotificationStatus/{Id}/{UserId}")]
        public ActionResult UpdateNotificationStatus(string Id, string UserId)
        {
            var objNotification = this._api.GetFirst(p => p.Id == Id);
            if (objNotification != null)
            {
                objNotification.IsReadable = true;
                objNotification.UpdatedBy = UserId;
                _api.Update(objNotification);

            }
            return Ok(objNotification);
        }
    }
}