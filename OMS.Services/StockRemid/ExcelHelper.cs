

using OfficeOpenXml;
using System.IO;


namespace OMS.Services.StockRemid
{
    /// <summary>
    /// excel帮助类
    /// </summary>
    public static class ExcelHelper
    {

        /// <summary>
        /// 导出excel
        /// </summary>
        public static void Export(string path, string[][] data, string destination)
        {
            FileStream file;
            using (ExcelPackage package = new ExcelPackage(file = new FileStream(path, FileMode.Open)))
            {
                var sheet = package.Workbook.Worksheets[0];
                for (int i = 0; i < data.Length; i++)
                    for (int j = 0; j < data[i].Length; j++)
                        sheet.Cells[i + 2, j + 1].Value = data[i][j];
                //导出
                File.WriteAllBytes(destination, package.GetAsByteArray());
                file.Close();
                file.Dispose();
            }
        }
    }
}
