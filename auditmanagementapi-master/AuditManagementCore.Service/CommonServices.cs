using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using MongoDB.Driver.Core.Operations;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Drawing;
using VJLiabraries;

namespace AuditManagementCore.Service
{
    public class CommonServices
    {
        IMongoDbSettings _dbsetting;

        public CommonServices(IMongoDbSettings dbSettings)
        {
            _dbsetting = dbSettings;
        }

        public Country GetCountryById(string id)
        {
            var conrepo = new MongoGenericRepository<Country>(_dbsetting);
            var country = conrepo.GetByID(id);
            return country;
        }

        public List<Country> GetAllCountries()
        {
            var conrepo = new MongoGenericRepository<Country>(_dbsetting);
            var countries = conrepo.GetAll();
            return countries.ToList();
        }

        public State GetStateById(string id)
        {
            var srepo = new MongoGenericRepository<State>(_dbsetting);
            var state = srepo.GetByID(id);
            state.Country = GetCountryById(state.CountryId);
            return state;
        }

        public List<State> GetAllState()
        {
            var srepo = new MongoGenericRepository<State>(_dbsetting);
            var state = srepo.GetAll();
            var conrepo = new MongoGenericRepository<Country>(_dbsetting);
            foreach (var s in state)
            {
                s.Country = conrepo.GetByID(s.CountryId);
            }
            return state.ToList();
        }

        public CityOrTown GetCityById(string id)
        {
            var cityRepo = new MongoGenericRepository<CityOrTown>(_dbsetting);
            var city = cityRepo.GetByID(id);
            city.State = GetStateById(city.StateId);
            return city;
        }

        public List<CityOrTown> GetAllCities()
        {
            var cityRepo = new MongoGenericRepository<CityOrTown>(_dbsetting);
            var cities = cityRepo.GetAll();
            var stateRepo = new MongoGenericRepository<State>(_dbsetting);
            foreach (var city in cities)
            {
                city.State = GetStateById(city.StateId);
            }
            return cities.ToList();
        }

        public BusinessCycle GetBusinessCycleById(string id)
        {
            var businessCycleRepo = new MongoGenericRepository<BusinessCycle>(_dbsetting);
            var businessCycle = businessCycleRepo.GetByID(id);
            return businessCycle;
        }

        public List<BusinessCycle> GetAllBusinessCycles()
        {
            var businessCycleRepo = new MongoGenericRepository<BusinessCycle>(_dbsetting);
            var businessCycles = businessCycleRepo.GetAll();
            return businessCycles.ToList();
        }

        public ProcessL1 GetProcessL1ById(string id)
        {
            var processL1Repo = new MongoGenericRepository<ProcessL1>(_dbsetting);
            var processL1 = processL1Repo.GetByID(id);
            processL1.BusinessCycle = GetBusinessCycleById(processL1.BusinessCycleId);
            return processL1;
        }

        public List<ProcessL1> GetAllProcessL1()
        {
            var processL1Repo = new MongoGenericRepository<ProcessL1>(_dbsetting);
            var processL1s = processL1Repo.GetAll();
            foreach (var processL1 in processL1s)
            {
                processL1.BusinessCycle = GetBusinessCycleById(processL1.BusinessCycleId);
            }
            return processL1s.ToList();
        }

        public ProcessL2 GetProcessL2ById(string id)
        {
            var processL2Repo = new MongoGenericRepository<ProcessL2>(_dbsetting);
            var processL2 = processL2Repo.GetByID(id);
            processL2.ProcessL1 = GetProcessL1ById(processL2.ProcessL1Id);
            return processL2;
        }

        public List<ProcessL2> GetAllProcessL2()
        {
            var processL2Repo = new MongoGenericRepository<ProcessL2>(_dbsetting);
            var processL2s = processL2Repo.GetAll();
            foreach (var processL2 in processL2s)
            {
                processL2.ProcessL1 = GetProcessL1ById(processL2.ProcessL1Id);
            }
            return processL2s.ToList();
        }

        public Control GetControlById(string id)
        {
            var controlRepo = new MongoGenericRepository<Control>(_dbsetting);
            var control = controlRepo.GetByID(id);
            return control;
        }

        public List<Control> GetAllControls()
        {
            var controlRepo = new MongoGenericRepository<Control>(_dbsetting);
            var controls = controlRepo.GetAll();
            return controls.ToList();
        }

        public Sector GetSectorById(string id)
        {
            var sectorRepo = new MongoGenericRepository<Sector>(_dbsetting);
            var sector = sectorRepo.GetByID(id);
            return sector;
        }

        public List<Sector> GetAllSectors()
        {
            var sectorRepo = new MongoGenericRepository<Sector>(_dbsetting);
            var sectors = sectorRepo.GetAll();
            return sectors.ToList();
        }

        public Risk GetRiskById(string id)
        {
            var riskRepo = new MongoGenericRepository<Risk>(_dbsetting);
            var risk = riskRepo.GetByID(id);
            risk.ProcessL1 = GetProcessL1ById(risk.ProcessL1Id);
            return risk;
        }

        public List<Risk> GetAllRisks()
        {
            var riskRepo = new MongoGenericRepository<Risk>(_dbsetting);
            var risks = riskRepo.GetAll();
            foreach (var risk in risks)
            {
                risk.ProcessL1 = GetProcessL1ById(risk.ProcessL1Id);
            }
            return risks.ToList();
        }
        public EYBenchmark GetEyBenchmarkById(string id)
        {
            var eyBenchmarkRepo = new MongoGenericRepository<EYBenchmark>(_dbsetting);
            var eyBenchmark = eyBenchmarkRepo.GetByID(id);
            eyBenchmark.ProcessL1 = GetProcessL1ById(eyBenchmark.ProcessL1Id);
            return eyBenchmark;
        }

        public List<EYBenchmark> GetAllEyBenchmarks()
        {
            var eyBenchmarkRepo = new MongoGenericRepository<EYBenchmark>(_dbsetting);
            var eyBenchmarks = eyBenchmarkRepo.GetAll();
            foreach (var eyBenchmark in eyBenchmarks)
            {
                eyBenchmark.ProcessL1 = GetProcessL1ById(eyBenchmark.ProcessL1Id);
            }
            return eyBenchmarks.ToList();
        }

        public OverallAssesment GetOverAllAssesmentPlanByProcess(string ProcessLocationMappingId, string businessCycleId, string processL1Id, string processL2Id, string userId)
        {
            try
            {
                OverallAssesment audit;
                var auditRepo = new MongoGenericRepository<OverallAssesment>(_dbsetting);
                var IsExist = auditRepo
                    .GetFirst(x => x.ProcessLocationMappingID == ProcessLocationMappingId);

                if (IsExist == null)
                    audit = new OverallAssesment();
                else
                    audit = IsExist;

                //var bc = new MongoGenericRepository<BusinessCycle>(_dbsetting);
                //audit.BusinessCycle = bc.GetByID(businessCycleId);

                //var prL1 = new MongoGenericRepository<ProcessL1>(_dbsetting);
                //audit.ProcessL1 = prL1.GetByID(processL1Id);

                //var prL2 = new MongoGenericRepository<ProcessL2>(_dbsetting);
                //audit.ProcessL2 = prL2.GetByID(processL2Id);

                audit.Coverage = true;// add or update set to true;
                var prReo = new MongoGenericRepository<ProcessRiskMapping>(_dbsetting);
                //audit.ProcessRiskMapping = prReo.GetFirst(x => x.BusinessCycleID == businessCycleId && x.ProcessL1ID == processL1Id && x.ProcessL2Id == processL2Id);
                audit.ProcessRiskMapping = prReo.GetFirst(x => x.ProcessLocationMappingID == ProcessLocationMappingId);

                var BIrepo = new MongoGenericRepository<KeyBusinessInitiative>(_dbsetting);
                //audit.KeyBusinessInitiative = BIrepo.GetFirst(x => x.ProcessLocationMappingID == ProcessLocationMappingId);
                var exists = BIrepo.GetFirst(x => x.ProcessLocationMappingID == ProcessLocationMappingId);
                if (exists != null)
                    audit.isKeyBusiness = true;
                else
                    audit.isKeyBusiness = false;

                var ERMrepo = new MongoGenericRepository<ERMRisks>(_dbsetting);
                //audit.ERMRisks = ERMrepo.GetFirst(x => x.ProcessLocationMappingID == ProcessLocationMappingId);
                var existsERMRisks = ERMrepo.GetFirst(x => x.ProcessLocationMappingID == ProcessLocationMappingId);
                if (existsERMRisks != null)
                    audit.isERMRisks = true;
                else
                    audit.isERMRisks = false;

                var plRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                audit.ProcessLocationMappings = plRepo.GetMany(x => x.Id == ProcessLocationMappingId).ToList();

                audit.ProcessLocationMappingID = ProcessLocationMappingId;

                if (IsExist != null)
                {
                    audit.UpdatedBy = userId;
                    auditRepo.Update(audit);
                }
                else
                {
                    audit.CreatedBy = userId;
                    auditRepo.Insert(audit);
                }
                return audit;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Audit> CreateUnplannedAudit(Audit audit)
        {
            try
            {
                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var locationRepo = new MongoGenericRepository<Location>(_dbsetting);

                List<Audit> audits = new List<Audit>();

                foreach (var loc in audit.Locations)
                {
                    var isExists = auditRepo.GetFirst(a => a.Id == audit.Id);

                    Audit a = new Audit();

                    if (isExists != null)
                        a = isExists;

                    a.AuditName = audit.AuditName;
                    a.OverallAssesment = null;
                    a.OverallAssesmentId = null;
                    a.ProcessLocationMapping = null;
                    a.Location = locationRepo.GetByID(loc.Id);

                    if (isExists != null)
                    {
                        a.UpdatedBy = audit.CreatedBy;
                        auditRepo.Update(a);
                        CommonServices obj = new CommonServices(_dbsetting);
                        obj.ActivityLog(a.UpdatedBy, a.Id, "", "Audit", "Audit | Edit", "Updated Audit");
                    }
                    else
                    {
                        //string auditNumber = GenerateAuditNumber(a, false);
                        a.CreatedBy = audit.CreatedBy;
                        auditRepo.Insert(a);
                        //Activity Log
                        CommonServices obj = new CommonServices(_dbsetting);
                        obj.ActivityLog(a.CreatedBy, a.Id, "", "Audit", "Audit | Add", "Added Audit");
                    }
                    //InsertScopeAndSchedule(auditNumber);
                    audits.Add(a);
                }

                return audits;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Audit> CreateAuditPlansByOverAllAssesment(OverallAssesment os)
        {
            try
            {
                var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
                var locationRepo = new MongoGenericRepository<Location>(_dbsetting);

                List<Audit> audits = new List<Audit>();

                var isCentralized = (os.ProcessModel.ToLower() == "centralized" || (os.ProcessModel.ToLower() == "decentralized" && !os.isLocationWiseAudit)) ? true : false;

                foreach (var locationMapping in os.ProcessLocationMappings)
                {
                    //Centralized to contain only single audit for multiple location
                    if (isCentralized)
                    {
                        Audit a = new Audit();
                        a.AuditName = locationMapping.AuditName;
                        a.OverallAssesment = os;
                        a.OverallAssesmentId = os.Id;
                        a.ProcessLocationMapping = locationMapping;

                        //Temporary adding one location just cause front end won't fail
                        a.Location = locationRepo.GetByID(locationMapping.Locations);

                        foreach (var loc in locationMapping.Locations)
                        {
                            Location location = locationRepo.GetByID(loc);
                            if (location != null)
                                a.Locations.Add(location);
                        }
                        //string auditNumber = GenerateAuditNumber(a, true);
                        auditRepo.Insert(a);
                        //InsertScopeAndSchedule(auditNumber);
                        audits.Add(a);
                    }
                    //De-Centralized to contain multiple audit for multiple location ( 1:1 mapping )
                    else
                    {
                        foreach (var loc in locationMapping.Locations)
                        {
                            Audit a = new Audit();
                            a.AuditName = locationMapping.AuditName;
                            a.OverallAssesment = os;
                            a.OverallAssesmentId = os.Id;
                            a.ProcessLocationMapping = locationMapping;
                            a.Location = locationRepo.GetByID(loc);
                            //string auditNumber = GenerateAuditNumber(a, false);
                            auditRepo.Insert(a);
                            //InsertScopeAndSchedule(auditNumber);
                            audits.Add(a);
                        }
                    }
                }
                return audits;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void InsertScopeAndSchedule(string auditNo)
        {
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            while (true)
            {
                var dummyAuditNumber = auditNo + "/" + GenerateRandomNo();
                ScopeAndSchedule ss = scopeAndScheduleRepo.GetFirst(x => x.AuditNumber == dummyAuditNumber);
                if (ss == null)
                {
                    auditNo = dummyAuditNumber;
                    break;
                }
            }

            ScopeAndSchedule newSS = new ScopeAndSchedule();
            newSS.AuditNumber = auditNo;

            scopeAndScheduleRepo.Insert(newSS);
        }

        //Generate Random 5 Digit Number
        public int GenerateRandomNo()
        {
            int _min = 00000;
            int _max = 99999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        private string GenerateAuditNumber(Audit a, bool isCentralized)
        {
            string auditNumber = "";
            if (a.Location != null && !String.IsNullOrWhiteSpace(a.Location.Countrycode))
            {
                auditNumber = auditNumber + GetInitials(a.Location.Countrycode);
            }

            if (a.OverallAssesment.ProcessRiskMapping.ProcessL2 != null)
            {
                auditNumber = auditNumber + "/" + GetInitials(a.OverallAssesment.ProcessRiskMapping.ProcessL2.Name);
            }
            else if (a.OverallAssesment.ProcessRiskMapping.ProcessL1 != null)
            {
                auditNumber = auditNumber + "/" + GetInitials(a.OverallAssesment.ProcessRiskMapping.ProcessL1.Name);
            }
            else if (a.OverallAssesment.ProcessRiskMapping.BusinessCycle != null)
            {
                auditNumber = auditNumber + "/" + GetInitials(a.OverallAssesment.ProcessRiskMapping.BusinessCycle.Name);
            }

            /*if (!isCentralized && !String.IsNullOrWhiteSpace(a.Location.ProfitCenterCode))
            {
                auditNumber = auditNumber + "/" + a.Location.ProfitCenterCode;
            }*/

            auditNumber = auditNumber + "/" + GetCurrentFinancialYear();

            return auditNumber;
        }

        public string GetCurrentFinancialYear()
        {
            int CurrentYear = DateTime.Today.Year;
            int PreviousYear = DateTime.Today.Year - 1;
            int NextYear = DateTime.Today.Year + 1;
            string PreYear = PreviousYear.ToString();
            string NexYear = NextYear.ToString();
            string CurYear = CurrentYear.ToString();
            string FinYear = null;

            if (DateTime.Today.Month > 3)
                FinYear = CurYear + "-" + NexYear;
            else
                FinYear = PreYear + "-" + CurYear;
            return FinYear.Trim();
        }

        private string GetInitials(string countrycode)
        {
            countrycode = countrycode.Trim();
            String[] wordsArray = countrycode.Split();
            int numberOfWords = wordsArray.Length;

            if (numberOfWords == 1)
            {
                string firstFourChar = !String.IsNullOrWhiteSpace(countrycode) && countrycode.Length >= 4
                    ? countrycode.Substring(0, 4).ToUpper() : countrycode.ToUpper();
                return firstFourChar;
            }
            else if (numberOfWords == 2)
            {
                string firstThreeChar = !String.IsNullOrWhiteSpace(wordsArray[0]) && wordsArray[0].Length >= 3
                    ? wordsArray[0].Substring(0, 3).ToUpper() : wordsArray[0].ToUpper();
                string secondChar = !String.IsNullOrWhiteSpace(wordsArray[1]) && wordsArray[0].Length >= 1
                    ? wordsArray[1].Substring(0, 1).ToUpper() : wordsArray[1].ToUpper();
                return firstThreeChar + secondChar;
            }
            else if (numberOfWords == 3)
            {
                string firstTwoChar = !String.IsNullOrWhiteSpace(wordsArray[0]) && wordsArray[0].Length >= 2
                        ? wordsArray[0].Substring(0, 2).ToUpper() : wordsArray[0].ToUpper();
                string secondChar = !String.IsNullOrWhiteSpace(wordsArray[1]) && wordsArray[0].Length >= 1
                    ? wordsArray[1].Substring(0, 1).ToUpper() : wordsArray[1].ToUpper();
                string thirdChar = !String.IsNullOrWhiteSpace(wordsArray[2]) && wordsArray[2].Length >= 1
                    ? wordsArray[2].Substring(0, 1).ToUpper() : wordsArray[2].ToUpper();
                return firstTwoChar + secondChar + thirdChar;
            }
            else if (numberOfWords >= 4)
            {
                string resultString = "";
                foreach (string word in wordsArray)
                {
                    resultString = resultString + word[0];
                }
                return resultString;
            }
            return "";
        }

        public Company GetCompanyDetail(string Companyid)
        {
            var companykRepo = new MongoGenericRepository<Company>(_dbsetting);
            var company = companykRepo.GetWithInclude<Country, State, CityOrTown>(x => x.Id == Companyid).FirstOrDefault();
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            if (company != null)
            {
                company.AuditManager = userRepo.GetByID(company.AuditManagerId);
                company.Coordinator = userRepo.GetByID(company.CoordinatorId);
                company.HeadOfAudit = userRepo.GetByID(company.HeadOfAuditId);
            }

            return company;
        }

        public Location GetLocationDetail(string locationId)
        {
            var locRepo = new MongoGenericRepository<Location>(_dbsetting);
            var location = locRepo.GetWithInclude<State, Country, Company, CityOrTown>(x => x.Id == locationId).FirstOrDefault();
            if (location != null && location.Company != null)
            {
                location.Company = GetCompanyDetail(location.Company.Id);
            }
            return location;
        }

        public Audit GetAuditDetail(string AuditId)
        {
            var locRepo = new MongoGenericRepository<Audit>(_dbsetting);

            var audit = locRepo.GetWithInclude<OverallAssesment, ProcessLocationMapping>(x => x.Id == AuditId).FirstOrDefault();

            if (audit != null && audit.Location != null)
                audit.Location = GetLocationDetail(audit.Location.Id);

            var OverAllRepo = new MongoGenericRepository<OverallAssesment>(_dbsetting);

            if (audit.OverallAssesmentId != null)
                audit.OverallAssesment = OverAllRepo.GetWithInclude<BusinessCycle, ProcessL1, ProcessL2>(x => x.Id == audit.OverallAssesmentId).FirstOrDefault();

            if (audit.ProcessLocationMapping != null)
            {
                var ProcessLocationRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
                audit.ProcessLocationMapping = ProcessLocationRepo.GetWithInclude<BusinessCycle, ProcessL1, ProcessL2>(x => x.Id == audit.ProcessLocationMapping.Id).FirstOrDefault();
            }

            return audit;
        }

        public void InsertUpdateAuditApproverMapping(AuditApprovalMapping mp, string userid)
        {
            try
            {
                if (mp != null)
                {
                    var auditAppMppingRepo = new MongoGenericRepository<AuditApprovalMapping>(_dbsetting);
                    auditAppMppingRepo.Delete(x => x.AuditId == mp.AuditId);
                    mp.CreatedBy = userid;
                    auditAppMppingRepo.Insert(mp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public AuditApprovalMapping AttachAuditApproverMapping(string auditId)
        {
            var auditAppMppingRepo = new MongoGenericRepository<AuditApprovalMapping>(_dbsetting);
            var app = auditAppMppingRepo.GetWithInclude<Audit>(x => x.AuditId == auditId).FirstOrDefault();

            if (app == null)
                app = new AuditApprovalMapping();
            else
            {
                var modUserData = new List<UserData>();
                foreach (var item in app.UserData)
                {
                    var userRepo = new MongoGenericRepository<User>(_dbsetting);
                    item.User = userRepo.GetByID(item.UserId);
                    modUserData.Add(item);
                }
                app.UserData = modUserData;
            }
            return app;
        }

        public string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        public Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        public async Task<MemoryStream> GetMemoryStream(IEnumerable<string> csvString, string location, string filename)
        {
            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }
            var path = Path.Combine(location, filename);

            using (TextWriter tw = System.IO.File.CreateText(path))
            {
                foreach (var line in csvString)
                {
                    tw.WriteLine(line);
                }
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            // after converting to memory delete the file
            System.IO.File.Delete(path);
            return memory;
        }

        public string InsertBusinessCycle(BusinessCycle _businessCycle)
        {
            try
            {
                if (_businessCycle != null)
                {
                    var businessCycleRepo = new MongoGenericRepository<BusinessCycle>(_dbsetting);
                    var existModel = businessCycleRepo.GetFirst(x => x.Name == _businessCycle.Name);

                    if (existModel == null)
                    {
                        businessCycleRepo.Insert(_businessCycle);
                        return _businessCycle.Id;
                    }
                    else
                        return existModel.Id;
                }

                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string InsertProcessL1(ProcessL1 _processL1)
        {
            try
            {
                if (_processL1 != null)
                {
                    var _processL1Repo = new MongoGenericRepository<ProcessL1>(_dbsetting);
                    var existModel = _processL1Repo.GetFirst(x => x.Name == _processL1.Name && x.BusinessCycleId == _processL1.BusinessCycleId);

                    if (existModel == null)
                    {
                        _processL1Repo.Insert(_processL1);
                        return _processL1.Id;
                    }
                    else
                        return existModel.Id;
                }

                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InsertProcessL2(ProcessL2 _processL2)
        {
            try
            {
                if (_processL2 != null)
                {
                    var _processL2Repo = new MongoGenericRepository<ProcessL2>(_dbsetting);

                    if (!_processL2Repo.Exists(x => x.Name == _processL2.Name && x.ProcessL1Id == _processL2.ProcessL1Id && x.BusinessCycleId == _processL2.BusinessCycleId))
                        _processL2Repo.Insert(_processL2);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Designation> GetAllDesignations()
        {
            var designationRepo = new MongoGenericRepository<Designation>(_dbsetting);
            var designations = designationRepo.GetAll();
            return designations.ToList();
        }
        public void SaveHistoryforDiscussionNote(string user, string status, string discussionNote)
        {
            var repoHistroy = new MongoGenericRepository<DiscussionNoteHistory>(_dbsetting);
            try
            {
                switch (status)
                {
                    case AuditConstants.Status.INPROGRESS:
                        status = AuditConstants.CommonStatus.REJECTED; break;
                    case AuditConstants.Status.COMPLETED:
                        status = AuditConstants.CommonStatus.APPROVED; break;
                    case AuditConstants.CommonStatus.SAVETODRAFT:
                        status = AuditConstants.CommonStatus.SAVETODRAFT; break;
                    default:
                        status = ""; break;
                }

                DiscussionNoteHistory objDiscussionNoteHistory = new DiscussionNoteHistory();
                objDiscussionNoteHistory.UserId = user;
                objDiscussionNoteHistory.Status = status;
                objDiscussionNoteHistory.DiscussionNoteID = discussionNote;
                objDiscussionNoteHistory.DiscussNoteDate = DateTime.Now;
                objDiscussionNoteHistory.CreatedBy = user;
                repoHistroy.Insert(objDiscussionNoteHistory);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void SaveHistoryForDraftReport(string user, string status, string discussionNote)
        {
            var repoHistroy = new MongoGenericRepository<DraftReportHistory>(_dbsetting);
            try
            {
                switch (status)
                {
                    case AuditConstants.Status.INPROGRESS:
                        status = AuditConstants.CommonStatus.REJECTED; break;
                    case AuditConstants.CommonStatus.APPROVED:
                        status = AuditConstants.CommonStatus.APPROVED; break;
                    case AuditConstants.Status.COMPLETED:
                        status = AuditConstants.CommonStatus.SAVETOFINALREPORT; break;
                    default:
                        status = ""; break;
                }

                DraftReportHistory objDiscussionNoteHistory = new DraftReportHistory();
                objDiscussionNoteHistory.UserId = user;
                objDiscussionNoteHistory.Status = status;
                objDiscussionNoteHistory.DraftReportID = discussionNote;
                objDiscussionNoteHistory.DraftReportDate = DateTime.Now;
                objDiscussionNoteHistory.CreatedBy = user;
                repoHistroy.Insert(objDiscussionNoteHistory);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void SaveHistoryforTestingofControl(string user, string status, string procedureDetailsId)
        {
            var repoHistroy = new MongoGenericRepository<TestingofControlHistory>(_dbsetting);
            try
            {
                switch (status.ToUpper())
                {
                    case AuditConstants.Status.INPROGRESS:
                        status = AuditConstants.CommonStatus.REJECTED; break;
                    case AuditConstants.Status.COMPLETED:
                        status = AuditConstants.CommonStatus.APPROVED; break;
                    case AuditConstants.CommonStatus.SAVETODRAFT:
                        status = AuditConstants.CommonStatus.SAVETODRAFT; break;
                    default:
                        status = ""; break;
                }

                TestingofControlHistory objDiscussionNoteHistory = new TestingofControlHistory();
                objDiscussionNoteHistory.UserId = user;
                objDiscussionNoteHistory.Status = status;
                objDiscussionNoteHistory.RACMAuditProcedureDetailsId = procedureDetailsId;
                objDiscussionNoteHistory.TestingOfControlDate = DateTime.Now;
                objDiscussionNoteHistory.CreatedBy = user;
                repoHistroy.Insert(objDiscussionNoteHistory);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public FileContentByte DownloadExcelAttachmentForAuditProcedure(RACMAuditProcedureDetails item)
        {
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var businessCycleRepo = new MongoGenericRepository<BusinessCycle>(_dbsetting);
            var processL1Repo = new MongoGenericRepository<ProcessL1>(_dbsetting);
            var processL2Repo = new MongoGenericRepository<ProcessL2>(_dbsetting);
            var tocUploadRepo = new MongoGenericRepository<TestingOfControlUpload>(_dbsetting);

            item.Responsibility = userRepo.GetByID(item.ResponsibilityId);
            item.Reviewer = userRepo.GetByID(item.ReviewerId);

            if (item.RACMAuditProcedure != null)
            {
                item.RACMAuditProcedure.Control.User = userRepo.GetByID(item.RACMAuditProcedure.Control.UserId) == null ? new User() : userRepo.GetByID(item.RACMAuditProcedure.Control.UserId);
                item.RACMAuditProcedure.BusinessCycle = businessCycleRepo.GetByID(item.RACMAuditProcedure.BusinessCycleId) == null ? new BusinessCycle() : businessCycleRepo.GetByID(item.RACMAuditProcedure.BusinessCycleId);
                item.RACMAuditProcedure.ProcessL1 = processL1Repo.GetByID(item.RACMAuditProcedure.ProcessL1Id) == null ? new ProcessL1() : processL1Repo.GetByID(item.RACMAuditProcedure.ProcessL1Id);
                item.RACMAuditProcedure.ProcessL2 = processL2Repo.GetByID(item.RACMAuditProcedure.ProcessL2Id) == null ? new ProcessL2() : processL2Repo.GetByID(item.RACMAuditProcedure.ProcessL2Id);
            }
            else
            {
                item.RACMAuditProcedure = new RACMAuditProcedure();
                item.RACMAuditProcedure.Control = new Control();
                item.RACMAuditProcedure.Control.User = new User();
                item.RACMAuditProcedure.BusinessCycle = new BusinessCycle();
                item.RACMAuditProcedure.ProcessL1 = new ProcessL1();
                item.RACMAuditProcedure.ProcessL2 = new ProcessL2();
            }

            item.TestingOfControlUploads = tocUploadRepo.GetMany(x => x.TestingOfCountrolId == item.Id).ToList();
            var fileName = "Testing of Control.xlsx";
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells["A1"].Value = "Business Cycle";
                worksheet.Cells["B1"].Value = "Process L1";
                worksheet.Cells["C1"].Value = "Process L2";
                worksheet.Cells["D1"].Value = "Risk ID";
                worksheet.Cells["E1"].Value = "Risk Rating";
                worksheet.Cells["F1"].Value = "Risk Description";
                worksheet.Cells["G1"].Value = "Control ID";
                worksheet.Cells["H1"].Value = "Control Type";
                worksheet.Cells["I1"].Value = "Control Nature";
                worksheet.Cells["J1"].Value = "Control Frequency";
                worksheet.Cells["K1"].Value = "Control Owner";
                worksheet.Cells["L1"].Value = "Control Description";
                worksheet.Cells["M1"].Value = "Procedure ID";
                worksheet.Cells["N1"].Value = "Procedure Title";
                worksheet.Cells["O1"].Value = "Procedure Description";
                worksheet.Cells["P1"].Value = "Start Date";
                worksheet.Cells["Q1"].Value = "End Date";
                worksheet.Cells["R1"].Value = "Testing Result";
                worksheet.Cells["S1"].Value = "Analytics";
                worksheet.Cells["T1"].Value = "Analytics Test Number";
                worksheet.Cells["U1"].Value = "Performed By";
                worksheet.Cells["V1"].Value = "Reviewed By";
                worksheet.Cells["W1"].Value = "Finding";
                worksheet.Cells["X1"].Value = "Design Marks";
                worksheet.Cells["Y1"].Value = "Design Effectiveness (%)";
                worksheet.Cells["Z1"].Value = "OE Marks";
                worksheet.Cells["AA1"].Value = "OE Effectiveness (%)";

                var rowIndex = 2;

                worksheet.Cells["A" + rowIndex.ToString()].Value = item.RACMAuditProcedure.BusinessCycle.Name;
                worksheet.Cells["B" + rowIndex.ToString()].Value = item.RACMAuditProcedure.ProcessL1.Name;
                worksheet.Cells["C" + rowIndex.ToString()].Value = item.RACMAuditProcedure.ProcessL2.Name;
                worksheet.Cells["D" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Risk != null ? item.RACMAuditProcedure.Risk.RiskId : "";
                worksheet.Cells["E" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Risk != null ? item.RACMAuditProcedure.Risk.RiskId : "";
                worksheet.Cells["F" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Risk != null ? item.RACMAuditProcedure.Risk.Description : "";
                worksheet.Cells["G" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Control != null ? item.RACMAuditProcedure.Control.ControlId : "";
                worksheet.Cells["H" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Control != null ? item.RACMAuditProcedure.Control.Type : "";
                worksheet.Cells["I" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Control != null ? item.RACMAuditProcedure.Control.Nature : "";
                worksheet.Cells["J" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Control != null ? item.RACMAuditProcedure.Control.Frequency : "";
                worksheet.Cells["K" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Control != null ? item.RACMAuditProcedure.Control.User.FirstName + " " + item.RACMAuditProcedure.Control.User.LastName : "";
                worksheet.Cells["L" + rowIndex.ToString()].Value = item.RACMAuditProcedure.Control != null ? item.RACMAuditProcedure.Control.Description : "";
                worksheet.Cells["M" + rowIndex.ToString()].Value = item.Procedure != null ? item.Procedure.ProcedureId : "";
                worksheet.Cells["N" + rowIndex.ToString()].Value = item.Procedure != null ? item.Procedure.ProcedureTitle : "";
                worksheet.Cells["O" + rowIndex.ToString()].Value = item.Procedure != null ? item.Procedure.ProcedureDesc : "";
                worksheet.Cells["P" + rowIndex.ToString()].Value = item.ProcedureStartDate;
                worksheet.Cells["Q" + rowIndex.ToString()].Value = item.ProcedureEndDate;
                worksheet.Cells["R" + rowIndex.ToString()].Value = item.Conclusion;
                worksheet.Cells["S" + rowIndex.ToString()].Value = item.Analytics;
                worksheet.Cells["T" + rowIndex.ToString()].Value = item.TestNumber;
                worksheet.Cells["U" + rowIndex.ToString()].Value = item.Responsibility != null ? item.Responsibility.FirstName + " " + item.Responsibility.LastName : "";
                worksheet.Cells["V" + rowIndex.ToString()].Value = item.Reviewer != null ? item.Reviewer.FirstName + " " + item.Reviewer.LastName : "";
                worksheet.Cells["W" + rowIndex.ToString()].Value = item.Finding;
                worksheet.Cells["X" + rowIndex.ToString()].Value = item.DesignMarks;
                worksheet.Cells["Y" + rowIndex.ToString()].Value = item.DesignEffectiveness;
                worksheet.Cells["Z" + rowIndex.ToString()].Value = item.OEMarks;
                worksheet.Cells["AA" + rowIndex.ToString()].Value = item.OEEffectiveness;

                rowIndex++;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                package.Save();
            }
            byte[] bytes = memoryStream.ToArray();

            FileContentByte objFileContentByt = new FileContentByte()
            {
                FileContents = bytes,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileDownloadName = fileName,
            };

            return objFileContentByt;
        }
        public FileContentByte DownloadExcelAttachmentForDataTracker(InitialDataRequest tList)
        {
            var repoUser = new MongoGenericRepository<User>(_dbsetting);

            var fileName = "DataTracker.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells["A1"].Value = "Area";
                worksheet.Cells["B1"].Value = "Status";
                worksheet.Cells["C1"].Value = "Data Requested";
                worksheet.Cells["D1"].Value = "Data Request Date";
                worksheet.Cells["E1"].Value = "Data Received Date";
                worksheet.Cells["F1"].Value = "Process Owner";
                worksheet.Cells["G1"].Value = "Pending Data";

                var rowIndex = 2;

                var processOwner = repoUser.GetFirst(x => x.Id == tList.ProcessOwnerId);

                worksheet.Cells["A" + rowIndex.ToString()].Value = tList.Area;
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.Status;
                worksheet.Cells["C" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.DataRequested);

                worksheet.Cells["D" + rowIndex.ToString()].Style.Numberformat.Format = "dd-MMM-yyyy";
                worksheet.Cells["D" + rowIndex.ToString()].Value = Convert.ToDateTime(tList.DataRequestDate);

                worksheet.Cells["E" + rowIndex.ToString()].Style.Numberformat.Format = "dd-MMM-yyyy";
                worksheet.Cells["E" + rowIndex.ToString()].Value = Convert.ToDateTime(tList.DataReceivedDate);

                worksheet.Cells["F" + rowIndex.ToString()].Value = processOwner != null ? processOwner.FirstName + " " + processOwner.LastName : "";
                worksheet.Cells["G" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.PendingData);

                rowIndex++;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;

                package.Save();
            }
            byte[] bytes = memoryStream.ToArray();

            FileContentByte objFileContentByt = new FileContentByte()
            {
                FileContents = bytes,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileDownloadName = fileName,
            };

            return objFileContentByt;
        }
        public FileContentByte DownloadExcelAttachmentForDraftReport(DraftReport tList)
        {
            var RepoDiscussionNote = new MongoGenericRepository<DiscussionNote>(_dbsetting);
            var risktypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);

            if (tList.DiscussionNoteID != null)
            {
                var objDiscussionNote = RepoDiscussionNote.GetFirst(p => p.Id == tList.DiscussionNoteID);
                if (objDiscussionNote != null)
                {
                    tList.DiscussionNote = objDiscussionNote;

                    if (tList.DiscussionNote.RiskTypeId != null)
                    {
                        var objRiskType = risktypeRepo.GetFirst(p => p.Id == tList.DiscussionNote.RiskTypeId);
                        if (objRiskType != null)
                        {
                            tList.DiscussionNote.RiskType = objRiskType;
                        }
                    }
                }
            }

            var fileName = "DraftReport.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                #region Sheet 1
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("DraftReport");
                int rowIndex = 2;
                worksheet.Cells["A1"].Value = "Observation No";
                worksheet.Cells["B1"].Value = "RACM Number";
                worksheet.Cells["C1"].Value = "Observation Heading ";
                worksheet.Cells["D1"].Value = "Background";
                worksheet.Cells["E1"].Value = "Detailed Observation";
                worksheet.Cells["F1"].Value = "Root Cause";
                worksheet.Cells["G1"].Value = "Risks / Business Impact";
                worksheet.Cells["H1"].Value = "Recommendation";
                worksheet.Cells["I1"].Value = "Risk Type";
                worksheet.Cells["J1"].Value = "Value At Risk";
                worksheet.Cells["K1"].Value = "Observations Grading";
                worksheet.Cells["L1"].Value = "Repeat";
                worksheet.Cells["M1"].Value = "System Improvement";
                worksheet.Cells["N1"].Value = "Red Flag";
                worksheet.Cells["O1"].Value = "Leading Practices";
                worksheet.Cells["P1"].Value = "Potential Saving";
                worksheet.Cells["Q1"].Value = "Realised Saving";
                worksheet.Cells["R1"].Value = "Leakage";
                worksheet.Cells["S1"].Value = "Report Consideration";
                worksheet.Cells["T1"].Value = "Impact";
                worksheet.Cells["U1"].Value = "Recommendation";
                worksheet.Cells["V1"].Value = "Cause";
                worksheet.Cells["W1"].Value = "Management Response";

                worksheet.Cells["A" + rowIndex.ToString()].Value = tList.ObservationNumber;
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.DiscussionNote != null ? getRACM_Ids(tList.DiscussionNote.RACM_Ids) : "";
                worksheet.Cells["C" + rowIndex.ToString()].Value = tList.DiscussionNote != null ? tList.DiscussionNote.ObservationHeading : "";
                worksheet.Cells["D" + rowIndex.ToString()].Value = tList.DiscussionNote != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.DiscussionNote.FieldBackground) : "";
                worksheet.Cells["E" + rowIndex.ToString()].Value = tList.DiscussionNote != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.DiscussionNote.DetailedObservation) : "";
                worksheet.Cells["F" + rowIndex.ToString()].Value = tList.DiscussionNote != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.DiscussionNote.RootCause) : "";
                worksheet.Cells["G" + rowIndex.ToString()].Value = tList.DiscussionNote != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.DiscussionNote.Risks) : "";
                worksheet.Cells["H" + rowIndex.ToString()].Value = tList.Recommendation != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.Recommendation) : "";
                var _riskTypes = string.Empty;

                if (tList.DiscussionNote != null && tList.DiscussionNote.RiskTypeIds.Length > 0)
                {
                    foreach (var _riskItem in tList.DiscussionNote.RiskTypeIds)
                    {
                        var _riskType = risktypeRepo.GetByID(_riskItem);

                        if (_riskType != null)
                        {
                            _riskTypes += _riskType.Name + ", ";
                        }
                    }
                    _riskTypes = _riskTypes.Trim().TrimEnd(',');
                }
                worksheet.Cells["I" + rowIndex.ToString()].Value = _riskTypes;
                worksheet.Cells["J" + rowIndex.ToString()].Value = tList.ValueAtRisk;
                worksheet.Cells["K" + rowIndex.ToString()].Value = tList.ObservationGrading;
                worksheet.Cells["L" + rowIndex.ToString()].Value = tList.DiscussionNote != null ? tList.DiscussionNote.IsRepeat ? "Yes" : "No" : "";
                worksheet.Cells["M" + rowIndex.ToString()].Value = tList.isSystemImprovement ? "Yes" : "No";
                worksheet.Cells["N" + rowIndex.ToString()].Value = tList.isRedFlag ? "Yes" : "No";
                worksheet.Cells["O" + rowIndex.ToString()].Value = tList.isLeadingPractices ? "Yes" : "No";
                worksheet.Cells["P" + rowIndex.ToString()].Value = tList.PotentialSaving;
                worksheet.Cells["Q" + rowIndex.ToString()].Value = tList.RealisedSaving;
                worksheet.Cells["R" + rowIndex.ToString()].Value = tList.Leakage;
                worksheet.Cells["S" + rowIndex.ToString()].Value = tList.ReportConsiderations != null ? getReportConsideration(tList.ReportConsiderations.ToList()) : "";
                worksheet.Cells["T" + rowIndex.ToString()].Value = tList.Impacts != null ? getImpacts(tList.Impacts.ToList()) : "";
                worksheet.Cells["U" + rowIndex.ToString()].Value = tList.Recommendations != null ? getRecommendations(tList.Recommendations.ToList()) : "";
                worksheet.Cells["V" + rowIndex.ToString()].Value = tList.RootCauses != null ? getCause(tList.RootCauses.ToList()) : "";
                worksheet.Cells["W" + rowIndex.ToString()].Value = tList.ManagementResponse != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.ManagementResponse) : "";


                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Sheet 2 - Action Plan
                ExcelWorksheet worksheet1 = package.Workbook.Worksheets.Add("Action Plan");

                worksheet1.Cells["A1"].Value = "Action Plan";
                worksheet1.Cells["B1"].Value = "Implementation Owner";
                worksheet1.Cells["C1"].Value = "Implementation Start Date";
                worksheet1.Cells["D1"].Value = "Implementation End Date";
                var userRepo = new MongoGenericRepository<User>(_dbsetting);


                int rowIndexAction = 2;
                if (tList.ActionPlan != null)
                {
                    foreach (var actionPlan in tList.ActionPlans)
                    {
                        if (actionPlan.ProcessOwnerID != null)
                        {
                            var objProcessOwner = userRepo.GetFirst(p => p.Id == actionPlan.ProcessOwnerID);
                            if (objProcessOwner != null)
                            {
                                tList.ProcessOwner = objProcessOwner;
                            }
                        }
                        worksheet1.Cells["A" + rowIndexAction.ToString()].Value = actionPlan.ActionPlan != null ? VJLiabraries.UtilityMethods.HtmlToText(actionPlan.ActionPlan) : "";
                        worksheet1.Cells["B" + rowIndexAction.ToString()].Value = tList.ProcessOwner != null ? tList.ProcessOwner.FirstName + " " + tList.ProcessOwner.LastName : "";
                        worksheet1.Cells["C" + rowIndexAction.ToString()].Value = actionPlan.ImplementationStartDate;
                        worksheet1.Cells["D" + rowIndexAction.ToString()].Value = actionPlan.ImplementationEndDate;

                        rowIndex++;
                    }
                }
                worksheet1.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet1.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion
                package.Save();
            }
            byte[] bytes = memoryStream.ToArray();

            FileContentByte objFileContentByt = new FileContentByte()
            {
                FileContents = bytes,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileDownloadName = fileName,
            };
            return objFileContentByt;
        }
        public FileContentByte DownloadExcelAttachmentForTOR(TOR tList)
        {
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var activtiyRepo = new MongoGenericRepository<Activity>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var idrRepo = new MongoGenericRepository<InitialDataRequest>(_dbsetting);

            tList.Activities = activtiyRepo.GetMany(x => x.AuditID == tList.AuditId).ToHashSet();
            tList.Audit = scopeAndScheduleRepo.GetFirst(x => x.AuditId == tList.AuditId);
            tList.AuditSpecificInformations = idrRepo.GetMany(x => x.AuditId == tList.AuditId).ToList();
            if (tList.Audit != null)
            {
                tList.Audit = populateScopeAndSchedule(tList.Audit);
            }

            var fileName = "TOR.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                #region Sheet 1
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TOR");

                worksheet.Cells["A1"].Value = "Audit Unit";
                worksheet.Cells["B1"].Value = tList.Audit.Location.DivisionDescription;

                worksheet.Cells["A2"].Value = "Audit Name";
                worksheet.Cells["B2"].Value = tList.Audit.Audit.ProcessLocationMapping.AuditName;

                worksheet.Cells["A3"].Value = "Audit Approvers";
                worksheet.Cells["B3"].Value = GetAuditApprovers(tList.Audit.AuditApprovalMapping);

                worksheet.Cells["A4"].Value = "Audit Team";
                worksheet.Cells["B4"].Value = GetAuditTeam(tList.Audit.AuditResources);

                worksheet.Cells["A5"].Value = "Location";
                worksheet.Cells["B5"].Value = tList.Audit.Location.ProfitCenterCode;

                worksheet.Cells["A6"].Value = "Period Of Audit";
                worksheet.Cells["B6"].Value = tList.Audit.AuditStartDate + " - " + tList.Audit.AuditEndDate;

                worksheet.Cells["A7"].Value = "TOR Date Issued";
                worksheet.Cells["B7"].Value = tList.TORIssuedDate;

                var rowIndex = 8;

                foreach (var activity in tList.Activities)
                {
                    worksheet.Cells["A" + rowIndex.ToString()].Value = activity.ActivityName + " Start Date";
                    worksheet.Cells["B" + rowIndex.ToString()].Value = activity.ActualStartDate.ToString("yyy/MM/dd");
                    rowIndex++;

                    worksheet.Cells["A" + rowIndex.ToString()].Value = activity.ActivityName + " End Date";
                    worksheet.Cells["B" + rowIndex.ToString()].Value = activity.ActualEndDate.ToString("yyy/MM/dd");
                    rowIndex++;
                }

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Audit Objectives";
                worksheet.Cells["B" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.AuditObjective);
                rowIndex++;

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Policies, Standards & Reference Document";
                worksheet.Cells["B" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.Policies);
                rowIndex++;

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Deliverable";
                worksheet.Cells["B" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.Deliverable);
                rowIndex++;

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Disclaimer";
                worksheet.Cells["B" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.Disclaimer);
                rowIndex++;

                worksheet.Cells["A" + rowIndex.ToString()].Value = "Limitation";
                worksheet.Cells["B" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(tList.Limitation);
                rowIndex++;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:A" + rowIndex.ToString()].Style.Font.Bold = true;
                #endregion

                #region Sheet 2 - Audit Scopes
                ExcelWorksheet worksheet1 = package.Workbook.Worksheets.Add("Audit Scopes");

                worksheet1.Cells["A1"].Value = "Areas";
                worksheet1.Cells["B1"].Value = "Scope";

                rowIndex = 2;

                foreach (var scope in tList.AuditScopes)
                {
                    worksheet1.Cells["A" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(scope.Areas);
                    worksheet1.Cells["B" + rowIndex.ToString()].Value = VJLiabraries.UtilityMethods.HtmlToText(scope.Scope);

                    rowIndex++;
                }

                worksheet1.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet1.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Sheet 3 - Audit Specific Information
                ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add("Audit Specific Information");

                worksheet2.Cells["A1"].Value = "Area";
                worksheet2.Cells["B1"].Value = "Data Requested";
                worksheet2.Cells["C1"].Value = "Process Owner";
                worksheet2.Cells["D1"].Value = "Data Request Date";
                worksheet2.Cells["E1"].Value = "Data Received Date";
                worksheet2.Cells["F1"].Value = "Pending Data";
                worksheet2.Cells["G1"].Value = "Status";

                rowIndex = 2;

                foreach (var info in tList.AuditSpecificInformations)
                {
                    worksheet2.Cells["A" + rowIndex.ToString()].Value = info.Area;
                    worksheet2.Cells["B" + rowIndex.ToString()].Value = info.DataRequested;
                    worksheet2.Cells["C" + rowIndex.ToString()].Value = info.ProcessOwner;
                    worksheet2.Cells["D" + rowIndex.ToString()].Value = info.DataRequestDate;
                    worksheet2.Cells["E" + rowIndex.ToString()].Value = info.DataReceivedDate;
                    worksheet2.Cells["F" + rowIndex.ToString()].Value = info.PendingData;
                    worksheet2.Cells["G" + rowIndex.ToString()].Value = info.Status;

                    rowIndex++;
                }

                worksheet2.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet2.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                package.Save();
            }

            byte[] bytes = memoryStream.ToArray();
            FileContentByte objFileContentByt = new FileContentByte()
            {
                FileContents = bytes,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileDownloadName = fileName,
            };
            return objFileContentByt;
        }
        public FileContentByte DownloadExcelAttachmentForFollowUp(FollowUp tList)
        {
            var repoFollowupActionPlan = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);
            var userRepo = new MongoGenericRepository<User>(_dbsetting);

            var fileName = "FollowUp.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                #region Sheet 1
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("FollowUp");
                int rowIndex = 2;
                worksheet.Cells["A1"].Value = "Audit";
                worksheet.Cells["B1"].Value = "Root Cause";
                worksheet.Cells["C1"].Value = "Observation Heading";
                worksheet.Cells["D1"].Value = "Observations Grading";
                worksheet.Cells["E1"].Value = "Detailed Observation";
                worksheet.Cells["F1"].Value = "Risk Type";
                worksheet.Cells["G1"].Value = "End Date";
                worksheet.Cells["H1"].Value = "Status";


                worksheet.Cells["A" + rowIndex.ToString()].Value = tList.ProcessLocationMapping != null ? tList.ProcessLocationMapping.AuditName : "";
                worksheet.Cells["B" + rowIndex.ToString()].Value = tList.RootCauses != null ? getCause(tList.RootCauses.ToList()) : "";
                worksheet.Cells["C" + rowIndex.ToString()].Value = tList.ObservationHeading;
                worksheet.Cells["D" + rowIndex.ToString()].Value = tList.ObservationGrading;
                worksheet.Cells["E" + rowIndex.ToString()].Value = tList.DetailedObservation != null ? VJLiabraries.UtilityMethods.HtmlToText(tList.DetailedObservation) : "";
                worksheet.Cells["F" + rowIndex.ToString()].Value = tList.RiskType != null ? tList.RiskType.Name : "";
                worksheet.Cells["G" + rowIndex.ToString()].Value = tList.ImplementationEndDate;
                worksheet.Cells["H" + rowIndex.ToString()].Value = tList.Status;


                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Sheet 2 - Action Plan
                ExcelWorksheet worksheet1 = package.Workbook.Worksheets.Add("Action Plan");

                worksheet1.Cells["A1"].Value = "Management Comments";
                worksheet1.Cells["B1"].Value = "Action Plan";
                worksheet1.Cells["C1"].Value = "Implementation Owner";
                worksheet1.Cells["D1"].Value = "Revised Date";


                int rowIndexAction = 2;

                foreach (var actionPlan in tList.ActionPlansInfo)
                {
                    worksheet1.Cells["A" + rowIndexAction.ToString()].Value = actionPlan.Comments != null ? VJLiabraries.UtilityMethods.HtmlToText(actionPlan.Comments) : "";
                    worksheet1.Cells["B" + rowIndexAction.ToString()].Value = actionPlan.ActionPlan != null ? VJLiabraries.UtilityMethods.HtmlToText(actionPlan.ActionPlan) : "";
                    if (actionPlan.ImplementationOwnerId != null)
                    {
                        var objImplementationOwner = userRepo.GetFirst(p => p.Id == actionPlan.ImplementationOwnerId);
                        if (objImplementationOwner != null)
                        {
                            worksheet1.Cells["C" + rowIndexAction.ToString()].Value = objImplementationOwner.FirstName + " " + objImplementationOwner.LastName;
                        }
                    }
                    worksheet1.Cells["D" + rowIndexAction.ToString()].Value = actionPlan.RevisedDate;
                    rowIndex++;
                }

                worksheet1.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet1.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion
                package.Save();
            }
            byte[] bytes = memoryStream.ToArray();

            FileContentByte objFileContentByt = new FileContentByte()
            {
                FileContents = bytes,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileDownloadName = fileName,
            };
            return objFileContentByt;
        }
        public string getRACM_Ids(List<string> lstRacmID)
        {
            List<string> lstID = new List<string>();
            foreach (var racmID in lstRacmID)
            {
                lstID.Add(racmID);
            }
            String[] Id = lstID.ToArray();
            var str = String.Join(",", Id);
            return str;
        }
        public string getReportConsideration(List<string> lstReportConsideration)
        {
            var _repoReportConsideration = new MongoGenericRepository<ReportConsideration>(_dbsetting);
            List<string> lstRC = new List<string>();
            foreach (var RcID in lstReportConsideration)
            {
                var objReport = _repoReportConsideration.GetFirst(p => p.Id == RcID);
                if (objReport != null)
                {
                    lstRC.Add(objReport.Name);
                }
            }
            String[] Id = lstRC.ToArray();
            var str = String.Join(",", Id);
            return str;
        }
        public string getImpacts(List<string> lstImpact)
        {
            var _repoImpactMaster = new MongoGenericRepository<ImpactMaster>(_dbsetting);
            List<string> lstimp = new List<string>();
            foreach (var impactId in lstImpact)
            {
                var objImpact = _repoImpactMaster.GetFirst(p => p.Id == impactId);
                if (objImpact != null)
                {
                    lstimp.Add(objImpact.Name);
                }
            }
            String[] Id = lstimp.ToArray();
            var str = String.Join(",", Id);
            return str;
        }
        public string getRecommendations(List<string> lstRecommendation)
        {
            var _repoRecommendation = new MongoGenericRepository<Recommendation>(_dbsetting);
            List<string> lstRecom = new List<string>();
            foreach (var recomId in lstRecommendation)
            {
                var objRecommendations = _repoRecommendation.GetFirst(p => p.Id == recomId);
                if (objRecommendations != null)
                {
                    lstRecom.Add(objRecommendations.Name);
                }
            }
            String[] Id = lstRecom.ToArray();
            var str = String.Join(",", Id);
            return str;
        }
        public string getCause(List<string> lstCause)
        {
            var _repoRootCause = new MongoGenericRepository<RootCause>(_dbsetting);
            List<string> lstRootCause = new List<string>();
            foreach (var causeId in lstCause)
            {
                var objRootCause = _repoRootCause.GetFirst(p => p.Id == causeId);
                if (objRootCause != null)
                {
                    lstRootCause.Add(objRootCause.Name);
                }
            }
            String[] Id = lstRootCause.ToArray();
            var str = String.Join(",", Id);
            return str;
        }
        public string GetAuditTeam(List<AuditResource> resources)
        {
            var sBuilder = new StringBuilder();
            foreach (var resource in resources)
            {
                if (resource.User != null)
                {
                    var resName = resource.User.FirstName + " " + resource.User.LastName + " (" + resource.User.Designation + ") ";
                    //sBuilder.Append(", ");
                    sBuilder.Append(resName);
                }
            }
            return sBuilder.ToString();
        }
        public string GetAuditeeName(List<Auditees> auditees)
        {
            var sBuilder = new StringBuilder();
            foreach (var auditee in auditees)
            {
                var resName = auditee.User.FirstName + " " + auditee.User.LastName + " (" + auditee.User.Designation + ")";
                sBuilder.Append(resName);
            }
            return sBuilder.ToString();
        }
        public string GetAuditApprovers(AuditApprovalMapping approvers)
        {
            var sBuilder = new StringBuilder();

            foreach (var approver in approvers.UserData)
            {
                if (approver.User != null)
                {
                    var resName = approver.User.FirstName + " " + approver.User.LastName + " (" + approver.Responsibility + ") ";
                    //sBuilder.Append(", ");
                    sBuilder.Append(resName);
                }
            }

            return sBuilder.ToString();
        }
        public ScopeAndSchedule populateScopeAndSchedule(ScopeAndSchedule item)
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
            return item;
        }
        public string GetCurrentFinancialYearByDate(DateTime startDate, DateTime endDate)
        {
            int CurrentYear = startDate.Year;
            int PreviousYear = startDate.Year - 1;
            int NextYear = endDate.Year;
            string PreYear = PreviousYear.ToString();
            string NexYear = NextYear.ToString();
            string CurYear = CurrentYear.ToString();
            string FinYear = null;

            if (startDate.Month > 3)
                FinYear = CurYear + "-" + NexYear;
            else
                FinYear = PreYear + "-" + CurYear;
            return FinYear.Trim();
        }
        public bool IsLastRowEmpty(ExcelWorksheet worksheet)
        {
            var empties = new List<bool>();
            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
            {
                var rowEmpty = worksheet.Cells[worksheet.Dimension.End.Row, i].Value == null ? true : false;
                empties.Add(rowEmpty);
            }
            return empties.All(e => e);
        }
        public List<string> getControlFrequency() {
            var lst = new List<string>();
            lst.Add("Event Driven");
            lst.Add("Annual");
            lst.Add("Semi Annual");
            lst.Add("Quarterly");
            lst.Add("Monthly");
            lst.Add("Weekly");
            lst.Add("Daily"); 
            return lst;
        }
        public List<string> getControlNature()
        {
            var lst = new List<string>();
            lst.Add("Automated");
            lst.Add("Manual");
            lst.Add("IT Dependent"); 
            return lst;
        }
        public List<string> getTorStatus()
        {
            var lst = new List<string>();
            lst.Add("Partially Received");
            lst.Add("Pending");
            lst.Add("Received");
            return lst;
        }
        public List<string> getControlType()
        {
            var lst = new List<string>();
            lst.Add("Financial");
            lst.Add("Operational");
            lst.Add("Financial Reporting");
            return lst;
        }

        public string getLocation(List<string> lstLoc)
        {
            try
            {
                var _repoLocation = new MongoGenericRepository<Location>(_dbsetting);
                List<string> lstLocation = new List<string>();
                foreach (var LocationId in lstLoc)
                {
                    var objLoc = _repoLocation.GetFirst(p => p.Id == LocationId);
                    if (objLoc != null)
                    {
                        lstLocation.Add(objLoc.LocationDescription);
                    }
                }
                String[] Id = lstLocation.ToArray();
                var str = String.Join(",", Id);
                return str;
            }
            catch (Exception ex)
            {
                ActivityLog(ex.Message, ex.InnerException.ToString(), ex.Source, ex.HResult.ToString(), ex.HelpLink, ex.StackTrace);
                throw;
            }
        }
        public void ActivityLog(string UserId, string ReferenceId, string Reference, string Type, string Event, string History)
        {
            try
            {
                var _logRepo = new MongoGenericRepository<ActivityLog>(_dbsetting);
                var _userRepo = new MongoGenericRepository<User>(_dbsetting);
                var user = _userRepo.GetWithInclude<Role>(p => p.Id == UserId).FirstOrDefault();
                if (user != null)
                {
                    if (user.Role.Name.ToLower() == "admin")
                    {
                        ActivityLog objActivityLog = new ActivityLog();
                        objActivityLog.UserId = UserId;
                        objActivityLog.ReferenceId = ReferenceId;
                        objActivityLog.Reference = Reference;
                        objActivityLog.Type = Type;
                        objActivityLog.Event = Event;
                        objActivityLog.History = History;
                        objActivityLog.LogDate = DateTime.Now;
                        _logRepo.Insert(objActivityLog);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void ServiceLog(string serviceName, string serviceDescription, string error, string? stackTrace, string methodName)
        {
            var _log = new MongoGenericRepository<ServiceLog>(_dbsetting);
            ServiceLog objBackgroundServiceLog = new ServiceLog();
            objBackgroundServiceLog.ServiceName = serviceName;
            objBackgroundServiceLog.ServiceDescription = serviceDescription;
            objBackgroundServiceLog.ExceptionMsg = error;
            objBackgroundServiceLog.ExceptionSource = stackTrace;
            objBackgroundServiceLog.MethodName = methodName;
            objBackgroundServiceLog.LogDate = DateTime.Now;
            _log.Insert(objBackgroundServiceLog);
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

        public void InsertNotification(string message, string module, bool isReadable, string referenceId, string status, string discussNumber, string userId)
        {
            var _repoNotification = new MongoGenericRepository<Notification>(_dbsetting);
            var repoUser = new MongoGenericRepository<User>(_dbsetting);
            var objUser = repoUser.GetFirst(p => p.Id == userId);
            switch (status)
            {
                case AuditConstants.Status.INPROGRESS:
                    status = AuditConstants.CommonStatus.REJECTED; break;
                case AuditConstants.Status.COMPLETED:
                    status = AuditConstants.CommonStatus.APPROVED; break;
                case AuditConstants.CommonStatus.SAVETODRAFT:
                    status = AuditConstants.CommonStatus.SAVETODRAFT; break;
                default:
                    status = ""; break;
            }
            Notification objNotification = new Notification();
            objNotification.Message = message;

            objNotification.Module = module;
            objNotification.IsReadable = isReadable;
            objNotification.ReferenceId = referenceId;
            objNotification.Status = status;
            objNotification.DiscussionNumber = discussNumber;
            objNotification.UserId = objUser != null ? objUser.Id : "";
            objNotification.UserName = objUser != null ? objUser.FirstName + " " + objUser.LastName : "";
            _repoNotification.Insert(objNotification);
        }
        // Generate a random number between two numbers    
        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        // Generate a random string with a given size and case.   
        // If second parameter is true, the return string is lowercase  
        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
        public string RandomPassword(int size = 0)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(RandomString(4, true));
            builder.Append(RandomNumber(1000, 9999));
            builder.Append(RandomString(2, false));
            return builder.ToString();
        }
        public class FileContentByte
        {
            public byte[] FileContents { get; set; }
            public string ContentType { get; set; }
            public string FileDownloadName { get; set; }
        }
        public FileContentByte DownloadExcelAttachment(List<FollowUp> tList)
        {
            var rootcauseRepo = new MongoGenericRepository<RootCause>(_dbsetting);
            var risktypeRepo = new MongoGenericRepository<RiskType>(_dbsetting);
            var userRepo = new MongoGenericRepository<User>(_dbsetting);
            var auditRepo = new MongoGenericRepository<Audit>(_dbsetting);
            var scopeAndScheduleRepo = new MongoGenericRepository<ScopeAndSchedule>(_dbsetting);
            var processLocationMappingRepo = new MongoGenericRepository<ProcessLocationMapping>(_dbsetting);
            var locationRepo = new MongoGenericRepository<Location>(_dbsetting);
            var followupActionPlanRepo = new MongoGenericRepository<FollowupActionPlan>(_dbsetting);

            var fileName = "ActionPlans.xlsx";
            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Production + Factory Operations");

                Color green = ColorTranslator.FromHtml("#92D050");
                Color yellow = ColorTranslator.FromHtml("#FFC001");
                Color blue = ColorTranslator.FromHtml("#D9D9D9");

                worksheet.Cells["A1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["A1"].Value = "Audit Number";
                worksheet.Cells["B1"].Value = "Review Area";
                worksheet.Cells["C1"].Value = "Plant";
                worksheet.Cells["D1"].Value = "Audit Exists";
                worksheet.Cells["E1"].Value = "Review Qtr";
                worksheet.Cells["F1"].Value = "Obs Number";
                worksheet.Cells["G1"].Value = "Observation Heading";

                worksheet.Cells["H1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["H1:I1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["H1"].Value = "Observation Description";
                worksheet.Cells["I1"].Value = "Management response";

                worksheet.Cells["J1:K1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["J1:K1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["J1"].Value = "Agreed action plan";
                worksheet.Cells["K1"].Value = "Risk Rating";

                worksheet.Cells["L1:M1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["L1:M1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["L1"].Value = "Risk Source";
                worksheet.Cells["M1"].Value = "Responsibility (Department)";

                worksheet.Cells["N1:O1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["N1:O1"].Style.Fill.BackgroundColor.SetColor(green);
                worksheet.Cells["N1"].Value = "Responsibility (Person)";
                worksheet.Cells["O1"].Value = "Agreed Timeline";

                worksheet.Cells["P1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["P1"].Style.Fill.BackgroundColor.SetColor(blue);
                worksheet.Cells["P1"].Value = "Implementation Remarks";

                worksheet.Cells["Q1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["Q1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells["Q1"].Value = "Status";

                var rowIndex = 2;

                foreach (var plan in tList)
                {
                    var followActionPlans = followupActionPlanRepo.GetMany(a => a.FollowupId == plan.Id);
                    if (followActionPlans != null)
                    {
                        if (plan.AuditExist && plan.AuditId != null)
                        {
                            var audit = scopeAndScheduleRepo.GetWithInclude<Location, Audit>(x => x.AuditId == plan.AuditId).FirstOrDefault();
                            if (audit != null)
                            {
                                worksheet.Cells["A" + rowIndex.ToString()].Value = audit.AuditNumber;
                                worksheet.Cells["B" + rowIndex.ToString()].Value = audit.Audit.AuditName;
                            }
                            if (audit.LocationId != null)
                                worksheet.Cells["C" + rowIndex.ToString()].Value = audit.Location.DivisionDescription;
                        }
                        else
                        {
                            worksheet.Cells["A" + rowIndex.ToString()].Value = plan.AuditNumber;
                            worksheet.Cells["B" + rowIndex.ToString()].Value = plan.AuditName;
                            if (plan.LocationID != null)
                            {
                                var objLocation = locationRepo.GetFirst(p => p.Id == plan.LocationID);
                                worksheet.Cells["C" + rowIndex.ToString()].Value = objLocation != null ? objLocation.LocationDescription:"";
                            }
                        }
                        worksheet.Cells["D" + rowIndex.ToString()].Value = plan.AuditExist;
                        worksheet.Cells["E" + rowIndex.ToString()].Value = plan.ReviewQtr;
                        worksheet.Cells["F" + rowIndex.ToString()].Value = plan.ObsNumber;


                        worksheet.Cells["G" + rowIndex.ToString()].Value = plan.ObservationHeading;
                        worksheet.Cells["H" + rowIndex.ToString()].Value = plan.DetailedObservation != null ? UtilityMethods.HtmlToText(plan.DetailedObservation) : null;
                        worksheet.Cells["I" + rowIndex.ToString()].Value = plan.ManagementResponse != null ? UtilityMethods.HtmlToText(plan.ManagementResponse) : null;
                        worksheet.Cells["J" + rowIndex.ToString()].Value = plan.AgreedActionPlan != null ? UtilityMethods.HtmlToText(plan.AgreedActionPlan) : null;
                        worksheet.Cells["K" + rowIndex.ToString()].Value = plan.ObservationGrading;

                        if (plan.RiskTypeId != null)
                        {
                            var riskType = risktypeRepo.GetByID(plan.RiskTypeId);

                            if (riskType != null)
                                worksheet.Cells["L" + rowIndex.ToString()].Value = riskType.Name;
                        }
                        worksheet.Cells["M" + rowIndex.ToString()].Value = plan.ResponsibilityDepartment;
                        if (plan.ImplementationOwnerId != null)
                        {
                            var owner = userRepo.GetByID(plan.ImplementationOwnerId);

                            if (owner != null)
                                worksheet.Cells["N" + rowIndex.ToString()].Value = owner.FirstName + " " + owner.LastName;
                        }
                        if (plan.ImplementationEndDate != null)
                        {
                            worksheet.Cells["O" + rowIndex.ToString()].Style.Numberformat.Format = "dd-mm-yyyy";
                            worksheet.Cells["O" + rowIndex.ToString()].Value = Convert.ToDateTime(plan.ImplementationEndDate);
                        }
                        worksheet.Cells["P" + rowIndex.ToString()].Value = plan.ImplementationRemarks;
                        worksheet.Cells["Q" + rowIndex.ToString()].Value = plan.Status;
                        rowIndex++;
                    }
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                worksheet.Column(2).Style.WrapText = true;
                worksheet.Column(3).Style.WrapText = true;
                package.Save();
            }
            byte[] bytes = memoryStream.ToArray();

            FileContentByte objFileContentByt = new FileContentByte()
            {
                FileContents = bytes,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileDownloadName = fileName,
            };

            return objFileContentByt;
        }
    }
}