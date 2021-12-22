using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using AuditManagementCore.Service.Utilities;
using AuditManagementCore.ViewModels;
using LumenWorks.Framework.IO.Csv;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportCountryStateCityController : VJBaseGenericAPIController<Country>
    {
        IMongoDbSettings _dbsetting;
        private readonly IDocumentUpload _docUpload;
        CommonServices _CommonServices;
        public ImportCountryStateCityController(IMongoGenericRepository<Country> api, IMongoDbSettings mongoDbSettings, IDocumentUpload documentUpload, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _docUpload = documentUpload;
            _CommonServices = cs;
        }

        [HttpPost("PostCountry")]
        public ActionResult PostCountry(IFormFile file)  //File template : Country
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            csvTable = _docUpload.FileToDataTable(file);

            List<Country> countries = new List<Country>();

            var CountryRepo = new MongoGenericRepository<Country>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString()))
                {
                    countries.Add(new Country() { Name = csvTable.Rows[i][0].ToString() });
                }
            }
            foreach (var c in countries)
            {
                if (!CountryRepo.Exists(x => x.Name == c.Name))
                {
                    CountryRepo.Insert(c);
                }
            }
           _docUpload.ReleaseObject(csvTable);
            return ResponseOK(countries);
        }

        [HttpPost("PostState")]
        public ActionResult PostState(IFormFile file)  //File template : Country,State
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            //csvTable = FileToDataTable(file, out csvTable);
            csvTable =  _docUpload.FileToDataTable(file);

            List<ImportCountryStateCity> import = new List<ImportCountryStateCity>();

            var CountryRepo = new MongoGenericRepository<Country>(_dbsetting);
            var StateRepo = new MongoGenericRepository<State>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString()) && !String.IsNullOrWhiteSpace(csvTable.Rows[i][1].ToString()))
                {
                    import.Add(new ImportCountryStateCity()
                    {
                        Country = csvTable.Rows[i][0].ToString(),
                        State = csvTable.Rows[i][1].ToString()

                    });
                }
            }


            foreach (var c in import)
            {
                var country = new Country();
                var st = new State();
                if (!StateRepo.Exists(x => x.Name == c.State && x.Country.Name == c.Country))
                {
                    if (!CountryRepo.Exists(x => x.Name == c.Country))
                    {
                        country.Name = c.Country;
                        CountryRepo.Insert(country);
                    }

                    st.Country = CountryRepo.GetFirst(x => x.Name == c.Country);
                    st.CountryId = st.Country.Id;
                    st.Name = c.State;
                    StateRepo.Insert(st);
                }
            }
            _docUpload.ReleaseObject(csvTable);
            return Ok();
        }

        [HttpPost("PostCity")]
        public ActionResult PostCity(IFormFile file)  //File template : Country,State,City
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            csvTable = _docUpload.FileToDataTable(file);

            List<ImportCountryStateCity> import = new List<ImportCountryStateCity>();

            var CountryRepo = new MongoGenericRepository<Country>(_dbsetting);
            var StateRepo = new MongoGenericRepository<State>(_dbsetting);
            var CityRepo = new MongoGenericRepository<CityOrTown>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {

                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString())
                    && !String.IsNullOrWhiteSpace(csvTable.Rows[i][1].ToString())
                    && !String.IsNullOrWhiteSpace(csvTable.Rows[i][2].ToString()))
                {
                    import.Add(new ImportCountryStateCity()
                    {
                        Country = csvTable.Rows[i][0].ToString(),
                        State = csvTable.Rows[i][1].ToString(),
                        City = csvTable.Rows[i][2].ToString()
                    });
                }

            }


            foreach (var c in import)
            {
                var country = new Country();
                var city = new CityOrTown();
                var state = new State();

                if (!CityRepo.Exists(x => x.Name == c.City && x.Country.Name == c.Country && x.State.Name == c.State))
                {
                    if (!StateRepo.Exists(x => x.Name == c.State && x.Country.Name == c.Country))
                    {
                        if (!CountryRepo.Exists(x => x.Name == c.Country))
                        {
                            country.Name = c.Country;
                            CountryRepo.Insert(country);
                        }

                        state.Name = c.State;
                        StateRepo.Insert(state);
                    }

                    city.Country = CountryRepo.GetFirst(x => x.Name == c.Country);
                    city.CountryId = city.Country.Id;
                    city.State = StateRepo.GetFirst(x => x.Name == c.State);
                    city.StateId = city.State.Id;
                    city.Name = c.City;
                    CityRepo.Insert(city);
                }
            }
            _docUpload.ReleaseObject(csvTable);
            return Ok();
        }

        [HttpPost("PostCompany")]
        public ActionResult PostCompany(IFormFile file)  //File template : Company
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            //csvTable = FileToDataTable(file, out csvTable);
            csvTable = _docUpload.FileToDataTable(file);

            List<ImportCompany> import = new List<ImportCompany>();

            var CompanyRepo = new MongoGenericRepository<Company>(_dbsetting);
            var CountryRepo = new MongoGenericRepository<Country>(_dbsetting);
            var StateRepo = new MongoGenericRepository<State>(_dbsetting);
            var CityRepo = new MongoGenericRepository<CityOrTown>(_dbsetting);
            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString()) && !String.IsNullOrWhiteSpace(csvTable.Rows[i][1].ToString()))
                {
                    import.Add(new ImportCompany()
                    {
                        Company = csvTable.Rows[i][0].ToString(),
                        City = csvTable.Rows[i][1].ToString(),
                        //State = csvTable.Rows[i][1].ToString()
                        PanNo = csvTable.Rows[i][2].ToString(),
                        GSTNo = csvTable.Rows[i][3].ToString(),
                    });
                }
            }


            foreach (var c in import)
            {
                var company = new Company();
                //var country = new Country();
                //var st = new State();
                var city = new CityOrTown();

                if (!CompanyRepo.Exists(x => x.Name == c.Company && x.CityOrTown.Name == c.City))
                {
                    if (!CityRepo.Exists(x => x.Name == c.City))
                    {
                        city.Name = c.City;
                        CityRepo.Insert(city);
                    }

                    company.CityOrTown = CityRepo.GetFirst(x => x.Name == c.City);
                    company.CityId = company.CityOrTown.Id;
                    company.GSTNO = c.City;
                    company.PanNo = c.PanNo;
                    CompanyRepo.Insert(company);
                }
            }
            _docUpload.ReleaseObject(csvTable);
            return Ok();
        }

        [HttpPost("PostLocation")]
        public ActionResult PostLocation(IFormFile file)  //File template : Location
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            DataTable csvTable;
            //csvTable = FileToDataTable(file, out csvTable);
            csvTable = _docUpload.FileToDataTable(file);

            List<ImportLocation> import = new List<ImportLocation>();

            var CompanyRepo = new MongoGenericRepository<Company>(_dbsetting);
            var CountryRepo = new MongoGenericRepository<Country>(_dbsetting);
            var StateRepo = new MongoGenericRepository<State>(_dbsetting);
            var CityRepo = new MongoGenericRepository<CityOrTown>(_dbsetting);
            var LocationRepo = new MongoGenericRepository<Location>(_dbsetting);

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(csvTable.Rows[i][0].ToString()) && !String.IsNullOrWhiteSpace(csvTable.Rows[i][1].ToString()))
                {
                    import.Add(new ImportLocation()
                    {
                        Division = csvTable.Rows[i][0].ToString(),
                        DivisionDescription = csvTable.Rows[i][1].ToString(),
                        //State = csvTable.Rows[i][1].ToString()
                        RiskIndex = csvTable.Rows[i][2].ToString(),
                        LocationId = csvTable.Rows[i][3].ToString(),
                        City = csvTable.Rows[i][4].ToString(),
                        ProfitCenterCode = csvTable.Rows[i][5].ToString(),
                        CompanyName = csvTable.Rows[i][6].ToString()
                    });
                }
            }


            foreach (var c in import)
            {
                var location = new Location();
                var company = new Company();
                var country = new Country();
                var st = new State();
                var city = new CityOrTown();

                if (!LocationRepo.Exists((x => x.LocationID.ToLower() == c.LocationId.ToLower() && x.Division == c.Division))) //&& x.Sector == c.Sector 
                {
                    if (!CityRepo.Exists(x => x.Name == c.City))
                    {
                        //city.Name = c.City;
                        //CityRepo.Insert(city);
                        break;
                    }

                    if (!CompanyRepo.Exists(x => x.Name == c.CompanyName))
                    {
                        //city.Name = c.City;
                        //CityRepo.Insert(city);
                        break;
                    }

                    if (!CountryRepo.Exists(x => x.Name == c.Country))
                    {
                        //city.Name = c.City;
                        //CityRepo.Insert(city);
                        break;
                    }
                    //if (!StateRepo.Exists(x => x.Name == c.S))
                    //{
                        //city.Name = c.City;
                        //CityRepo.Insert(city);
                        //break;
                    //}

                    location.Division = c.Division;
                    location.DivisionDescription = c.DivisionDescription;

                    location.CityOrTown = CityRepo.GetFirst(x => x.Name == c.City);
                    location.CityId = location.CityOrTown.Id;

                    location.Company = CompanyRepo.GetFirst(x => x.Name == c.CompanyName);
                    location.CompanyID = location.Company.Id;

                    location.Country = CountryRepo.GetFirst(x => x.Name == c.Country);
                    location.CountryID = location.Country.Id;

                    location.RiskIndex = c.RiskIndex;
                    location.ProfitCenterCode = c.ProfitCenterCode;
                    location.LocationID = c.LocationId;

                    LocationRepo.Insert(location);
                }
            }
            _docUpload.ReleaseObject(csvTable);
            return Ok();
        }

    }
}