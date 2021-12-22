using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VJLiabraries;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : VJBaseGenericAPIController<Audit>
    {
        #region Class Properties Declarations
        IMongoDbSettings _dbsetting;
        IWebHostEnvironment _IWebHostEnvironment;
        IEmailUtility _IEmailUtility;
        CommonServices _CommonServices;

        private readonly IDocumentUpload _IDocumentUpload;
        #endregion

        public AuditController(IMongoGenericRepository<Audit> api, IMongoDbSettings mongoDbSettings, IDocumentUpload documentUpload, IWebHostEnvironment webHostEnvironment, IEmailUtility emailUtility, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IWebHostEnvironment = webHostEnvironment;
            _IEmailUtility = emailUtility;
            _IDocumentUpload = documentUpload;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] Audit e)
        {
            if (string.IsNullOrWhiteSpace(e.OverallAssesmentId)) return ResponseBad("Audit is null or blank");

            var overallAssessmentRepo = new MongoGenericRepository<OverallAssesment>(_dbsetting);
            var IsExist = overallAssessmentRepo.GetFirst(x => x.Id == e.OverallAssesmentId);
            if (IsExist == null)
            {
                return ResponseBad("Overall Assessment not found");
            }
            else
            {
                try
                {
                    var auditObj = _CommonServices.CreateAuditPlansByOverAllAssesment(IsExist);
                    return Ok(auditObj);
                }
                catch (Exception ex)
                {
                    return ResponseError(ex.Message);
                }
            }
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                if (id == null) return ResponseBad("Audit Plans object is null");
                var objAuditPlans = _api.GetFirst(x => x.Id == id);

                if (objAuditPlans == null)
                {
                    return ResponseError("Audit Plans does not exists");
                }
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, objAuditPlans.AuditName, "Audit", "Audit Planning Engine | Audit Plans | Delete", "Deleted Audit");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }

        [HttpPost("saveunplannedaudit")]
        public ActionResult SaveUnplannedAudit([FromBody] UnplannedAudit unplannedAudit)
        {
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);

            var IsExist = auditRepo.GetFirst(x => x.Id == unplannedAudit.Id);
            if (IsExist == null)
                IsExist = new Audit();
            try
            {
                IsExist.AuditName = unplannedAudit.AuditName;
                IsExist.Locations.Add(new Location() { Id = unplannedAudit.LocationId });
                IsExist.CreatedBy = unplannedAudit.CreatedBy;

                var auditObj = _CommonServices.CreateUnplannedAudit(IsExist);
                return Ok(auditObj);
            }
            catch (Exception ex)
            {
                return ResponseError(ex.Message);
            }
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<OverallAssesment, ProcessLocationMapping>();

            if (tList == null)
                return ResponseNotFound();

            var returnList = tList.Where(a => a.ProcessLocationMapping != null && a.OverallAssesment != null).ToList();
            var repoSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            foreach (var item in returnList)
            {
                try
                {
                    if (item.ProcessLocationMapping != null)
                    {
                        var objScope = repoSchedule.GetFirst(p => p.ProcessLocationMappingId == item.ProcessLocationMapping.Id);
                        item.ScopeAndSchedule = objScope != null ? objScope : null;
                    }

                    if (item.Location != null && item.Location.Id != null)
                        item.Location =  _CommonServices.GetLocationDetail(item.Location.Id);
                    
                }
                catch (Exception err)
                {
                    item.Location = null;
                }
            }

            return ResponseOK(returnList);
        }

        public override ActionResult GetByID(string id)
        {
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);

            var tList = scopeAndScheduleRepo.GetWithInclude<ScopeAndSchedule, Audit>(x => x.AuditId == id).FirstOrDefault();

            if (tList == null)
                return ResponseNotFound();

            return ResponseOK(tList);
        }

        [HttpGet("getlocationbyid/{locationId}")]
        public IActionResult GetLocationByID(string locationId)
        {
            var audit = _CommonServices.GetLocationDetail(locationId);
            if (audit == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(audit);
        }

        [HttpPost("uploadfile")]
        public async Task<IActionResult> UploadFile(IFormFile[] files)
        {
            if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                return ResponseError("formfile is empty");

            var auditId = Request.Form["auditId"].ToString().Trim() == "" ? "0" : Request.Form["auditId"].ToString().Trim();
            var module = Request.Form["module"].ToString().Trim();
            //var location = Request.Form["location"].ToString().Trim();
            //var year = Request.Form["year"].ToString().Trim();
            //var audit = Request.Form["auditName"].ToString().Trim();

            //if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            //{
            //    return ResponseError("Not Support file extension");
            //}

            try
            {
                List<AuditFiles> returnFiles = new List<AuditFiles>();

                //{Location}/{Year}/{Audit}/{Module / Name of the tabs}
                string newPath = Path.Combine("manageaudits", auditId, module.Trim());

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                foreach (var item in files)
                {
                    var res = await _IDocumentUpload.UploadToWebRoot(item, newPath);

                    AuditFiles auditFiles = new AuditFiles();
                    auditFiles.OriginalFileName = item.FileName;
                    auditFiles.UploadedDatetime = DateTime.Now;
                    auditFiles.UploadedFileName = res;
                    auditFiles.ModuleName = module;
                    auditFiles.AuditId = auditId;
                    repo.Insert(auditFiles);
                    
                    //Activity Log
                    _CommonServices.ActivityLog(auditFiles.CreatedBy, auditFiles.Id, "", "AuditFiles", "AuditFiles | Add", "Added AuditFiles");
                    returnFiles.Add(auditFiles);
                }
                return ResponseOK(new { files = returnFiles, isUploaded = true });
            }
            catch (Exception ex)
            {
                return ResponseOK(new { isUploaded = false });
            }
        }

        [HttpDelete("removefile/{id}")]
        public IActionResult RemoveUploadedFile(string id)
        {
            try
            {
                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var deleteFile = repo.GetByID(id);

                //{Location}/{Year}/{Audit}/{Module / Name of the tabs}
                //string filePath = Path.Combine("manageaudits", auditId, "unplannedaudits", deleteFile.UploadedFileName);

                if (!_IDocumentUpload.IsExists(deleteFile.UploadedFileName))
                    return ResponseOK("File does not exists.");

                _IDocumentUpload.DeleteFile(deleteFile.UploadedFileName);

                repo.Delete(deleteFile);

                return ResponseOK(new { isDeleted = true });
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }

        [HttpGet("getallfiles/{id}")]
        public ActionResult GetAllFiles(string id)
        {
            var auditModel = _api.GetWithInclude<Audit>(x => x.Id == id).FirstOrDefault();

            if (auditModel == null)
                return ResponseNotFound();

            //var Id = id == null || id == "" ? "0" : id;
            //var location = auditModel.Location.ProfitCenterCode.Replace("/", "-").Trim();
            //var year = DateTime.Now.Year.ToString();
            //var audit = auditModel.AuditName.Replace("/", "-").Trim();

            try
            {
                var returnFiles = new List<AuditFiles>();

                //{Location}/{Year}/{Audit}/{Module / Name of the tabs}
                string newPath = Path.Combine("manageaudits", id, "unplannedaudits");

                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var auditFiles = repo.GetMany(a => a.AuditId == id);

                foreach (var file in auditFiles)
                {
                    if (System.IO.File.Exists(file.UploadedFileName))
                        returnFiles.Add(file);
                }

                return ResponseOK(returnFiles);
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }

        [HttpGet("downloadfile/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            try
            {
                var repo = new MongoGenericRepository<AuditFiles>(_dbsetting);

                var downloadFile = repo.GetByID(id);

                if (!_IDocumentUpload.IsExists(downloadFile.UploadedFileName))
                    return ResponseOK("File does not exists.");

                var memory = new MemoryStream();
                using (var stream = new FileStream(downloadFile.UploadedFileName, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, UtilityMethods.GetContentType(downloadFile.UploadedFileName), downloadFile.OriginalFileName);
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }
        [HttpPost("sendemail")]
        public IActionResult SendEmail(sendmail param)
        {
            var emailModel = new EmailModel();
            try
            {
                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var userModel = userRepo.GetFirst(p => p.EmailId == param.Email);
                var username = userModel == null ? "" : userModel.FirstName + " " + userModel.LastName;

                //var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                //var companyModel = companyRepo.GetByID(auditModel.Location.CompanyID);

                //if (companyModel == null)
                //    return ResponseNotFound();

                var webRootPath = _IWebHostEnvironment.WebRootPath;
                var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "OverallAssesment.html");

                var emailBody = new StringBuilder();
                using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                {
                    var htmlContent = streamReader.ReadToEnd();
                    emailBody.Append(htmlContent);
                }

                //var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                //var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                //var auditPeriod = auditStartDate + " to " + auditEndDate;
                var returnBuilder = new StringBuilder();
                returnBuilder.Append("<table cellpadding='5' cellspacing='0'  border='1' style='width: 100%; font-size: 12px; font-family: Arial'>");
                returnBuilder.Append("<tr><td>Audit Name</td><td>Location</td><td>Sector</td><td>Country</td></tr>");
                foreach (var item in param.Id)
                {
                    var tList = _api.GetFirst(x => x.Id == item);

                    //var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                    //var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == item).FirstOrDefault();
                    //var location = _service.getLocation(tList.ProcessLocationMappings[0].Locations.ToList());
                    if (tList.Location != null && tList.Location.Id != null)
                        tList.Location = _CommonServices.GetLocationDetail(tList.Location.Id);
                    returnBuilder.Append("<tr>");
                    returnBuilder.Append("<td>" + (tList.ProcessLocationMapping == null ? "" : tList.ProcessLocationMapping.AuditName) + "</td>");
                    returnBuilder.Append("<td>" + tList.Location.ProfitCenterCode + "</td>");
                    returnBuilder.Append("<td>" + tList.Location.DivisionDescription + "</td>");
                    returnBuilder.Append("<td>" + tList.Location.Country.Name + "</td>");
                    
                    returnBuilder.Append("</tr>");
                }
                returnBuilder.Append("</table>");
                emailBody = emailBody
                    .Replace("#ResponsibleName#", userModel.FirstName + " " + userModel.LastName)
                    .Replace("#table#", returnBuilder.ToString());
                emailModel.ToEmail = new List<string>() { param.Email };
                emailModel.Subject = /*companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Activity"*/"Audit Plan";
                emailModel.MailBody = emailBody.ToString();
                _IEmailUtility.SendEmail(emailModel);

                return ResponseOK(new { sent = true });
            }
            catch (Exception ex)
            {
                return ResponseError("Internal server error.");
            }
        }
    }
}