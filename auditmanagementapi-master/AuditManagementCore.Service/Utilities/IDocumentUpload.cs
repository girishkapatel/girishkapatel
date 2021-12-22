using Microsoft.AspNetCore.Http;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace AuditManagementCore.Service.Utilities
{
    public interface IDocumentUpload
    {
        bool PreserveFileName { get; set; }

        string FileLocation { get; set; }

        public Task<string> Upload(IFormFile file);

        public Task<string> UploadToWebRoot(IFormFile file, string location);

        public Task<bool> IsExists(IFormFile file);

        public bool IsExists(string location);

        public DataTable FileToDataTable(IFormFile file);

        public void ReleaseObject(Object obj);

        public void DeleteFile(string location);

        public void DeleteFullPath(string fullPath);

        //MemoryStream GetMemoryStream(string path, string fileName);
    }
}