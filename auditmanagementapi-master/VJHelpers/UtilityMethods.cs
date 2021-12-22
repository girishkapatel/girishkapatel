using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
namespace VJLiabraries
{
    public static class UtilityMethods
    {
        public static IEnumerable<string> ToCsv<T>(IEnumerable<T> objectlist, string separator = ",", bool header = true)
        {
            FieldInfo[] fields = typeof(T).GetFields();
            PropertyInfo[] properties = typeof(T).GetProperties();
            if (header)
            {
                yield return String.Join(separator, fields.Select(f => f.Name).Concat(properties.Select(p => p.Name)).ToArray());
            }
            foreach (var o in objectlist)
            {
                yield return string.Join(separator, fields.Select(f => (f.GetValue(o) ?? "").ToString())
                    .Concat(properties.Select(p => (p.GetValue(o, null) ?? "").ToString())).ToArray());
            }
        }

        public static MemoryStream ToExcel<T>(IEnumerable<T> objectlist, bool header = true)
        {
            var stream = new MemoryStream();

            using (ExcelPackage package = new ExcelPackage(stream)) 
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                Color yellow = ColorTranslator.FromHtml("#FFFF00");
                worksheet.Cells["A1:XFD1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:XFD1"].Style.Fill.BackgroundColor.SetColor(yellow);
                worksheet.Cells.LoadFromCollection(objectlist, header);
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells["A1:XFD1"].Style.Font.Bold = true;

                package.Save();
            }

            stream.Position = 0;

            return stream;
        }

        public static DataTable GetDataTableFromExcel(ExcelPackage pck, bool hasHeader = true)
        {
            //using (var pck = new ExcelPackage())
            //{
            //using (var stream = File.OpenRead(path))
            //{
            //    pck.Load(stream);
            //}

            var ws = pck.Workbook.Worksheets.First();

            DataTable tbl = new DataTable();

            foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
            {
                tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
            }

            var startRow = hasHeader ? 2 : 1;

            for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];

                DataRow row = tbl.Rows.Add();

                foreach (var cell in wsRow)
                {
                    row[cell.Start.Column - 1] = cell.Text;
                }
            }

            return tbl;
            //}
        }

        public static string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private static Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                //{".doc", "application/vnd.ms-word"},
                //{".docx", "application/vnd.ms-word"},
                {".doc", "application/msword"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".ppt","application/vnd.ms-powerpoint"},
                {".pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        public static string HtmlToText(string htmlContent)
        {
            // Remove new lines since they are not visible in HTML
            htmlContent = htmlContent.Replace("\n", " ");

            // Remove tab spaces
            htmlContent = htmlContent.Replace("\t", " ");

            // Remove multiple white spaces from HTML
            htmlContent = Regex.Replace(htmlContent, "\\s+", " ");

            // Remove HEAD tag
            htmlContent = Regex.Replace(htmlContent, "<head.*?</head>", ""
                                , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any JavaScript
            htmlContent = Regex.Replace(htmlContent, "<script.*?</script>", ""
              , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Replace special characters like &, <, >, " etc.
            StringBuilder sbHTML = new StringBuilder(htmlContent);
            // Note: There are many more special characters, these are just
            // most common. You can add new characters in this arrays if needed
            string[] OldWords = { "&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&reg;", "&copy;", "&bull;", "&trade;" };
            string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }

            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML.Replace("<br>", "\n<br>");
            sbHTML.Replace("<br ", "\n<br ");
            //sbHTML.Replace("<p> ", "\n<p> ");
            //sbHTML.Replace("<p ", "\n<p ");

            // insert line breaks in places of <BR> and <LI> tags
            string sbHTMLa = System.Text.RegularExpressions.Regex.Replace(sbHTML.ToString(),
                      @"<( )*br( )*>", "\r",
                      System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sbHTMLa = System.Text.RegularExpressions.Regex.Replace(sbHTMLa,
                     @"<( )*li( )*>", "\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sbHTMLa = System.Text.RegularExpressions.Regex.Replace(sbHTMLa,
                   @"<( )*p( )*>", "\r",
                   System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sbHTMLa = System.Text.RegularExpressions.Regex.Replace(sbHTMLa,
                  @"<( )*tr( )*>", "\r",
                  System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Finally, remove all HTML tags and return plain text
            return Regex.Replace(sbHTMLa.ToString(), "<[^>]*>", "");
                                                     
            ////--------< HTML_to_Text() >--------

            ////< remove blocks >
            //htmlContent = remove_script(htmlContent);
            //htmlContent = remove_Head(htmlContent);
            ////</ remove blocks >

            ////< remove Html-Tags >
            //htmlContent = remove_Tags(htmlContent);
            ////</ remove Html-Tags >

            ////< remove Charaters >
            //htmlContent = remove_Control_Characters(htmlContent);
            //htmlContent = remove_HTML_Characters(htmlContent);
            //htmlContent = remove_Special_Characters(htmlContent);
            //htmlContent = remove_Punctuation_Mark_Characters(htmlContent);

            //htmlContent = remove_Brackets_Characters(htmlContent);
            ////</ remove Charaters >

            ////< output >
            //return htmlContent;
            ////</ output >
            ////--------</ HTML_to_Text() >--------
        }

        public static byte[] ConvertHtmlToPDFByWkhtml(string wkhtmlPath, string switches, string html)
        {
            // switches:
            //     "-q"  - silent output, only errors - no progress messages
            //     " -"  - switch output to stdout
            //     "- -" - switch input to stdin and output to stdout
            switches = "-q " + switches + " -";

            // generate PDF from given HTML string, not from URL
            if (!string.IsNullOrEmpty(html))
            {
                switches += " -";
                html = SpecialCharsEncode(html);
            }

            string rotativaLocation;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                rotativaLocation = Path.Combine(wkhtmlPath, "Windows", "wkhtmltopdf.exe");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                rotativaLocation = Path.Combine(wkhtmlPath, "Mac", "wkhtmltopdf");
            else
                rotativaLocation = Path.Combine(wkhtmlPath, "Linux", "wkhtmltopdf");

            if (!System.IO.File.Exists(rotativaLocation))
                throw new Exception("wkhtmltopdf not found, searched for " + rotativaLocation);

            using (var proc = new Process())
            {
                try
                {
                    proc.StartInfo = new ProcessStartInfo
                    {
                        FileName = rotativaLocation,
                        Arguments = switches,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                        Verb = "runas"
                    };

                    proc.Start();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                // generate PDF from given HTML string, not from URL
                if (!string.IsNullOrEmpty(html))
                {
                    using (var sIn = proc.StandardInput)
                    {
                        sIn.WriteLine(html);
                    }
                }

                using (var ms = new MemoryStream())
                {
                    using (var sOut = proc.StandardOutput.BaseStream)
                    {
                        byte[] buffer = new byte[4096];
                        int read;

                        while ((read = sOut.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                    }

                    string error = proc.StandardError.ReadToEnd();

                    if (ms.Length == 0)
                    {
                        throw new Exception(error);
                    }

                    proc.WaitForExit();

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Encode all special chars
        /// </summary>
        /// <param name="text">Html text</param>
        /// <returns>Html with special chars encoded</returns>
        private static string SpecialCharsEncode(string text)
        {
            var chars = text.ToCharArray();
            var result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (var c in chars)
            {
                var value = System.Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        //public static string remove_HTML_Characters(string htmlContent)
        //{
        //    //--------< HTML_to_Text() >--------

        //    htmlContent = htmlContent.Replace("&gt;", " ");
        //    htmlContent = htmlContent.Replace("&lt;", " ");
        //    htmlContent = htmlContent.Replace("&nbsp;", " ");
        //    htmlContent = htmlContent.Replace("&gt;", " ");

        //    while (htmlContent.IndexOf("  ") >= 0)
        //        htmlContent = htmlContent.Replace("  ", " ");

        //    //< output >
        //    return htmlContent;
        //    //</ output >

        //    //--------</ HTML_to_Text() >--------
        //}

        //public static string remove_Special_Characters(string htmlContent)
        //{
        //    //--------< HTML_to_Text() >--------

        //    htmlContent = htmlContent.Replace("\\", " ");
        //    htmlContent = htmlContent.Replace("/", " ");

        //    while (htmlContent.IndexOf("  ") >= 0)
        //        htmlContent = htmlContent.Replace("  ", " ");

        //    //< output >
        //    return htmlContent;
        //    //</ output >

        //    //--------</ HTML_to_Text() >--------
        //}

        //public static string remove_Punctuation_Mark_Characters(string htmlContent)
        //{
        //    //--------< remove_Punctuation_Mark_Characters() >--------

        //    htmlContent = htmlContent.Replace(";", " ");
        //    htmlContent = htmlContent.Replace(".", " ");
        //    htmlContent = htmlContent.Replace(",", " ");
        //    htmlContent = htmlContent.Replace("'", " ");
        //    htmlContent = htmlContent.Replace(":", " ");
        //    htmlContent = htmlContent.Replace("*", " ");
        //    htmlContent = htmlContent.Replace("+", " ");
        //    htmlContent = htmlContent.Replace("=", " ");
        //    htmlContent = htmlContent.Replace("\"", " ");
        //    htmlContent = htmlContent.Replace("-", " ");
        //    htmlContent = htmlContent.Replace("_", " ");
        //    htmlContent = htmlContent.Replace("!", " ");
        //    htmlContent = htmlContent.Replace("?", " ");
        //    htmlContent = htmlContent.Replace("~", " ");
        //    htmlContent = htmlContent.Replace("#", " ");
        //    htmlContent = htmlContent.Replace("$", " ");
        //    htmlContent = htmlContent.Replace("%", " ");
        //    htmlContent = htmlContent.Replace("`", " ");
        //    htmlContent = htmlContent.Replace("´", " ");
        //    htmlContent = htmlContent.Replace("°", " ");
        //    htmlContent = htmlContent.Replace("^", " ");
        //    htmlContent = htmlContent.Replace("&", " ");

        //    while (htmlContent.IndexOf("  ") >= 0)
        //        htmlContent = htmlContent.Replace("  ", " ");

        //    //< output >
        //    return htmlContent;
        //    //</ output >

        //    //--------</ remove_Punctuation_Mark_Characters() >--------
        //}

        //public static string remove_Brackets_Characters(string htmlContent)

        //{

        //    //--------< remove_Brackets_Characters() >--------

        //    htmlContent = htmlContent.Replace("(", " ");

        //    htmlContent = htmlContent.Replace(")", " ");

        //    htmlContent = htmlContent.Replace("[", " ");

        //    htmlContent = htmlContent.Replace("]", " ");

        //    htmlContent = htmlContent.Replace("{", " ");

        //    htmlContent = htmlContent.Replace("}", " ");



        //    htmlContent = htmlContent.Replace("<", " ");

        //    htmlContent = htmlContent.Replace(">", " ");







        //    while (htmlContent.IndexOf("  ") >= 0)

        //    { htmlContent = htmlContent.Replace("  ", " "); }



        //    //< output >

        //    return htmlContent;

        //    //</ output >

        //    //--------</ remove_Brackets_Characters() >--------

        //}

        //public static string remove_Control_Characters(string htmlContent)

        //{

        //    //--------< HTML_to_Text() >--------

        //    htmlContent = htmlContent.Replace("\n", " ");

        //    htmlContent = htmlContent.Replace("\r", " ");

        //    htmlContent = htmlContent.Replace("\t", " ");



        //    while (htmlContent.IndexOf("  ") >= 0)

        //    { htmlContent = htmlContent.Replace("  ", " "); }



        //    //< output >

        //    return htmlContent;

        //    //</ output >

        //    //--------</ HTML_to_Text() >--------

        //}

        //public static string remove_Tags(string htmlContent)

        //{

        //    //--------< remove_Tags() >--------



        //    //----< @Loop: Search tags >----

        //    int intStart = -1;

        //    while (1 == 1)

        //    {

        //        //---< Search Tag >---

        //        //< check end >

        //        if (htmlContent.Length <= intStart) break;

        //        //< check end >



        //        //< find open >

        //        int posOpenTag = htmlContent.IndexOf("<", intStart + 1);

        //        if (posOpenTag < 0) break;

        //        //</ find open >





        //        //< find close >

        //        int posCloseTag = htmlContent.IndexOf(">", posOpenTag);

        //        if (posCloseTag < 0) break; //no end tag

        //        //</ find close >





        //        //< cut Tag >

        //        string sLeft = htmlContent.Substring(0, posOpenTag);

        //        string sRight = htmlContent.Substring(posCloseTag + 1);

        //        htmlContent = sLeft + " " + sRight;

        //        //</ cut Tag >





        //        intStart = sLeft.Length;

        //        //---</ Search Tag >---

        //    }

        //    //----</ @Loop: Search tags >----





        //    //< output >

        //    return htmlContent;

        //    //</ output >

        //    //--------</ remove_Tags() >--------

        //}

        //public static string remove_script(string htmlContent)

        //{

        //    //--------< remove_script() >--------



        //    //----< @Loop: Search tags >----

        //    int intStart = 0;

        //    while (1 == 1)

        //    {

        //        //---< Search Tag >---

        //        //< check end >

        //        if (htmlContent.Length <= intStart) break;

        //        //< check end >



        //        //< find open >

        //        int posscript_Open = htmlContent.IndexOf("<script", intStart + 1, comparisonType: System.StringComparison.InvariantCultureIgnoreCase);

        //        if (posscript_Open < 0) break; //no open tag

        //        //</ find open >



        //        //< find close >

        //        int posscript_Close = htmlContent.IndexOf("</script", posscript_Open + 1, comparisonType: System.StringComparison.InvariantCultureIgnoreCase);

        //        if (posscript_Close < 0) break; //no end tag

        //        //</ find close >



        //        //< find close >

        //        int posCloseTag = htmlContent.IndexOf(">", posscript_Close);

        //        if (posCloseTag < 0) break; //no end tag

        //        //</ find close >



        //        //< cut Tag >

        //        string sLeft = htmlContent.Substring(0, posscript_Open);

        //        string sRight = htmlContent.Substring(posCloseTag + 1);

        //        htmlContent = sLeft + sRight;

        //        //</ cut Tag >





        //        intStart = sLeft.Length;

        //        //---</ Search Tag >---

        //    }

        //    //----</ @Loop: Search tags >----



        //    //< output >

        //    return htmlContent;

        //    //</ output >

        //    //--------</ remove_script() >--------

        //}

        //public static string remove_Head(string htmlContent)

        //{

        //    //--------< remove_Head() >--------



        //    //----< @Loop: Search tags >----

        //    int intStart = 0;

        //    while (1 == 1)

        //    {

        //        //---< Search Tag >---

        //        //< check end >

        //        if (htmlContent.Length <= intStart) break;

        //        //< check end >



        //        //< find open >

        //        int posHead_Open = htmlContent.IndexOf("<head", intStart + 1, comparisonType: System.StringComparison.InvariantCultureIgnoreCase);

        //        if (posHead_Open < 0) break; //no open tag

        //        //</ find open >



        //        //< find close >

        //        int posHead_Close = htmlContent.IndexOf("</head", posHead_Open + 1, comparisonType: System.StringComparison.InvariantCultureIgnoreCase);

        //        if (posHead_Close < 0) break; //no end tag

        //        //</ find close >



        //        //< find close >

        //        int posCloseTag = htmlContent.IndexOf(">", posHead_Close);

        //        if (posCloseTag < 0) break; //no end tag

        //        //</ find close >



        //        //< cut Tag >

        //        string sLeft = htmlContent.Substring(0, posHead_Open);

        //        string sRight = htmlContent.Substring(posCloseTag + 1);

        //        htmlContent = sLeft + sRight;

        //        //</ cut Tag >





        //        intStart = sLeft.Length;

        //        //---</ Search Tag >---

        //    }
        //    //----</ @Loop: Search tags >----

        //    //< output >
        //    return htmlContent;
        //    //</ output >
        //    //--------</ remove_Head() >--------
        //}

        //public static string StripHTML(string source)
        //{
        //    try
        //    {
        //        string result;

        //        // Remove HTML Development formatting
        //        // Replace line breaks with space
        //        // because browsers inserts space
        //        result = source.Replace("\r", " ");
        //        // Replace line breaks with space
        //        // because browsers inserts space
        //        result = result.Replace("\n", " ");
        //        // Remove step-formatting
        //        result = result.Replace("\t", string.Empty);
        //        // Remove repeating spaces because browsers ignore them
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                                                              @"( )+", " ");

        //        // Remove the header (prepare first by clearing attributes)
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*head([^>])*>", "<head>",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"(<( )*(/)( )*head( )*>)", "</head>",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 "(<head>).*(</head>)", string.Empty,
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // remove all scripts (prepare first by clearing attributes)
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*script([^>])*>", "<script>",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"(<( )*(/)( )*script( )*>)", "</script>",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        //result = System.Text.RegularExpressions.Regex.Replace(result,
        //        //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
        //        //         string.Empty,
        //        //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"(<script>).*(</script>)", string.Empty,
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // remove all styles (prepare first by clearing attributes)
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*style([^>])*>", "<style>",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"(<( )*(/)( )*style( )*>)", "</style>",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 "(<style>).*(</style>)", string.Empty,
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // insert tabs in spaces of <td> tags
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*td([^>])*>", "\t",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // insert line breaks in places of <BR> and <LI> tags
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*br( )*>", "\r",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*li( )*>", "\r",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // insert line paragraphs (double line breaks) in place
        //        // if <P>, <DIV> and <TR> tags
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*div([^>])*>", "\r\r",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*tr([^>])*>", "\r\r",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<( )*p([^>])*>", "\r\r",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // Remove remaining tags like <a>, links, images,
        //        // comments etc - anything that's enclosed inside < >
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"<[^>]*>", string.Empty,
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // replace special characters:
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @" ", " ",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&bull;", " * ",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&lsaquo;", "<",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&rsaquo;", ">",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&trade;", "(tm)",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&frasl;", "/",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&lt;", "<",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&gt;", ">",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&copy;", "(c)",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&reg;", "(r)",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        // Remove all others. More can be added, see
        //        // http://hotwired.lycos.com/webmonkey/reference/special_characters/
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 @"&(.{2,6});", string.Empty,
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // for testing
        //        //System.Text.RegularExpressions.Regex.Replace(result,
        //        //       this.txtRegex.Text,string.Empty,
        //        //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        //        // make line breaking consistent
        //        result = result.Replace("\n", "\r");

        //        // Remove extra line breaks and tabs:
        //        // replace over 2 breaks with 2 and over 4 tabs with 4.
        //        // Prepare first to remove any whitespaces in between
        //        // the escaped characters and remove redundant tabs in between line breaks
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 "(\r)( )+(\r)", "\r\r",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 "(\t)( )+(\t)", "\t\t",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 "(\t)( )+(\r)", "\t\r",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 "(\r)( )+(\t)", "\r\t",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        // Remove redundant tabs
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 "(\r)(\t)+(\r)", "\r\r",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        // Remove multiple tabs following a line break with just one tab
        //        result = System.Text.RegularExpressions.Regex.Replace(result,
        //                 "(\r)(\t)+", "\r\t",
        //                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //        // Initial replacement target string for line breaks
        //        string breaks = "\r\r\r";
        //        // Initial replacement target string for tabs
        //        string tabs = "\t\t\t\t\t";
        //        for (int index = 0; index < result.Length; index++)
        //        {
        //            result = result.Replace(breaks, "\r\r");
        //            result = result.Replace(tabs, "\t\t\t\t");
        //            breaks = breaks + "\r";
        //            tabs = tabs + "\t";
        //        }

        //        // That's it.
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return source;
        //    }
        //}
    }
}