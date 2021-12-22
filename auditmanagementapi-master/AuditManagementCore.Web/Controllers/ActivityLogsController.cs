using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogsController : VJBaseGenericAPIController<ActivityLog>
    {
        #region Class Properties Declarations
        IMongoDbSettings _dbsetting;

        private readonly IWebHostEnvironment _IWebHostEnvironment;
        private readonly IEmailUtility _IEmailUtility;
        #endregion

        public ActivityLogsController
           (IMongoGenericRepository<ActivityLog> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, IEmailUtility emailUtility) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IWebHostEnvironment = webHostEnvironment;
            _IEmailUtility = emailUtility;
        }
        [HttpGet("getActivityLog")]
        public ActionResult GetActivityLog()
        {
            var tList = _api.GetAllWithInclude<User>();
            var _UserRepo = new MongoGenericRepository<User>(_dbsetting);

            if (tList == null)
            {
                return ResponseNotFound();
            }

            var dashboardQuery = new DashboardQuery();
            dashboardQuery.StartDate = Request.Query["StartDate"];
            dashboardQuery.EndDate = Request.Query["EndDate"];
            if (!string.IsNullOrWhiteSpace(dashboardQuery.EndDate) && !string.IsNullOrWhiteSpace(dashboardQuery.StartDate))
                tList = tList.Where(x => x.LogDate.Date <= DateTime.Parse(dashboardQuery.EndDate).Date && x.LogDate >= DateTime.Parse(dashboardQuery.StartDate));
            return ResponseOK(tList);
        }
        [HttpDelete("deleteActivityLog")]
        public ActionResult DeleteActivityLog()
        {
            try
            {

                var tList = _api.GetAllWithInclude<Audit>();
                if (tList == null)
                {
                    return ResponseNotFound();
                }

                var dashboardQuery = new DashboardQuery();
                dashboardQuery.StartDate = Request.Query["StartDate"];
                dashboardQuery.EndDate = Request.Query["EndDate"];
                if (!string.IsNullOrWhiteSpace(dashboardQuery.EndDate) && !string.IsNullOrWhiteSpace(dashboardQuery.StartDate))
                    tList = tList.Where(x => x.LogDate.Date <= DateTime.Parse(dashboardQuery.EndDate).Date && x.LogDate >= DateTime.Parse(dashboardQuery.StartDate));

                foreach (var item in tList)
                {
                    _api.Delete(item);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return ResponseOK(null);
        }
    }
}
