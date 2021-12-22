using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Text;

using Excel = Microsoft.Office.Interop.Excel;

namespace AuditManagementCore.Service.Utilities
{
    class ExcelUtility
    {
        public void createListRows()
        {
            string excelFilePath = "excel.xlsx";
            List<List<string>> rows = new List<List<string>>();
            List<string> row = new List<string>();
            row.Add("2349");
            row.Add("The Prime Time of Your Life");
            // etc
            rows.Add(row);
            convertRowsToExcel(rows, excelFilePath);
        }

        public void convertRowsToExcel(List<List<string>> matrix, string excelFilePath)
        {
            var xlApp = new Application();
            var xlWorkBook = xlApp.Workbooks.Add();
            var xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.Item[1];

            for (int i = 0; i < matrix.Count; i++)
            {
                for (int j = 0; j < matrix[i].Count; j++)
                {
                    xlWorkSheet.Cells[j + 1, i + 1] = matrix[i][j];
                }
            }

            xlWorkBook.SaveAs(excelFilePath);
            xlWorkBook.Close(true);
            xlApp.Quit();
        }
    }
}
