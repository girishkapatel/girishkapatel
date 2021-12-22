using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VJLiabraries.GenericRepository;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CompanyController : VJBaseGenericAPIController<Company>
    {
        IMongoDbSettings _debsetting;
        CommonServices _CommonServices;
        public CompanyController(IMongoGenericRepository<Company> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _debsetting = mongoDbSettings;
            _CommonServices = cs;
        }

        public override ActionResult Post([FromBody] Company e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());
            if (isExist)
            {
                return ResponseError("Company Name is already Exists within State");
            }
            var Company = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "Company", "Master | Company | Add", "Added Company");
            return Company;
        }
        public override ActionResult Put([FromBody] Company e)
        {
            if (e == null) return ResponseBad("Company object is null");
            var company = _api.GetFirst(x => x.Id == e.Id);

            if (company == null)
            {
                return ResponseError("Company does not exists");
            }

            populateCompany(company, e);
            _api.Update(company);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "Company", "Master | Company | Edit", "Updated Company");
            return ResponseOK(e);
        }
        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoState = new MongoGenericRepository<State>(_debsetting);
                var _repoCity = new MongoGenericRepository<CityOrTown>(_debsetting);
                var _repoLocation = new MongoGenericRepository<Location>(_debsetting);
                var _repoCompany = new MongoGenericRepository<Company>(_debsetting);
                if (id == null) return ResponseBad("Company object is null");
                var company = _api.GetFirst(x => x.Id == id);
                if (company == null)
                    return ResponseError("Company does not exists");
                var loca = _repoLocation.GetFirst(x => x.CompanyID == id);
                if (loca != null)
                    return CustomResponseError("");

                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, company.Name, "Company", "Master | Company | Delete", "Deleted Company");
            }
            catch (Exception)
            {

                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }
        private void populateCompany(Company objCompany, Company e)
        {
            objCompany.Name = e.Name;
            objCompany.CountryId = e.CountryId;
            objCompany.StateId = e.StateId;
            objCompany.CityId = e.CityId;
            objCompany.PanNo = e.PanNo;
            objCompany.GSTNO = e.GSTNO;
            objCompany.AuditApprovalMapping = e.AuditApprovalMapping;
            objCompany.UpdatedBy = e.UpdatedBy;
        }

        [HttpGet("GetByState/{id}")]
        public ActionResult GetByState(string id)
        {
            var cities = _api.GetMany(x => x.StateId == id);
            return ResponseSuccess(cities);
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<Country, State, CityOrTown>();
            var userRepo = new MongoGenericRepository<User>(_debsetting);
            foreach (var item in tList)
            {
                item.AuditManager = userRepo.GetByID(item.AuditManagerId);
                item.Coordinator = userRepo.GetByID(item.CoordinatorId);
                item.HeadOfAudit = userRepo.GetByID(item.HeadOfAuditId);
                if (item.AuditApprovalMapping != null && item.AuditApprovalMapping.UserData != null) {
                    item.AuditApprovalMapping.UserData = populateUsers(item.AuditApprovalMapping.UserData);
                }
            }
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        public List<UserData> populateUsers(List<UserData> tList)
        {
            var userRepo = new MongoGenericRepository<User>(_debsetting);
            var users = new List<UserData>();

            foreach (var item in tList)
            {
                var user = userRepo.GetByID(item.UserId);
                item.User = user;
                users.Add(item);
            }
            return users;
        }
    }
}