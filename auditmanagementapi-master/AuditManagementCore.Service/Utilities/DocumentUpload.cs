using LumenWorks.Framework.IO.Csv;
using Microsoft.AspNetCore.Http;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace AuditManagementCore.Service.Utilities
{
    public class DocumentUpload : IDocumentUpload
    {
        #region Class Properties Declarations
        public bool PreserveFileName { get; set; }

        public string FileLocation { get; set; }
        #endregion

        public DocumentUpload(string FileLocation)
        {
            this.PreserveFileName = false;
            this.FileLocation = FileLocation;
        }

        public DocumentUpload(bool PreserveFileName, string FileLocation)
        {
            this.PreserveFileName = PreserveFileName;
            this.FileLocation = FileLocation;
        }

        public async Task<bool> IsExists(IFormFile file)
        {
            var savedPath = Path.Combine(FileLocation, file.FileName);
            return File.Exists(savedPath);
        }

        public bool IsExists(string filePath)
        {
            var savedPath = Path.Combine(FileLocation, filePath);
            return File.Exists(savedPath);
        }

        public async Task<string> Upload(IFormFile file)
        {
            string savedPath = string.Empty;
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                if (!PreserveFileName)
                {
                    fileName = DateTime.Now.Ticks + extension; //Create a new Name for the file due to security reasons.
                }
                else
                {
                    fileName = file.FileName;
                }

                if (!Directory.Exists(FileLocation))
                {
                    Directory.CreateDirectory(FileLocation);
                }

                savedPath = Path.Combine(FileLocation, fileName);

                using (var stream = new FileStream(savedPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

            }
            catch (Exception e)
            {
                //log error
            }

            return savedPath;
        }

        public async Task<string> UploadToWebRoot(IFormFile file, string location)
        {
            string savedPath = string.Empty;
            string fileName;

            try
            {
                string newPath = Path.Combine(FileLocation, location);

                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];

                if (!PreserveFileName)
                    fileName = DateTime.Now.Ticks + extension; //Create a new Name for the file due to security reasons.
                else
                    fileName = file.FileName;

                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);

                //fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                savedPath = Path.Combine(newPath, fileName);

                using (var stream = new FileStream(savedPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception e)
            {
                //log error
            }

            return savedPath;
        }

        public DataTable FileToDataTable(IFormFile file)
        {
            var res = this.Upload(file);
            res.Wait();

            DataTable csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(res.Result)), true))
            {
                csvTable.Load(csvReader);
            }
            return csvTable;
        }

        public void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                // MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        public void DeleteFile(string location)
        {
            string newPath = Path.Combine(FileLocation, location);
            File.Delete(newPath);
        }

        public void DeleteFullPath(string fullPath)
        {
            File.Delete(fullPath);
        }

        //public MemoryStream GetMemoryStream(string path, string fileName)
        //{
        //    var memory = new MemoryStream();
        //    using (var stream = new FileStream(path, FileMode.Open))
        //    {
        //        stream.CopyToAsync(memory);
        //    }
        //    memory.Position = 0;

        //    return memory;
        //}
    }
}