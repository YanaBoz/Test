using ClosedXML.Excel;
using Test.UI.Data;

namespace Test.UI.Models
{
    // класс для вывода данных в Excel
    public class MyViewModelimp
    {
        public List<Constructor> Con { get; set; }
        //функция вывода данных в Excel из одного файла
        public async Task<string> To_Filez(IWebHostEnvironment _appEnvironment, DBContext _context, List<Constructor> construct, int fileId)
        {
            //создание нового файла
            var file = await _context.Files.FindAsync(fileId);
            string path = _appEnvironment.WebRootPath + "/Files_Load/" + file.Name;
            string directoryPath = Path.Combine(_appEnvironment.WebRootPath, "Files_Load");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            //открытие и ввод данных в файл
            var fileStream = new FileStream(path, FileMode.OpenOrCreate);
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Лист1");
            var rngTable = worksheet.Range("B1:C1");
            rngTable.Merge();
            rngTable = worksheet.Range("D1:E1");
            rngTable.Merge();
            rngTable = worksheet.Range("F1:G1");
            rngTable.Merge();
            rngTable = worksheet.Range("A1:A2");
            rngTable.Merge();
            worksheet.Cell("A" + 1).Value = "Б/сч";
            worksheet.Cell("B" + 1).Value = "ВХОДЯЩЕЕ САЛЬДО";
            worksheet.Cell("D" + 1).Value = "ОБОРОТЫ";
            worksheet.Cell("F" + 1).Value = "ИСХОДЯЩЕЕ САЛЬДО";
            worksheet.Cell("B" + 2).Value = "Актив";
            worksheet.Cell("C" + 2).Value = "Пассив";
            worksheet.Cell("D" + 2).Value = "Дебет";
            worksheet.Cell("E" + 2).Value = "Кредит";
            worksheet.Cell("F" + 2).Value = "Актив";
            worksheet.Cell("G" + 2).Value = "Пассив";
            int i = 3;
            //группировка данных по классу
            foreach (var group in construct.GroupBy(x => x.CL))
            {
                rngTable = worksheet.Range($"A{i}:G{i}");
                rngTable.Merge();
                worksheet.Cell("A" + i).Value = group.Key;
                i++;
                foreach (var item in group)
                {
                    worksheet.Cell("A" + i).Value = item.B_sch;
                    worksheet.Cell("B" + i).Value = item.VS_A;
                    worksheet.Cell("C" + i).Value = item.VS_P;
                    worksheet.Cell("D" + i).Value = item.O_D;
                    worksheet.Cell("E" + i).Value = item.O_C;
                    worksheet.Cell("F" + i).Value = item.IS_A;
                    worksheet.Cell("G" + i).Value = item.IS_P;
                    i++;
                }
            }
            worksheet.Columns().AdjustToContents(); //установка размера ячеек по содержимому
            // сохранение и закрытие файла
            workbook.SaveAs(fileStream);
            fileStream.Close();
            return path;
        }
        //функция вывода данных в Excel из всех файлов
        public string To_File(IWebHostEnvironment _appEnvironment, DBContext _context, List<Constructor> construct)
        {
            //создание общего файла
            string path = _appEnvironment.WebRootPath + "/Files_Load/" + "CommonFil.xlsx";
            string directoryPath = Path.Combine(_appEnvironment.WebRootPath, "Files_Load");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            //открытие и ввод данных в файл
            var fileStream = new FileStream(path, FileMode.OpenOrCreate);
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Лист1");
            var rngTable = worksheet.Range("B1:C1");
            rngTable.Merge();
            rngTable = worksheet.Range("D1:E1");
            rngTable.Merge();
            rngTable = worksheet.Range("F1:G1");
            rngTable.Merge();
            rngTable = worksheet.Range("A1:A2");
            rngTable.Merge();
            worksheet.Cell("A" + 1).Value = "Б/сч";
            worksheet.Cell("B" + 1).Value = "ВХОДЯЩЕЕ САЛЬДО";
            worksheet.Cell("D" + 1).Value = "ОБОРОТЫ";
            worksheet.Cell("F" + 1).Value = "ИСХОДЯЩЕЕ САЛЬДО";
            worksheet.Cell("B" + 2).Value = "Актив";
            worksheet.Cell("C" + 2).Value = "Пассив";
            worksheet.Cell("D" + 2).Value = "Дебет";
            worksheet.Cell("E" + 2).Value = "Кредит";
            worksheet.Cell("F" + 2).Value = "Актив";
            worksheet.Cell("G" + 2).Value = "Пассив";
            int i = 3;
            //группировка данных по классу
            foreach (var group in construct.GroupBy(x => x.CL))
            {
                rngTable = worksheet.Range($"A{i}:G{i}");
                rngTable.Merge();
                worksheet.Cell("A" + i).Value = group.Key;
                i++;
                foreach (var item in group)
                {
                    worksheet.Cell("A" + i).Value = item.B_sch;
                    worksheet.Cell("B" + i).Value = item.VS_A;
                    worksheet.Cell("C" + i).Value = item.VS_P;
                    worksheet.Cell("D" + i).Value = item.O_D;
                    worksheet.Cell("E" + i).Value = item.O_C;
                    worksheet.Cell("F" + i).Value = item.IS_A;
                    worksheet.Cell("G" + i).Value = item.IS_P;
                    i++;
                }
            }
            worksheet.Columns().AdjustToContents();
            // сохранение и закрытие файла
            workbook.SaveAs(fileStream);
            fileStream.Close();
            return path;
        }
    }
}
