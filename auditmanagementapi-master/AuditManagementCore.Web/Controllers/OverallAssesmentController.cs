using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aspose.Pdf;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Wkhtmltopdf.NetCore;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OverallAssesmentController : VJBaseGenericAPIController<OverallAssesment>
    {
        IMongoDbSettings _dbsetting;
        IWebHostEnvironment _IWebHostEnvironment;
        CommonServices _CommonServices;
        IEmailUtility _IEmailUtility;
        IDocumentUpload _IDocumentUpload;

        readonly IGeneratePdf _generatePdf;

        public OverallAssesmentController(IMongoGenericRepository<OverallAssesment> api, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, IEmailUtility emailUtility,
            IDocumentUpload documentUpload, IGeneratePdf generatePdf, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _IWebHostEnvironment = webHostEnvironment;
            _IEmailUtility = emailUtility;
            _IDocumentUpload = documentUpload;
            _generatePdf = generatePdf;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] OverallAssesment e)
        {
            if (e == null)
                return ResponseBad("Audit obj is null");

            //var processId = "";
            //processId = !string.IsNullOrEmpty(e.ProcessL2Id) ? e.ProcessL2Id : "";
            //processId = processId == "" && !string.IsNullOrEmpty(e.ProcessL1Id) ? e.ProcessL1Id : "";
            //processId = processId == "" && !string.IsNullOrEmpty(e.BusinessCycleId) ? e.BusinessCycleId : "";

            if (string.IsNullOrWhiteSpace(e.ProcessLocationMappingID))
                return ResponseBad("Audit Name is null or blank");
            try
            {
                var auditObj = _CommonServices.GetOverAllAssesmentPlanByProcess(e.ProcessLocationMappingID, e.BusinessCycleId, e.ProcessL1Id, e.ProcessL2Id, e.CreatedBy);
                if (auditObj.ProcessLocationMappings.Count > 0)
                {
                    var auditName = auditObj.ProcessLocationMappings[0].AuditName != null ? auditObj.ProcessLocationMappings[0].AuditName : "";
                    //Activity Log
                    _CommonServices.ActivityLog(auditObj.CreatedBy, auditObj.Id, "OverallAssesment(" + auditName + ")", "OverallAssesment", "Audit Planning Engine | OverallAssesment | Add", "Added OverallAssesment");
                }
                return Ok(auditObj);
            }
            catch (Exception ex)
            {
                return ResponseError(ex.Message);
            }
        }

        public override ActionResult Put([FromBody] OverallAssesment a)
        {

            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            var prID = a.Id;


            var ERMRisks = new MongoGenericRepository<ERMRisks>(_dbsetting);
            var BIrepo = new MongoGenericRepository<KeyBusinessInitiative>(_dbsetting);


            var auditObj = _api.GetByID(a.Id);

            if (auditObj != null)
            {

                auditObj.ERMRisks = ERMRisks.GetFirst(x => x.ProcessLocationMappingID == auditObj.ProcessLocationMappingID);
                auditObj.KeyBusinessInitiative = BIrepo.GetFirst(x => x.ProcessLocationMappingID == auditObj.ProcessLocationMappingID);

                auditObj.Coverage = a.Coverage;
                auditObj.Justification = a.Justification;
                auditObj.Status = a.Status;
                auditObj.Lastaudityear = a.Lastaudityear;
                if (auditObj.ProcessLocationMappings.Count > 0)
                {
                    var plRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                    var plmap = new List<ProcessLocationMapping>();
                    foreach (ProcessLocationMapping plm in auditObj.ProcessLocationMappings)
                    {
                        var newPl = plRepo.GetFirst(x => x.Id == plm.Id);
                        plmap.Add(newPl);
                    }
                    auditObj.ProcessLocationMappings = plmap;
                }
                _api.Update(auditObj);
                //Activity Log
                var auditName = auditObj.ProcessLocationMappings[0].AuditName != null ? auditObj.ProcessLocationMappings[0].AuditName : "";
                _CommonServices.ActivityLog(auditObj.UpdatedBy, auditObj.Id, "OverallAssesment(" + auditName + ")", "OverallAssesment", "Audit Planning Engine | OverallAssesment | Edit", "Updated OverallAssesment");
                return Ok(auditObj);
            }
            else
            {
                return NotFound();
            }
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, "", "OverallAssesment", "Audit Planning Engine | OverallAssesment | Delete", "Deleted OverallAssesment");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<BusinessCycle, ProcessL1, ProcessL2>();

            if (tList == null)
                return ResponseNotFound();

            var repoPLMAP = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var repoAudit = new MongoGenericRepository<Audit>(_dbsetting);

            foreach (var item in tList)
            {
                var plmap = repoPLMAP.GetByID(item.ProcessLocationMappingID);

                if (plmap != null)
                {
                    item.ProcessLocationMappings[0] = plmap;
                }
                var IsExistOverallAssementAudit = repoAudit.GetFirst(p => p.OverallAssesmentId == item.Id);
                if (IsExistOverallAssementAudit != null)
                    item.isOverallAssesmentWiseAudit = true;
            }

            return ResponseOK(tList);
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var tList = _api.GetAll();

            if (tList == null)
                return ResponseNotFound();

            var ProcessLocationMapping = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var ERMRisks = new MongoGenericRepository<ERMRisks>(_dbsetting);
            var KeyBI = new MongoGenericRepository<KeyBusinessInitiative>(_dbsetting);
            var _locationRepo = new MongoGenericRepository<Location>(_dbsetting);

            var fileName = "OverAllAssesment.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].Value = "Audit Name";
                worksheet.Cells["B1"].Value = "Location";
                worksheet.Cells["C1"].Value = "Overall Risk";
                worksheet.Cells["D1"].Value = "ERM Flag";
                worksheet.Cells["E1"].Value = "BI Flag";
                worksheet.Cells["F1"].Value = "Coverage";
                worksheet.Cells["G1"].Value = "Justification";
                worksheet.Cells["H1"].Value = "Last Audited";

                var rowIndex = 2;
                foreach (var assesment in tList)
                {
                    var ermRisk = ERMRisks.GetFirst(x => x.ProcessLocationMappingID == assesment.ProcessLocationMappingID);
                    var isErmRisk = ermRisk != null ? "Yes" : "No";

                    var keyBusiness = KeyBI.GetFirst(x => x.ProcessLocationMappingID == assesment.ProcessLocationMappingID);
                    var isKeyBusi = keyBusiness != null ? "Yes" : "No";

                    var locations = new List<string>();

                    if (assesment.ProcessLocationMappings[0].Locations != null)
                    {
                        var locationArray = assesment.ProcessLocationMappings[0].Locations;

                        foreach (var l in assesment.ProcessLocationMappings[0].Locations)
                        {
                            var location = _locationRepo.GetFirst(a => a.Id == l);
                            if (location != null)
                                locations.Add(location.ProfitCenterCode);
                        }
                    }
                    worksheet.Cells["A" + rowIndex.ToString()].Value = ProcessLocationMapping.GetWithInclude<ProcessLocationMapping>(x => x.Id == assesment.ProcessLocationMappingID).FirstOrDefault().AuditName;
                    worksheet.Cells["B" + rowIndex.ToString()].Value = string.Join(", ", locations);
                    worksheet.Cells["C" + rowIndex.ToString()].Value = assesment.ProcessRiskMapping.FinalProcessrating;
                    worksheet.Cells["D" + rowIndex.ToString()].Value = isErmRisk;
                    worksheet.Cells["E" + rowIndex.ToString()].Value = isKeyBusi;
                    worksheet.Cells["F" + rowIndex.ToString()].Value = assesment.Coverage ? "Yes" : "No";
                    worksheet.Cells["G" + rowIndex.ToString()].Value = assesment.Justification;
                    worksheet.Cells["H" + rowIndex.ToString()].Value = assesment.Lastaudityear;
                    rowIndex++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("updateermflag")]
        public ActionResult UpdateErmFlag([FromBody] OverallAssesment overallAssesment)
        {
            if (overallAssesment == null)
                return ResponseBad("Audit obj is null");

            if (string.IsNullOrWhiteSpace(overallAssesment.ProcessLocationMappingID))
                return ResponseBad("Audit Name is null or blank");

            var exists = _api.GetFirst(a => a.ProcessLocationMappingID == overallAssesment.ProcessLocationMappingID);
            if (exists != null)
            {
                exists.isERMRisks = overallAssesment.isERMRisks;
                _api.Update(exists);
            }

            return ResponseOK(exists);
        }

        [HttpPost("updatekeybusinessflag")]
        public ActionResult UpdateKeyBusinessFlag([FromBody] OverallAssesment overallAssesment)
        {
            if (overallAssesment == null)
                return ResponseBad("Audit obj is null");

            if (string.IsNullOrWhiteSpace(overallAssesment.ProcessLocationMappingID))
                return ResponseBad("Audit Name is null or blank");

            var exists = _api.GetFirst(a => a.ProcessLocationMappingID == overallAssesment.ProcessLocationMappingID);
            if (exists != null)
            {
                exists.isKeyBusiness = overallAssesment.isKeyBusiness;
                _api.Update(exists);
            }

            return ResponseOK(exists);
        }

        [HttpGet("downloadpdf")]
        public IActionResult DownloadPDF()
        {
            var tList = _api.GetAll();
            if (tList == null)
                return ResponseNotFound();

            var ProcessLocationMapping = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var ERMRisks = new MongoGenericRepository<ERMRisks>(_dbsetting);
            var KeyBI = new MongoGenericRepository<KeyBusinessInitiative>(_dbsetting);
            var _locationRepo = new MongoGenericRepository<Location>(_dbsetting);

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "OverallAssesmentPresentation.html");

            var emailBody = new StringBuilder();

            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }

            var returnBuilder = new StringBuilder();
            returnBuilder.Append(emailBody);
            returnBuilder.Append("<table cellpadding='5' cellspacing='0'  border='1'  style='width: 100%; font-size: 12px; font-family: Arial'>");
            returnBuilder.Append("<tr><td>Audit Name</td><td>Location</td><td>Overall Risk</td><td>ERM Flag</td><td>BI Flag</td><td>Coverage</td><td>Justification</td><td>Last Audited</td></tr>");
            foreach (var assesment in tList)
            {
                var ermRisk = ERMRisks.GetFirst(x => x.ProcessLocationMappingID == assesment.ProcessLocationMappingID);
                var isErmRisk = ermRisk != null ? "Yes" : "No";

                var keyBusiness = KeyBI.GetFirst(x => x.ProcessLocationMappingID == assesment.ProcessLocationMappingID);
                var isKeyBusi = keyBusiness != null ? "Yes" : "No";

                var locations = new List<string>();

                if (assesment.ProcessLocationMappings[0].Locations != null)
                {
                    var locationArray = assesment.ProcessLocationMappings[0].Locations;
                    var auditName = ProcessLocationMapping.GetWithInclude<ProcessLocationMapping>(x => x.Id == assesment.ProcessLocationMappingID).FirstOrDefault().AuditName;
                    foreach (var l in assesment.ProcessLocationMappings[0].Locations)
                    {
                        var location = _locationRepo.GetFirst(a => a.Id == l);
                        if (location != null)
                        {
                            returnBuilder.Append("<tr>");
                            returnBuilder.Append("<td>" + auditName + "</td>");
                            returnBuilder.Append("<td>" + location.ProfitCenterCode + "</td>");
                            returnBuilder.Append("<td>" + assesment.ProcessRiskMapping.FinalProcessrating + "</td>");
                            returnBuilder.Append("<td>" + isErmRisk + "</td>");
                            returnBuilder.Append("<td>" + isKeyBusi + "</td>");
                            returnBuilder.Append("<td>" + (assesment.Coverage ? "Yes" : "No") + "</td>");
                            returnBuilder.Append("<td>" + assesment.Justification + "</td>");
                            returnBuilder.Append("<td>" + assesment.Lastaudityear + "</td>");
                            returnBuilder.Append("</tr>");
                        }
                    }
                }
            }
            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q", returnBuilder.ToString());

            var memoryStream = new MemoryStream(pdfbyte);
            memoryStream.Position = 0;
            return File(memoryStream, VJLiabraries.UtilityMethods.GetContentType(".pdf"), "OverallAssesment.pdf");
        }

        [HttpGet("downloadppt")]
        public IActionResult DownloadPPT()
        {
            var tList = _api.GetAll();
            if (tList == null)
                return ResponseNotFound();

            var ProcessLocationMapping = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var ERMRisks = new MongoGenericRepository<ERMRisks>(_dbsetting);
            var KeyBI = new MongoGenericRepository<KeyBusinessInitiative>(_dbsetting);
            var _locationRepo = new MongoGenericRepository<Location>(_dbsetting);

            var webRootPath = _IWebHostEnvironment.WebRootPath;
            var htmlTemplatePath = Path.Combine(webRootPath, "ExportTemplates", "OverallAssesmentPresentation.html");

            var emailBody = new StringBuilder();

            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
            {
                var htmlContent = streamReader.ReadToEnd();
                emailBody.Append(htmlContent);
            }

            var returnBuilder = new StringBuilder();
            returnBuilder.Append(emailBody);
            returnBuilder.Append("<center><h2>CFPL – Audit Planning – Overall Assessment</h2></center>");

            returnBuilder.Append("<table cellpadding='5' cellspacing='0'  border='1' style='width: 100%; font-size: 12px; font-family: Arial'>");
            returnBuilder.Append("<tr><td>Audit Name</td><td>Location</td><td>Overall Risk</td><td>ERM &nbsp;Flag</td><td>BI Flag</td><td>Coverage</td><td>Justification</td><td>Last Audited</td></tr>");
            foreach (var assesment in tList)
            {
                var ermRisk = ERMRisks.GetFirst(x => x.ProcessLocationMappingID == assesment.ProcessLocationMappingID);
                var isErmRisk = ermRisk != null ? "Yes" : "No";

                var keyBusiness = KeyBI.GetFirst(x => x.ProcessLocationMappingID == assesment.ProcessLocationMappingID);
                var isKeyBusi = keyBusiness != null ? "Yes" : "No";

                var locations = new List<string>();
                var auditName = ProcessLocationMapping.GetWithInclude<ProcessLocationMapping>(x => x.Id == assesment.ProcessLocationMappingID).FirstOrDefault().AuditName;

                if (assesment.ProcessLocationMappings[0].Locations != null)
                {
                    var locationArray = assesment.ProcessLocationMappings[0].Locations;
                    foreach (var l in assesment.ProcessLocationMappings[0].Locations)
                    {
                        var location = _locationRepo.GetFirst(a => a.Id == l);
                        if (location != null)
                        {
                            returnBuilder.Append("<tr>");
                            returnBuilder.Append("<td>" + auditName + "</td>");
                            returnBuilder.Append("<td>" + location.ProfitCenterCode + "</td>");
                            returnBuilder.Append("<td>" + assesment.ProcessRiskMapping.FinalProcessrating + "</td>");
                            returnBuilder.Append("<td>" + isErmRisk + "</td>");
                            returnBuilder.Append("<td>" + isKeyBusi + "</td>");
                            returnBuilder.Append("<td>" + (assesment.Coverage ? "Yes" : "No") + "</td>");
                            returnBuilder.Append("<td>" + assesment.Justification + "</td>");
                            returnBuilder.Append("<td>" + assesment.Lastaudityear + "</td>");
                            returnBuilder.Append("</tr>");
                        }
                    }
                }
            }

            string wkhtmlexepath = System.IO.Directory.GetCurrentDirectory() + "\\wkhtmltopdf";
            byte[] pdfbyte = VJLiabraries.UtilityMethods.ConvertHtmlToPDFByWkhtml(wkhtmlexepath, "-q -O landscape ", returnBuilder.ToString());

            var memoryStream = new MemoryStream();
            memoryStream.Position = 0;
            var folderName = Path.Combine(wkhtmlexepath);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var myUniqueFileName = string.Format(@"{0}", Guid.NewGuid());
            var fullPath = Path.Combine(pathToSave, "OverallAssesment" + myUniqueFileName + ".pdf");
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                stream.Write(pdfbyte, 0, pdfbyte.Length);
            }

            Document pdfDocument = new Document(fullPath);
            PptxSaveOptions pptxOptions = new PptxSaveOptions();
            pdfDocument.Save(memoryStream, pptxOptions.SaveFormat);
            byte[] pptBytes = memoryStream.ToArray();
            return File(pptBytes, "application/octet-stream", "OverallAssesment.pptx");
        }

        [HttpPost("sendemail")]
        public IActionResult SendEmail(sendmail param)
        {
            var emailModel = new EmailModel();
            try
            {
                _CommonServices.ActivityLog("startEmail", "", "", "", "", "");
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
                _CommonServices.ActivityLog("startreturnBuilder", "", "", "", "", "");

                //var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                //var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                //var auditPeriod = auditStartDate + " to " + auditEndDate;
                var returnBuilder = new StringBuilder();
                returnBuilder.Append("<table cellpadding='5' cellspacing='0'  border='1' style='width: 100%; font-size: 12px; font-family: Arial'>");
                returnBuilder.Append("<tr><td>Audit Name</td><td>Location</td><td>Overall Risk</td><td>ERM Risk</td><td>Business Initiative</td><td>Coverage</td><td>Justification</td><td>Last Audited</td></tr>");
                foreach (var item in param.Id)
                {
                    var tList = _api.GetFirst(x => x.Id == item);

                    //var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                    //var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == item).FirstOrDefault();
                    var location = _CommonServices.getLocation(tList.ProcessLocationMappings[0].Locations.ToList());

                    returnBuilder.Append("<tr>");
                    returnBuilder.Append("<td>" + (tList.ProcessLocationMappings == null ? "" : tList.ProcessLocationMappings[0].AuditName) + "</td>");
                    returnBuilder.Append("<td>" + location + "</td>");
                    returnBuilder.Append("<td>" + (tList.ProcessRiskMapping == null ? "" : tList.ProcessRiskMapping.FinalProcessrating) + "</td>");
                    returnBuilder.Append("<td>" + tList.isERMRisks + "</td>");
                    returnBuilder.Append("<td>" + tList.isKeyBusiness + "</td>");
                    returnBuilder.Append("<td>" + tList.Coverage + "</td>");
                    returnBuilder.Append("<td>" + tList.Justification + "</td>");
                    returnBuilder.Append("<td>" + tList.Lastaudityear + "</td>");
                    returnBuilder.Append("</tr>");
                }
                returnBuilder.Append("</table>");
                emailBody = emailBody
                    .Replace("#ResponsibleName#", userModel.FirstName + " " + userModel.LastName)
                    .Replace("#table#", returnBuilder.ToString());
                emailModel.ToEmail = new List<string>() { param.Email };
                emailModel.Subject = /*companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Activity"*/"Overall Assesment";
                emailModel.MailBody = emailBody.ToString();
                _IEmailUtility.SendEmail(emailModel);

                return ResponseOK(new { sent = true });
            }
            catch (Exception ex)
            {
                _CommonServices.ActivityLog(ex.Message, ex.InnerException.ToString(), ex.Source, ex.HResult.ToString(), ex.HelpLink, ex.StackTrace);
                return ResponseError("Internal server error.");
            }
        }
    }
}