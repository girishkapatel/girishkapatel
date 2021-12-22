using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestingOfControlController : VJBaseGenericAPIController<TestingOfControl>
    {
        IMongoDbSettings _debsetting;
        private readonly IDocumentUpload _docUpload;
        public TestingOfControlController(IMongoGenericRepository<TestingOfControl> api, IMongoDbSettings mongoDbSettings, IDocumentUpload documentUpload) : base(api)
        {
            _debsetting = mongoDbSettings;
            _docUpload = documentUpload;
        }
        public override ActionResult Post([FromBody] TestingOfControl e)
        {
            var isExist = _api.Exists(x => x.RACMId == e.RACMId);
            if (isExist)
            {
                return ResponseError("Testing of control of this RACM is already exists.");
            }
            return base.Post(e);
        }

        public override ActionResult GetAll()
        {
            var tList = _api.GetAllWithInclude<RACM>();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            var userRepo = new MongoGenericRepository<User>(_debsetting);
            foreach (var item in tList)
            {
                item.PerformedBy = userRepo.GetByID(item.PerformedById);
                item.ControlOwner = userRepo.GetByID(item.ControlOwnerId);
            }
            return ResponseOK(tList);
        }

        [HttpPost("UploadFileTestingOfControl/{testingcontrolId}")]
        public async Task<IActionResult> UploadFileTestingOfControl(IFormFile[] files, string testingcontrolId)
        {
            //sample upload controller
            if (string.IsNullOrWhiteSpace(testingcontrolId))
                return ResponseError("Testing of control Id is missing");
            if (files == null || files.Count() == 0)
                return ResponseError("file not selected");
            if (files.Any(x => x.Length == 0))
                return ResponseError("Some file are curropted");
            var repo = new MongoGenericRepository<TestingOfControlUpload>(_debsetting);
            foreach (var item in files)
            {
                TestingOfControlUpload fm = new TestingOfControlUpload();
                var res = await _docUpload.Upload(item);
                fm.OriginalFileName = item.FileName;
                fm.UploadedDatetime = DateTime.Now;
                fm.UploadedFileName = res;
                fm.TestingOfCountrolId = testingcontrolId;
                repo.Insert(fm);
            }
            return Ok();
        }

        [HttpGet("DownloadFileTestingOfControl/{id}")]
        public async Task<IActionResult> UploadFileTestingOfControl(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ResponseError("Upload Id is missing");
            var repo = new MongoGenericRepository<TestingOfControlUpload>(_debsetting);
            var fileInfo = repo.GetByID(id);
            if (fileInfo == null)
                return ResponseError("file info not found");
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileInfo.UploadedFileName);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, "application/octet-stream", Path.GetFileName(path));
        }
        [HttpGet("GetTestingofControlHistory/{id}")]

        public ActionResult GetTestingofControlHistory(string id)
        {
            var repohistory = new MongoGenericRepository<TestingofControlHistory>(_debsetting);
            var tList = repohistory.GetWithInclude<RACMAuditProcedureDetails, User>(p => p.RACMAuditProcedureDetailsId == id);
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }
    }
}