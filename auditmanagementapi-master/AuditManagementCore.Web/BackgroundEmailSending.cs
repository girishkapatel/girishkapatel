using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AuditManagementCore.Models.AuditConstants;

namespace AuditManagementCore.Web
{
    public class BackgroundEmailSending : IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundEmailSending> logger;
        public int number = 0;
        private Timer timer;
        IMongoDbSettings _dbsetting;
        IEmailUtility _IEmailUtility;
        IWebHostEnvironment _IWebHostEnvironment;
        public string email = "baldev@silverwebbuzz.com";
        IGlobalConfiguration _globalConfig;
        CommonServices _CommonServices;
        public string serviceName = "Escalation Service";

        public BackgroundEmailSending(ILogger<BackgroundEmailSending> logger, IMongoDbSettings mongoDbSettings, IWebHostEnvironment webHostEnvironment, IEmailUtility emailUtility, IGlobalConfiguration config, CommonServices cs)
        {
            this.logger = logger;
            _dbsetting = mongoDbSettings;
            _IEmailUtility = emailUtility;
            _IWebHostEnvironment = webHostEnvironment;
            _globalConfig = config;
            _CommonServices = cs;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //timer = new Timer(o =>
            //{
            //    Interlocked.Increment(ref number);

            //}, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            Task.Run(TaskRoutine, cancellationToken);
            return Task.CompletedTask;
        }
        public Task TaskRoutine()
        {

            while (true)
            {
                var _repoServiceConfiguration = new MongoGenericRepository<ServiceConfiguration>(_dbsetting);
                var lstService = _repoServiceConfiguration.GetAll();
                if (lstService != null)
                {
                    if (lstService.Count() > 0)
                    {
                        foreach (var item in lstService)
                        {
                            //var setHour = _globalConfig.EmployeeAPISetHours;
                            //Wait  8 am  daily service till next execution
                            TimeSpan first = TimeSpan.Parse(item.SetHours);
                            TimeSpan second = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));

                            if (first.CompareTo(second) == 0)
                            {
                                //Datatracker();
                                SendMail();
                                //DateTime nextStop = DateTime.Now.AddHours(24);
                                //var timeToWait = nextStop - DateTime.Now;
                                //var millisToWait = timeToWait.TotalMilliseconds;

                                //var beforeDate = DateTime.Now.AddDays(-1);
                                //Thread.Sleep((int)millisToWait);
                            }
                        }
                    }
                }
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Sending mail");
            return Task.CompletedTask;
        }
        public void Datatracker()
        {
            var _repoDataTracker = new MongoGenericRepository<InitialDataRequest>(_dbsetting);

            var tList = _repoDataTracker.GetWithInclude<Audit, User>(p => p.Status != "Received" && p.DataReceivedDate != null);

            foreach (var itemdataTracker in tList)
            {
                DateTime stopDate = Convert.ToDateTime(itemdataTracker.DataReceivedDate);

                if (DateTime.Now.Date >= stopDate)
                {
                    Int32 OverdueDay = Convert.ToInt32(itemdataTracker.OverdueInDays) + 1;
                    itemdataTracker.OverdueInDays = OverdueDay.ToString();
                    _repoDataTracker.Update(itemdataTracker);
                }
            }
        }
        public void SendMail()
        {
            try
            {
                _CommonServices.ServiceLog(serviceName, "Start", "", "", "SendMail()");

                var escalationRepo = new MongoGenericRepository<Escalation>(_dbsetting);
                var tList = escalationRepo.GetAll();

                foreach (var Itemescalation in tList)
                {
                    if (Itemescalation.Module == "Activity1")
                    {
                        try
                        {
                            if (Itemescalation.EscalationRules != null)
                            {
                                foreach (var item in Itemescalation.EscalationRules)
                                {
                                    var activityRepo = new MongoGenericRepository<Activity>(_dbsetting);
                                    var lstAct = activityRepo.GetWithInclude<Audit>(p => p.ActivityStatus != "completed");
                                    IQueryable<Activity> lstactivity = FetchAllRequiredData(lstAct);

                                    if (item.Type.ToLower() == ServiceConstant.REMINDER)
                                    {
                                        lstactivity = lstactivity.Where(p => p.PlannedStartDate <= DateTime.Now);
                                    }
                                    else
                                    {
                                        lstactivity = lstactivity.Where(p => p.PlannedEndDate >= DateTime.Now);
                                    }

                                    foreach (var itemActivity in lstactivity)
                                    {
                                        TempClass objDate = new TempClass();
                                        objDate.actualDate = new DateTime();
                                        DateTime stopDate = itemActivity.PlannedEndDate;

                                        objDate.actualDate = setActualDateLogic(item, stopDate);

                                        if (objDate.actualDate.Date == DateTime.Now.Date)
                                        {
                                            SendActivity(itemActivity);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            SendExcepToDB(e, "Activity");
                        }
                    }
                    else if (Itemescalation.Module == "Testing Of Controls1")
                    {
                        try
                        {
                            if (Itemescalation.EscalationRules != null)
                            {
                                foreach (var item in Itemescalation.EscalationRules)
                                {
                                    var procedureDetailRepo = new MongoGenericRepository<RACMAuditProcedureDetails>(_dbsetting);
                                    var lstProcedureDetails = procedureDetailRepo.GetWithInclude<User>(p => p.Status != "completed");
                                    //IQueryable<RACMAuditProcedureDetails> lstactivity = FetchAllRequiredData(lstAct);

                                    if (item.Type.ToLower() == ServiceConstant.REMINDER)
                                    {
                                        lstProcedureDetails = lstProcedureDetails.Where(p => Convert.ToDateTime(p.ProcedureStartDate) <= DateTime.Now);
                                    }
                                    else
                                    {
                                        lstProcedureDetails = lstProcedureDetails.Where(p => Convert.ToDateTime(p.ProcedureStartDate) >= DateTime.Now);
                                    }

                                    foreach (var itemlstProcedureDetails in lstProcedureDetails)
                                    {
                                        TempClass objDate = new TempClass();
                                        objDate.actualDate = new DateTime();
                                        DateTime stopDate = Convert.ToDateTime(itemlstProcedureDetails.ProcedureEndDate);
                                        objDate.actualDate = setActualDateLogic(item, stopDate);

                                        if (objDate.actualDate.Date == DateTime.Now.Date)
                                        {
                                            SendTestingofControl(itemlstProcedureDetails);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            SendExcepToDB(e, "Testing Of Controls");
                            throw;
                        }
                    }
                    else if (Itemescalation.Module == "Data Tracker1")
                    {
                        try
                        {
                            if (Itemescalation.EscalationRules != null)
                            {
                                foreach (var item in Itemescalation.EscalationRules)
                                {
                                    var RepoInitialDataRequest = new MongoGenericRepository<InitialDataRequest>(_dbsetting);
                                    var lstInitialDataRequest = RepoInitialDataRequest.GetWithInclude<Audit, User>(p => p.Status != "Received");
                                    IQueryable<InitialDataRequest> lstdataTracker = FetchAllRequiredData(lstInitialDataRequest);

                                    if (item.Type.ToLower() == ServiceConstant.REMINDER)
                                    {
                                        lstdataTracker = lstdataTracker.Where(p => Convert.ToDateTime(p.DataRequestDate) <= DateTime.Now);
                                    }
                                    else
                                    {
                                        lstdataTracker = lstdataTracker.Where(p => Convert.ToDateTime(p.DataRequestDate) >= DateTime.Now);
                                    }

                                    foreach (var itemdataTracker in lstdataTracker)
                                    {
                                        TempClass objDate = new TempClass();
                                        objDate.actualDate = new DateTime();
                                        DateTime stopDate = Convert.ToDateTime(itemdataTracker.DataRequestDate);
                                        objDate.actualDate = setActualDateLogic(item, stopDate);

                                        if (objDate.actualDate.Date == DateTime.Now.Date)
                                        {
                                            SenddataTracker(itemdataTracker);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            SendExcepToDB(e, "Data Tracker");
                        }
                    }
                    else if (Itemescalation.Module == "Draft Report1")
                    {
                        try
                        {
                            if (Itemescalation.EscalationRules != null)
                            {
                                foreach (var item in Itemescalation.EscalationRules)
                                {
                                    var RepoDraftReport = new MongoGenericRepository<DraftReport>(_dbsetting);
                                    var lstDR = RepoDraftReport.GetWithInclude<User, DiscussionNote, Audit>(p => p.Status != "COMPLETED");
                                    IQueryable<DraftReport> lstDraftReport = FetchAllRequiredData(lstDR, item);
                                    foreach (var itemdataTracker in lstDraftReport)
                                    {
                                        TempClass objDate = new TempClass();
                                        objDate.actualDate = new DateTime();
                                        DateTime stopDate = Convert.ToDateTime(itemdataTracker.ImplementationEndDate);
                                        objDate.actualDate = setActualDateLogic(item, stopDate);

                                        if (objDate.actualDate.Date == DateTime.Now.Date)
                                        {
                                            SendDraftReport(itemdataTracker);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            SendExcepToDB(e, "Draft Report");
                        }
                    }
                    else if (Itemescalation.Module == "Follow Up")
                    {
                        try
                        {
                            if (Itemescalation.EscalationRules != null)
                            {
                                foreach (var item in Itemescalation.EscalationRules)
                                {
                                    var RepoFollowUp = new MongoGenericRepository<FollowUp>(_dbsetting);
                                    var objFollowup = RepoFollowUp.GetWithInclude<DraftReport>(p => p.Status != "completed");//&& p.AuditId != null
                                    IQueryable<FollowUp> lstFollowup = FetchAllRequiredData(objFollowup);

                                    if (item.Type.ToLower() == ServiceConstant.REMINDER)
                                    {
                                        lstFollowup = lstFollowup.Where(p => new DateTime(Math.Max(Convert.ToDateTime(p.ImplementationEndDate).Ticks, Convert.ToDateTime(p.RevisedDate).Ticks)) <= DateTime.Now);
                                    }
                                    else
                                    {
                                        lstFollowup = lstFollowup.Where(p => new DateTime(Math.Max(Convert.ToDateTime(p.ImplementationEndDate).Ticks, Convert.ToDateTime(p.RevisedDate).Ticks)) >= DateTime.Now);
                                    }

                                    foreach (var itemfollowup in lstFollowup)
                                    {
                                        TempClass objDate = new TempClass();
                                        objDate.actualDate = new DateTime();
                                        DateTime stopDate = new DateTime(Math.Max(Convert.ToDateTime(itemfollowup.ImplementationEndDate).Ticks, Convert.ToDateTime(itemfollowup.RevisedDate).Ticks));
                                        objDate.actualDate = setActualDateLogic(item, stopDate);

                                        if (objDate.actualDate.Date == DateTime.Now.Date)
                                        {
                                            SendFollowup(itemfollowup, item);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _CommonServices.ServiceLog(serviceName, "Error", e.Message, e.StackTrace, " Follow Up Module");
                        }
                    }
                    else if (Itemescalation.Module == "TOR1")
                    {
                        try
                        {
                            if (Itemescalation.EscalationRules != null)
                            {
                                foreach (var item in Itemescalation.EscalationRules)
                                {
                                    var TORRepo = new MongoGenericRepository<TOR>(_dbsetting);
                                    var lstTOR = TORRepo.GetAllWithInclude<ScopeAndSchedule, Audit>();

                                    if (item.Type.ToLower() == ServiceConstant.REMINDER)
                                    {
                                        lstTOR = lstTOR.Where(p => Convert.ToDateTime(p.TORIssuedDate) <= DateTime.Now);
                                    }
                                    else
                                    {
                                        lstTOR = lstTOR.Where(p => Convert.ToDateTime(p.TORIssuedDate) >= DateTime.Now);
                                    }

                                    foreach (var itemTOR in lstTOR)
                                    {
                                        TempClass objDate = new TempClass();
                                        objDate.actualDate = new DateTime();
                                        DateTime stopDate = Convert.ToDateTime(itemTOR.TORIssuedDate);
                                        objDate.actualDate = setActualDateLogic(item, stopDate);

                                        if (objDate.actualDate.Date == DateTime.Now.Date)
                                        {
                                            SendTOR(itemTOR);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            SendExcepToDB(e, "TOR");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _CommonServices.ServiceLog(serviceName, "Error", e.Message, e.StackTrace, " SendMail()");
            }
            _CommonServices.ServiceLog(serviceName, "End", "", "", "MigrateEmployee()");
        }
        public DateTime setActualDateLogic(EscalationRules item, DateTime stopDate)
        {
            try
            {
                TempClass objDate = new TempClass();
                if (item.BeforeAfter.ToLower() == "sameday")
                {
                    objDate.actualDate = stopDate;
                }
                else
                {
                    if (Convert.ToInt32(item.Counter) > 1)
                    {
                        objDate.actualDate = stopDate.AddDays(item.BeforeAfter == "Before" ? -(Convert.ToInt32(item.Condition)) : +(Convert.ToInt32(item.Condition)));
                        var interval = item.Interval.Split("Day")[0].Trim();
                        //objDate.actualDate = objDate.actualDate.AddDays(item.BeforeAfter == "Before" ? -(Convert.ToInt32(interval)) : +(Convert.ToInt32(interval)));
                        for (int i = 1; i <= Convert.ToInt32(item.Counter); i++)
                        {
                            if (objDate.actualDate.Date == DateTime.Now.Date)
                            {
                                break;
                            }
                            if (i != Convert.ToInt32(item.Counter))
                                objDate.actualDate = objDate.actualDate.AddDays(Convert.ToInt32(interval));
                        }
                    }
                    else
                    {
                        objDate.actualDate = stopDate.AddDays(item.BeforeAfter == "Before" ? -(item.Condition) : +(item.Condition));
                    }
                }
                return objDate.actualDate;
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "setActualDateLogic");
                return new DateTime();
            }
        }
        private IQueryable<Activity> FetchAllRequiredData(IQueryable<Activity> tList)
        {
            try
            {
                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                foreach (var item in tList)
                {
                    item.ResponsiblePerson = userRepo.GetByID(item.PersonResponsibleID);
                    if (item.Audit != null)
                    {
                        if (item.Audit.ProcessLocationMapping != null)
                        {
                            var objplm = plmRepo.GetFirst(p => p.Id == item.Audit.ProcessLocationMapping.Id);
                            if (objplm != null)
                            {
                                item.Audit.ProcessLocationMapping = objplm;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "FetchAllRequiredData Activity");
            }
            return tList;
        }
        public void SendActivity(Activity itemActivity)
        {
            try
            {
                EmailModel emailModel = new EmailModel();

                if (itemActivity != null)
                {
                    var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                    var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == itemActivity.AuditID).FirstOrDefault();

                    var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                    var companyModel = itemActivity.Audit.Location.CompanyID != null ? companyRepo.GetByID(itemActivity.Audit.Location.CompanyID) : null;

                    emailModel.ToEmail = new List<string> { email }; // itemActivity.ResponsiblePerson.EmailId 

                    var webRootPath = _IWebHostEnvironment.WebRootPath;
                    var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "ActivityRemainderMail.html");

                    var emailBody = new StringBuilder();
                    using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                    {
                        var htmlContent = streamReader.ReadToEnd();
                        emailBody.Append(htmlContent);
                    }
                    var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                    var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                    var auditPeriod = auditStartDate + " to " + auditEndDate;

                    emailBody = emailBody
                        .Replace("#ResponsibleName#", itemActivity.ResponsiblePerson.FirstName + " " + itemActivity.ResponsiblePerson.LastName)
                        .Replace("#auditname#", itemActivity.Audit.ProcessLocationMapping.AuditName)
                        .Replace("#activityname#", itemActivity.ActivityName)
                        .Replace("#targetenddate#", itemActivity.PlannedEndDate.ToShortDateString());

                    emailModel.Subject = companyModel.Name + " | " + itemActivity.Audit.ProcessLocationMapping.AuditName + " | " + auditPeriod + " | " + itemActivity.ActivityName;
                    emailModel.MailBody = emailBody.ToString();

                    _IEmailUtility.SendEmail(emailModel);
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "Activity Send Mail");
            }
        }
        private IQueryable<InitialDataRequest> FetchAllRequiredData(IQueryable<InitialDataRequest> tList)
        {
            try
            {
                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                foreach (var item in tList)
                {
                    item.ProcessOwner = userRepo.GetByID(item.ProcessOwnerId);
                    if (item.Audit != null)
                    {
                        if (item.Audit.ProcessLocationMapping != null)
                        {
                            var objplm = plmRepo.GetFirst(p => p.Id == item.Audit.ProcessLocationMapping.Id);
                            if (objplm != null)
                            {
                                item.Audit.ProcessLocationMapping = objplm;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "FetchAllRequiredData Data Tracker");
            }
            return tList;
        }
        public void SenddataTracker(InitialDataRequest objInitialDataRequest)
        {
            try
            {
                EmailModel emailModel = new EmailModel();

                if (objInitialDataRequest != null)
                {
                    var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                    var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == objInitialDataRequest.AuditId).FirstOrDefault();

                    var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                    var companyModel = objInitialDataRequest.Audit.Location.CompanyID != null ? companyRepo.GetByID(objInitialDataRequest.Audit.Location.CompanyID) : null;

                    emailModel.ToEmail = new List<string> { email }; // itemActivity.ResponsiblePerson.EmailId 

                    var webRootPath = _IWebHostEnvironment.WebRootPath;
                    var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "InitialDataRequest.html");

                    var emailBody = new StringBuilder();
                    using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                    {
                        var htmlContent = streamReader.ReadToEnd();
                        emailBody.Append(htmlContent);
                    }
                    var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                    var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                    var auditPeriod = auditStartDate + " to " + auditEndDate;

                    emailBody = emailBody
                        //.Replace("#ProcessOwnerName#", objInitialDataRequest.ProcessOwner.FirstName + " " + objInitialDataRequest.ProcessOwner.LastName)
                        .Replace("#AuditName#", objInitialDataRequest.Audit.AuditName)
                        .Replace("#AuditPeriod#", auditPeriod);

                    emailModel.Subject = companyModel.Name + " | " + objInitialDataRequest.Audit.ProcessLocationMapping.AuditName + " | " + auditPeriod + " | Initial Data Request";
                    emailModel.MailBody = emailBody.ToString();

                    var _commonServices = new CommonServices(_dbsetting);
                    var file = _commonServices.DownloadExcelAttachmentForDataTracker(objInitialDataRequest);
                    var objAttachment = new AttachmentByte()
                    {
                        FileContents = file.FileContents,
                        FileName = file.FileDownloadName
                    };

                    emailModel.Attachments = new List<AttachmentByte>() { objAttachment };

                    _IEmailUtility.SendEmail(emailModel);
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "Send Mail Data Tracker");
            }
        }
        public void SendTestingofControl(RACMAuditProcedureDetails objTestingOfControl)
        {
            try
            {
                EmailModel emailModel = new EmailModel();

                if (objTestingOfControl != null)
                {
                    var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                    var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == objTestingOfControl.AuditId).FirstOrDefault();

                    var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                    var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                    var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);

                    var objAudit = auditRepo.GetFirst(p => p.Id == objTestingOfControl.AuditId);
                    var companyModel = objAudit != null ? companyRepo.GetByID(objAudit.Location.CompanyID) : new Company();
                    var AuditModel = objAudit != null ? plmRepo.GetByID(objAudit.ProcessLocationMapping.Id) : new ProcessLocationMapping();


                    emailModel.ToEmail = new List<string> { email }; // itemActivity.ResponsiblePerson.EmailId 

                    var webRootPath = _IWebHostEnvironment.WebRootPath;
                    var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "TestingOfControl.html");

                    var emailBody = new StringBuilder();
                    using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                    {
                        var htmlContent = streamReader.ReadToEnd();
                        emailBody.Append(htmlContent);
                    }
                    var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                    var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                    var auditPeriod = auditStartDate + " to " + auditEndDate;

                    emailBody = emailBody
                        .Replace("#AuditName#", AuditModel.AuditName);

                    emailModel.Subject = companyModel.Name + " | " + AuditModel.AuditName + " | " + auditPeriod + " | Audit queries and working";
                    emailModel.MailBody = emailBody.ToString();

                    var _commonServices = new CommonServices(_dbsetting);
                    var file = _commonServices.DownloadExcelAttachmentForAuditProcedure(objTestingOfControl);
                    var objAttachment = new AttachmentByte()
                    {
                        FileContents = file.FileContents,
                        FileName = file.FileDownloadName
                    };

                    emailModel.Attachments = new List<AttachmentByte>() { objAttachment };

                    _IEmailUtility.SendEmail(emailModel);
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "Testing of Control Sending mail");
            }
        }

        public void SendTOR(TOR objTOR)
        {
            try
            {
                EmailModel emailModel = new EmailModel();

                if (objTOR != null)
                {
                    var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                    var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                    var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);



                    objTOR.Audit = scopeAndScheduleRepo.GetFirst(x => x.AuditId == objTOR.AuditId);
                    if (objTOR.Audit != null)
                    {
                        objTOR.Audit = populateScopeAndSchedule(objTOR.Audit);
                    }
                    var companyModel = objTOR.Audit.Location.CompanyID != null ? companyRepo.GetByID(objTOR.Audit.Location.CompanyID) : null;

                    var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                    var auditModel = objTOR.Audit.ProcessLocationMappingId != null ? plmRepo.GetByID(objTOR.Audit.ProcessLocationMappingId) : null;

                    List<string> lstApprovalName = new List<string>();
                    foreach (var item in objTOR.Audit.AuditApprovalMapping.UserData)
                    {
                        lstApprovalName.Add(item.User.FirstName + " " + item.User.LastName);
                    }
                    string approvalName = string.Join("", lstApprovalName);

                    List<string> lstResourceName = new List<string>();
                    foreach (var item in objTOR.Audit.AuditResources)
                    {
                        lstResourceName.Add(item.User.FirstName + " " + item.User.LastName);
                    }
                    string resourceName = string.Join("", lstResourceName);


                    emailModel.ToEmail = new List<string> { email }; // itemActivity.ResponsiblePerson.EmailId 

                    var webRootPath = _IWebHostEnvironment.WebRootPath;
                    var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "TermsOfReference-TOR.html");

                    var emailBody = new StringBuilder();
                    using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                    {
                        var htmlContent = streamReader.ReadToEnd();
                        emailBody.Append(htmlContent);
                    }
                    var auditStartDate = Convert.ToDateTime(objTOR.Audit.AuditStartDate).ToString("dd-MMM-yyyy");
                    var auditEndDate = Convert.ToDateTime(objTOR.Audit.AuditEndDate).ToString("dd-MMM-yyyy");
                    var auditPeriod = auditStartDate + " to " + auditEndDate;

                    emailBody = emailBody
                        .Replace("#Name#", objTOR.AuditObjective)
                        .Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"))
                        .Replace("#ApproverName#", approvalName)
                        .Replace("#ResourcesName#", resourceName)
                        .Replace("#AuditName#", auditModel.AuditName)
                        .Replace("#AuditPeriod#", auditPeriod);

                    emailModel.Subject = companyModel.Name + " | " + auditModel.AuditName + " | " + auditPeriod + " | Scope of audit";
                    emailModel.MailBody = emailBody.ToString();

                    var _commonServices = new CommonServices(_dbsetting);
                    var file = _commonServices.DownloadExcelAttachmentForTOR(objTOR);
                    var objAttachment = new AttachmentByte()
                    {
                        FileContents = file.FileContents,
                        FileName = file.FileDownloadName
                    };

                    _IEmailUtility.SendEmail(emailModel);
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "Send Mail TOR");
            }
        }
        public ScopeAndSchedule populateScopeAndSchedule(ScopeAndSchedule item)
        {
            try
            {
                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var mp = new CommonServices(_dbsetting);

                item.Location = mp.GetLocationDetail(item.LocationId);
                item.Audit = mp.GetAuditDetail(item.AuditId);

                foreach (var res in item.AuditResources)
                {
                    res.User = userRepo.GetByID(res.UserId);
                }

                item.AuditApprovalMapping = mp.AttachAuditApproverMapping(item.AuditId);

                if (item.Auditees != null)
                {
                    foreach (var auditee in item.Auditees)
                    {
                        auditee.User = userRepo.GetByID(auditee.UserId);
                    }
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "populateScopeAndSchedule TOR");
            }
            return item;
        }
        private IQueryable<FollowUp> FetchAllRequiredData(IQueryable<FollowUp> tList)
        {
            try
            {
                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var dnRepo = new MongoGenericRepository<DiscussionNote>(_dbsetting);
                var scopeAndSchedule = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                var riskTypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
                var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
                var repoAuditFiles = new MongoGenericRepository<AuditFiles>(_dbsetting);
                var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);


                foreach (var item in tList)
                {
                    if (item.ImplementationOwnerId != null)
                        item.ImplementationOwner = userRepo.GetByID(item.ImplementationOwnerId);
                    if (item.Audit != null)
                        item.Audit = populateScopeAndSchedule(scopeAndSchedule.GetFirst(x => x.AuditId == item.AuditId));

                    if (item.DraftReportId != null)
                        item.DraftReport.DiscussionNote = dnRepo.GetWithInclude<RiskType>(a => a.Id == item.DraftReport.DiscussionNoteID).FirstOrDefault();

                    if (item.RiskTypeId != null)
                        item.RiskType = riskTypeRepo.GetFirst(a => a.Id == item.RiskTypeId);

                    item.ActionPlansInfo = repoFollowupActionPlan.GetMany(a => a.FollowupId == item.Id).ToList();

                    if (item.ProcessLocationMappingId != null)
                    {
                        item.ProcessLocationMapping = plmRepo.GetByID(item.ProcessLocationMappingId);
                    }
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "FetchAllRequiredData FollowUp");
            }
            return tList;
        }
        public void SendFollowup(FollowUp objFollowUp, EscalationRules escRules)
        {
            try
            {
                var emailBody = new StringBuilder();
                EmailModel emailModel = new EmailModel();
                string htmlTemplatePath = "";
                var _repoUser = new MongoGenericRepository<User>(_dbsetting);
                if (objFollowUp != null)
                {
                    if (objFollowUp.ImplementationOwnerId != null)
                    {
                        var webRootPath = _IWebHostEnvironment.WebRootPath;
                        var objUserEmail = _repoUser.GetFirst(p => p.Id == objFollowUp.ImplementationOwnerId);
                        if (escRules.Type.ToLower() == ServiceConstant.REMINDER) htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "Followup_Escalation_Remainder.html");
                        else htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "Followup_Escalation.html");
                        using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                        {
                            var htmlContent = streamReader.ReadToEnd();
                            emailBody.Append(htmlContent);
                        }
                        if (escRules.Type.ToLower() == ServiceConstant.REMINDER)
                            emailBody = emailBody
                             .Replace("#url#", _globalConfig.WebURL)
                             .Replace("#RemainderDays#", escRules.Interval);
                        else
                            emailBody = emailBody
                                 .Replace("#RemainderDays#", escRules.Interval)
                                 .Replace("#Users#", objUserEmail.FirstName + " " + objUserEmail.LastName);

                        if (objFollowUp.AuditExist)
                        {
                            var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                            var scopeModel = scopeRepo.GetFirst(x => x.AuditId == objFollowUp.AuditId);
                            var companyRepo = new MongoGenericRepository<Company>(_dbsetting);

                            var locationRepo = new MongoGenericRepository<Location>(_dbsetting);
                            var objLocation = locationRepo.GetFirst(p => p.Id == scopeModel.LocationId);
                            if (objLocation != null)
                            {
                                scopeModel.Location = objLocation;
                            }
                            var companyModel = scopeModel.Location != null ? companyRepo.GetByID(scopeModel.Location.CompanyID) : new Company();
                            var currentFinancialYear = _CommonServices.GetCurrentFinancialYearByDate(scopeModel.AuditStartDate, scopeModel.AuditEndDate);
                            emailModel.Subject = companyModel.Name + " | " + currentFinancialYear + " | " + scopeModel.Quater + " | Implementation status";
                            emailModel.MailBody = emailBody.ToString();

                        }
                        else
                        {
                            emailModel.Subject = "Action Plan";
                            emailModel.MailBody = emailBody.ToString();
                        }
                        if (objUserEmail != null)
                        {
                            emailModel.ToEmail = new List<string> { objUserEmail.EmailId };
                            if (objUserEmail.ReportToId != null)
                            {
                                var reporterUser = _repoUser.GetFirst(p => p.Id == objUserEmail.ReportToId);
                                if (objUserEmail != null)
                                {
                                    emailModel.CcEmail = new List<string> { reporterUser.EmailId };
                                }
                            }
                        }
                        List<FollowUp> lstFollowup = new List<FollowUp>();
                        lstFollowup.Add(objFollowUp);
                        var file = _CommonServices.DownloadExcelAttachment(lstFollowup);
                        var objAttachment = new AttachmentByte()
                        {
                            FileContents = file.FileContents,
                            FileName = file.FileDownloadName
                        };
                        emailModel.Attachments = new List<AttachmentByte>() { objAttachment };
                        _CommonServices.ServiceLog(serviceName, objUserEmail.EmailId + " mail send successfully", "", "", "SendFollowup()");

                        _IEmailUtility.SendEmail(emailModel);
                    }
                }
            }
            catch (Exception e)
            {
                _CommonServices.ServiceLog(serviceName, "Error", e.Message, e.StackTrace, "SendFollowup()");
            }
        }
        private IQueryable<DraftReport> FetchAllRequiredData(IQueryable<DraftReport> tList, EscalationRules escalationRules)
        {
            try
            {
                var userRepo = new MongoGenericRepository<User>(_dbsetting);
                var plmRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                foreach (var item in tList)
                {
                    if (item.AuditId != null)
                    {
                        if (item.Audit.ProcessLocationMapping != null)
                        {
                            var objplm = plmRepo.GetFirst(p => p.Id == item.Audit.ProcessLocationMapping.Id);
                            if (objplm != null)
                            {
                                item.Audit.ProcessLocationMapping = objplm;
                            }
                        }
                        if (item.ActionPlans != null)
                        {
                            foreach (var actionPlan in item.ActionPlans)
                            {
                                var objProcessOwner = userRepo.GetFirst(p => p.Id == actionPlan.ProcessOwnerID);
                                if (objProcessOwner != null)
                                {
                                    actionPlan.ProcessOwner = objProcessOwner;
                                }
                            }


                        }
                    }
                    //Approval Date from DraftReport History
                    var RepoDraftReportHistory = new MongoGenericRepository<DraftReportHistory>(_dbsetting);
                    var objHistoryDraftReport = RepoDraftReportHistory.GetMany(p => p.Status == "APPROVED" && p.DraftReportID == item.Id).OrderByDescending(p => p.DraftReportDate).FirstOrDefault();
                    if (escalationRules.Type.ToLower() == ServiceConstant.REMINDER)
                    {
                        tList = tList.Where(p => objHistoryDraftReport.DraftReportDate <= DateTime.Now);
                    }
                    else
                    {
                        tList = tList.Where(p => objHistoryDraftReport.DraftReportDate >= DateTime.Now);
                    }

                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "FetchAllRequiredData Draft Report");
            }
            return tList;
        }
        public void SendDraftReport(DraftReport objDraftReport)
        {
            try
            {
                EmailModel emailModel = new EmailModel();

                if (objDraftReport != null)
                {
                    if (objDraftReport.ActionPlans.Count > 0)
                    {
                        foreach (var itemActionPlan in objDraftReport.ActionPlans)
                        {
                            var scopeRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
                            var scopeModel = scopeRepo.GetWithInclude<ScopeAndSchedule>(x => x.AuditId == objDraftReport.AuditId).FirstOrDefault();

                            var companyRepo = new MongoGenericRepository<Company>(_dbsetting);
                            var companyModel = objDraftReport.Audit.Location.CompanyID != null ? companyRepo.GetByID(objDraftReport.Audit.Location.CompanyID) : null;

                            emailModel.ToEmail = new List<string> { email }; // itemActivity.ResponsiblePerson.EmailId 

                            var webRootPath = _IWebHostEnvironment.WebRootPath;
                            var htmlTemplatePath = Path.Combine(webRootPath, "EmailTemplates", "DraftReport.html");

                            var emailBody = new StringBuilder();
                            using (StreamReader streamReader = new StreamReader(htmlTemplatePath))
                            {
                                var htmlContent = streamReader.ReadToEnd();
                                emailBody.Append(htmlContent);
                            }
                            var auditStartDate = Convert.ToDateTime(scopeModel.AuditStartDate).ToString("dd-MMM-yyyy");
                            var auditEndDate = Convert.ToDateTime(scopeModel.AuditEndDate).ToString("dd-MMM-yyyy");
                            var auditPeriod = auditStartDate + " to " + auditEndDate;

                            emailBody = emailBody
                                //.Replace("#ProcessOwnerName#", itemActionPlan.ProcessOwner.FirstName + " " + itemActionPlan.ProcessOwner.LastName)
                                .Replace("#DiscussionDate#", DateTime.Now.ToString("dd-MMM-yyyy"))
                                .Replace("#AuditName#", objDraftReport.Audit.ProcessLocationMapping.AuditName);

                            emailModel.Subject = companyModel.Name + " | " + objDraftReport.Audit.ProcessLocationMapping.AuditName + " | " + auditPeriod + " | DRAFT Report";
                            emailModel.MailBody = emailBody.ToString();

                            var _commonServices = new CommonServices(_dbsetting);
                            var file = _commonServices.DownloadExcelAttachmentForDraftReport(objDraftReport);
                            var objAttachment = new AttachmentByte()
                            {
                                FileContents = file.FileContents,
                                FileName = file.FileDownloadName
                            };

                            emailModel.Attachments = new List<AttachmentByte>() { objAttachment };

                            _IEmailUtility.SendEmail(emailModel);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SendExcepToDB(e, "Send Mail DraftReport");
            }
        }
        public void SendExcepToDB(Exception exdb, string URL)
        {
            var _log = new MongoGenericRepository<ExceptionLog>(_dbsetting);
            var objExceptionLog = new ExceptionLog();
            objExceptionLog.ExceptionMsg = exdb.Message.ToString();
            objExceptionLog.ExceptionType = exdb.GetType().Name.ToString();
            objExceptionLog.ExceptionSource = exdb.StackTrace.ToString();
            objExceptionLog.ExceptionURL = URL;
            _log.Insert(objExceptionLog);
        }
    }
    public class TempClass
    {
        public DateTime actualDate { get; set; }
    }
}
