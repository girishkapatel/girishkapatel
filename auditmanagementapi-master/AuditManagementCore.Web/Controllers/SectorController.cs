using AuditManagementCore.Models;
using AuditManagementCore.MongoDb;
using AuditManagementCore.Service;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AuditManagementCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectorController : VJBaseGenericAPIController<Sector>
    {
        IMongoDbSettings _dbsetting;
        CommonServices _CommonServices;
        public SectorController(IMongoGenericRepository<Sector> api, IMongoDbSettings mongoDbSettings, CommonServices cs) : base(api)
        {
            _dbsetting = mongoDbSettings;
            _CommonServices = cs;
        }
        public override ActionResult Post([FromBody] Sector e)
        {
            var isExist = _api.Exists(x => x.Name.ToLower() == e.Name.ToLower());
            if (isExist)
            {
                return ResponseError("Sector with name : " + e.Name + " already exists.");
            }
            var sector = base.Post(e);
            //Activity Log
            _CommonServices.ActivityLog(e.CreatedBy, e.Id, e.Name, "Sector", "Master | Sector | Add", "Added Sector");
            return sector;
        }

        public override ActionResult Put([FromBody] Sector e)
        {
            if (e == null) return ResponseBad("Sector object is null");

            var sector = _api.GetFirst(x => x.Id == e.Id);

            if (sector == null) return ResponseError("Sector does not exists");

            sector.Name = e.Name;
            sector.UpdatedBy = e.UpdatedBy;

            _api.Update(sector);
            //Activity Log
            _CommonServices.ActivityLog(e.UpdatedBy, e.Id, e.Name, "Sector", "Master | Sector | Edit", "Updated Sector");
            return ResponseOK(e);
        }

        public override ActionResult Delete(string id, string userid)
        {
            try
            {
                var _repoLocation = new MongoGenericRepository<Location>(_dbsetting);
                var _repoTrialBalance = new MongoGenericRepository<TrialBalance>(_dbsetting);
                if (id == null) return ResponseBad("Sector object is null");
                var Sector = _api.GetFirst(x => x.Id == id);
                if (Sector == null)
                    return ResponseError("Sector does not exists");

                var loca = _repoLocation.GetFirst(x => x.Sector == id);
                var trialBalance = _repoTrialBalance.GetFirst(x => x.SectorId == id);
                if (loca != null || trialBalance != null)
                    return CustomResponseError("");
                _api.Delete(id);
                //Activity Log
                _CommonServices.ActivityLog(userid, id, Sector.Name, "Sector", "Master | Sector | Delete", "Deleted Sector");
            }
            catch (Exception)
            {
                ResponseBad("Please Contact to Administator");
            }
            return ResponseSuccess(new JsonResult(id));
        }

        [HttpGet("downloadexcel")]
        public IActionResult DownloadExcel()
        {
            var sectors = _CommonServices.GetAllSectors();
            var businessCycles = _CommonServices.GetAllBusinessCycles();
            var processL1List = _CommonServices.GetAllProcessL1();
            var processL2List = _CommonServices.GetAllProcessL2();

            var memoryStream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                #region Add worksheet for Sectors
                ExcelWorksheet sectorSheet = package.Workbook.Worksheets.Add("Sectors");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");

                sectorSheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                sectorSheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sectorSheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                sectorSheet.Cells["A1"].Value = "Name *";

                var rowIndex = 2;
                foreach (var sector in sectors)
                {
                    sectorSheet.Cells["A" + rowIndex.ToString()].Value = sector.Name;
                    rowIndex++;
                }

                sectorSheet.Cells[sectorSheet.Dimension.Address].AutoFitColumns();
                sectorSheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Add worksheet for Business Cycles
                ExcelWorksheet businessCycleSheet = package.Workbook.Worksheets.Add("Business Cycle");
                businessCycleSheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                businessCycleSheet.Cells["A1"].Value = "Name*";
                businessCycleSheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                businessCycleSheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                rowIndex = 2;
                foreach (var businessCycle in businessCycles)
                {
                    businessCycleSheet.Cells["A" + rowIndex.ToString()].Value = businessCycle.Name;
                    rowIndex++;
                }

                businessCycleSheet.Cells[businessCycleSheet.Dimension.Address].AutoFitColumns();
                businessCycleSheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Add worksheet for Process L1
                ExcelWorksheet processL1Sheet = package.Workbook.Worksheets.Add("Process L1");
                processL1Sheet.Cells["A1:B1"].Style.Font.Color.SetColor(Color.Red);
                processL1Sheet.Cells["A1"].Value = "Business Cycle*";
                processL1Sheet.Cells["B1"].Value = "Name*";
                processL1Sheet.Cells["A1:B1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                processL1Sheet.Cells["A1:B1"].Style.Fill.BackgroundColor.SetColor(yellow);
                rowIndex = 2;
                foreach (var processL1 in processL1List)
                {
                    processL1Sheet.Cells["A" + rowIndex.ToString()].Value = processL1.BusinessCycle.Name;
                    processL1Sheet.Cells["B" + rowIndex.ToString()].Value = processL1.Name;
                    rowIndex++;
                }

                processL1Sheet.Cells[processL1Sheet.Dimension.Address].AutoFitColumns();
                processL1Sheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Add worksheet for Process L2
                ExcelWorksheet processL2Sheet = package.Workbook.Worksheets.Add("Process L2");
                processL2Sheet.Cells["A1:C1"].Style.Font.Color.SetColor(Color.Red);
                processL2Sheet.Cells["A1:C1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                processL2Sheet.Cells["A1:C1"].Style.Fill.BackgroundColor.SetColor(yellow);

                processL2Sheet.Cells["A1"].Value = "Business Cycle*";
                processL2Sheet.Cells["B1"].Value = "Process L1*";
                processL2Sheet.Cells["C1"].Value = "Name*";
                processL2Sheet.Cells["D1"].Value = "Process Model";

                rowIndex = 2;
                foreach (var processL2 in processL2List)
                {
                    processL2Sheet.Cells["A" + rowIndex.ToString()].Value = businessCycles.FirstOrDefault(a => a.Id == processL2.BusinessCycleId).Name;
                    processL2Sheet.Cells["B" + rowIndex.ToString()].Value = processL2.ProcessL1.Name;
                    processL2Sheet.Cells["C" + rowIndex.ToString()].Value = processL2.Name;
                    processL2Sheet.Cells["D" + rowIndex.ToString()].Value = processL2.ProcessModel;
                    rowIndex++;
                }

                processL2Sheet.Cells[processL2Sheet.Dimension.Address].AutoFitColumns();
                processL2Sheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion
                package.Save();
            }
            memoryStream.Position = 0;
            var fileName = "ProcessMaster.xlsx";
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpGet("sampledownloadexcel")]
        public IActionResult SampleDownloadExcel()
        {
            var memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                #region Add worksheet for Sectors
                ExcelWorksheet sectorSheet = package.Workbook.Worksheets.Add("Sectors");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");

                sectorSheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                sectorSheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sectorSheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                sectorSheet.Cells["A1"].Value = "Name *";
                sectorSheet.Cells[sectorSheet.Dimension.Address].AutoFitColumns();
                sectorSheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Add worksheet for Business Cycles
                ExcelWorksheet businessCycleSheet = package.Workbook.Worksheets.Add("Business Cycle");
                businessCycleSheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
                businessCycleSheet.Cells["A1"].Value = "Name*";
                businessCycleSheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                businessCycleSheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(yellow);
                businessCycleSheet.Cells[businessCycleSheet.Dimension.Address].AutoFitColumns();
                businessCycleSheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Add worksheet for Process L1
                ExcelWorksheet processL1Sheet = package.Workbook.Worksheets.Add("Process L1");
                processL1Sheet.Cells["A1:B1"].Style.Font.Color.SetColor(Color.Red);
                processL1Sheet.Cells["A1"].Value = "Business Cycle*";
                processL1Sheet.Cells["B1"].Value = "Name*";
                processL1Sheet.Cells["A1:B1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                processL1Sheet.Cells["A1:B1"].Style.Fill.BackgroundColor.SetColor(yellow);
                processL1Sheet.Cells[processL1Sheet.Dimension.Address].AutoFitColumns();
                processL1Sheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion

                #region Add worksheet for Process L2
                ExcelWorksheet processL2Sheet = package.Workbook.Worksheets.Add("Process L2");
                processL2Sheet.Cells["A1:C1"].Style.Font.Color.SetColor(Color.Red);
                processL2Sheet.Cells["A1:C1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                processL2Sheet.Cells["A1:C1"].Style.Fill.BackgroundColor.SetColor(yellow);

                processL2Sheet.Cells["A1"].Value = "Business Cycle*";
                processL2Sheet.Cells["B1"].Value = "Process L1*";
                processL2Sheet.Cells["C1"].Value = "Name*";
                processL2Sheet.Cells["D1"].Value = "Process Model";
                processL2Sheet.Cells[processL2Sheet.Dimension.Address].AutoFitColumns();
                processL2Sheet.Cells["A1:XFD1"].Style.Font.Bold = true;
                #endregion
                package.Save();
            }
            memoryStream.Position = 0;
            var fileName = "ProcessMaster.xlsx";
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("importexcel")]
        public ActionResult ImportExcel()
        {
            try
            {
                if (Request.Form.Files == null || Request.Form.Files.Count() <= 0)
                {
                    return ResponseError("formfile is empty");
                }
                int SectorExceptionrowCount = 0;
                int SectorTotalRow = 0;
                StringBuilder sbSector = new StringBuilder();

                int BusinessCycleExceptionrowCount = 0;
                int BusinessCycleTotalRow = 0;
                StringBuilder sbBusinessCycle = new StringBuilder();

                int P1ExceptionrowCount = 0;
                int P1TotalRow = 0;
                StringBuilder sbP1 = new StringBuilder();

                int P2ExceptionrowCount = 0;
                int P2TotalRow = 0;
                StringBuilder sbP2 = new StringBuilder();
                var file = Request.Form.Files[0];

                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return ResponseError("Not Support file extension");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        #region Import process L2
                        var processL2List = new List<ProcessL2>();

                        ExcelWorksheet processL2Sheet = package.Workbook.Worksheets["Process L2"];
                        var rowCount = processL2Sheet != null ? processL2Sheet.Dimension.Rows : 0;
                        P2TotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var businessCycleName = Convert.ToString(processL2Sheet.Cells[row, 1].Value).Trim();
                                var businessCycleId = _CommonServices.InsertBusinessCycle(new BusinessCycle { Name = businessCycleName });

                                var processL1Name = Convert.ToString(processL2Sheet.Cells[row, 2].Value).Trim();
                                var processL1Id = _CommonServices.InsertProcessL1(new ProcessL1 { BusinessCycleId = businessCycleId, Name = processL1Name });

                                if (Convert.ToString(processL2Sheet.Cells[row, 3].Value).Trim() != "")
                                    processL2List.Add(new ProcessL2
                                    {
                                        BusinessCycleId = businessCycleId,
                                        ProcessL1Id = processL1Id,
                                        Name = Convert.ToString(processL2Sheet.Cells[row, 3].Value).Trim(),
                                        ProcessModel = Convert.ToString(processL2Sheet.Cells[row, 4].Value).Trim()
                                    });
                            }
                            catch (Exception e)
                            {
                                P2ExceptionrowCount++;
                                sbP2.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "Sector/ImportExcel()/Process L2");
                            }
                        }

                        foreach (var pL2 in processL2List)
                        {
                            _CommonServices.InsertProcessL2(pL2);
                        }
                        #endregion

                        #region Import process L1
                        var processL1List = new List<ProcessL1>();

                        ExcelWorksheet processL1Sheet = package.Workbook.Worksheets["Process L1"];
                        rowCount = processL1Sheet != null ? processL1Sheet.Dimension.Rows : 0;
                        P1TotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var businessCycleName = Convert.ToString(processL1Sheet.Cells[row, 1].Value).Trim();
                                var businessCycleId = _CommonServices.InsertBusinessCycle(new BusinessCycle { Name = businessCycleName });

                                if (Convert.ToString(processL1Sheet.Cells[row, 2].Value).Trim() != "")
                                    processL1List.Add(new ProcessL1
                                    {
                                        BusinessCycleId = businessCycleId,
                                        Name = Convert.ToString(processL1Sheet.Cells[row, 2].Value).Trim()
                                    });
                            }
                            catch (Exception e)
                            {
                                P1ExceptionrowCount++;
                                sbP1.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "Sector/ImportExcel()/Process L1");
                            }
                        }

                        foreach (var pL1 in processL1List)
                        {
                            _CommonServices.InsertProcessL1(pL1);
                        }
                        #endregion

                        #region Import business cycles
                        var businessCyclesList = new List<BusinessCycle>();

                        ExcelWorksheet businessCyclesSheet = package.Workbook.Worksheets["Business Cycle"];
                        rowCount = businessCyclesSheet != null ? businessCyclesSheet.Dimension.Rows : 0;
                        BusinessCycleTotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                if (Convert.ToString(businessCyclesSheet.Cells[row, 1].Value).Trim() != "")
                                    businessCyclesList.Add(new BusinessCycle
                                    {
                                        Name = Convert.ToString(businessCyclesSheet.Cells[row, 1].Value).Trim()
                                    });
                            }
                            catch (Exception e)
                            {
                                BusinessCycleExceptionrowCount++;
                                sbBusinessCycle.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "Sector/ImportExcel()/Business Cycle");
                            }
                        }

                        foreach (var b in businessCyclesList)
                        {
                            _CommonServices.InsertBusinessCycle(b);
                        }
                        #endregion

                        #region Import sectors
                        var sectorsList = new List<Sector>();

                        ExcelWorksheet sectorsSheet = package.Workbook.Worksheets["Sectors"];
                        rowCount = sectorsSheet != null ? sectorsSheet.Dimension.Rows : 0;
                        SectorTotalRow = rowCount;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                if (Convert.ToString(sectorsSheet.Cells[row, 1].Value).Trim() != "")
                                    sectorsList.Add(new Sector
                                    {
                                        Name = Convert.ToString(sectorsSheet.Cells[row, 1].Value).Trim()
                                    });
                            }
                            catch (Exception e)
                            {
                                SectorExceptionrowCount++;
                                sbSector.Append(row + ",");
                                _CommonServices.SendExcepToDB(e, "Sector/ImportExcel()/Sectors");
                            }
                        }

                        foreach (var s in sectorsList)
                        {
                            if (!_api.Exists(x => x.Name == s.Name))
                                _api.Insert(s);
                        }
                        #endregion
                    }
                }

                var Processmaster = new
                {
                    SectionExcptionCount = SectorExceptionrowCount,
                    SectionExcptionRowNumber = sbSector.ToString(),
                    SectionTotalRow = SectorTotalRow - 1,

                    BusinessCycleExcptionCount = BusinessCycleExceptionrowCount,
                    BusinessCyclExcptionRowNumber = sbBusinessCycle.ToString(),
                    BusinessCyclTotalRow = BusinessCycleTotalRow - 1,

                    ProcessL1ExcptionCount = P1ExceptionrowCount,
                    ProcessL1ExcptionRowNumber = sbP1.ToString(),
                    ProcessL1TotalRow = P1TotalRow - 1,

                    ProcessL2ExcptionCount = P2ExceptionrowCount,
                    ProcessL2ExcptionRowNumber = sbP2.ToString(),
                    ProcessL2TotalRow = P2TotalRow - 1,
                };
                return ResponseOK(Processmaster);
            }
            catch (Exception e)
            {
                _CommonServices.SendExcepToDB(e, "Sector/ImportExcel()");
            }
            return ResponseOK(new object[0]);
        }
    }
}