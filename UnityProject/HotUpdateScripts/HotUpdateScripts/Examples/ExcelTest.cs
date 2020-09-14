using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using UnityEngine;

namespace HotUpdateScripts
{
    class ExcelTest : MonoBehaviour
    {
        public void Start()
        {
            CreateExcel();
            ReadExcel();
        }

        void CreateExcel()
        {
            string outPutDir = Application.dataPath + "\\" + "MyExcel.xls";
            FileInfo newFile = new FileInfo(outPutDir);
            if (newFile.Exists)
            {
                newFile.Delete(); // ensures we create a new workbook   
                Debug.Log("Clear ");
                newFile = new FileInfo(outPutDir);
            }

            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("我的Excel");
                worksheet.Cells[1, 1].Value = "序号";
                worksheet.Cells[1, 2].Value = "姓名";
                worksheet.Cells[1, 3].Value = "电话";
                package.Save();
                Debug.Log("创建Excel成功");
            }
        }
        void ReadExcel() {
            string outPutDir = Application.dataPath + "\\" + "MyExcel.xls";
            using ( ExcelPackage package = new ExcelPackage(new FileStream(outPutDir, FileMode.Open)) )
            {
                for ( int i = 1; i <= package.Workbook.Worksheets.Count; ++i )
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets[i];
                    for ( int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++ )
                    {
                        for ( int m = sheet.Dimension.Start.Row, n = sheet.Dimension.End.Row; m <= n; m++ )
                        {
                            string str = sheet.GetValue(m, j).ToString();                   
                            if ( str != null )
                            {
                                // do something
                                Debug.Log(str);
                            }
                        }
                    }
                }
            }
        }
    }
}