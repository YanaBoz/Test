using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Test.UI.Models;
using Test.UI.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Test.UI.Controllers;

// ������ ����� ��� ������ .xlsx
public class HomeController : Controller
{
    // ILogger ��� ������ �����
    private readonly ILogger<HomeController> _logger;

    // DBContext ��� ������� � ���� ������
    private readonly DBContext _context;

    // IWebHostEnvironment ��� ������� � ���������� �� ��������� ����������
    private readonly IWebHostEnvironment _appEnvironment;

    // ����������� ������ HomeController
    public HomeController(IWebHostEnvironment appEnvironment, DBContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
        _appEnvironment = appEnvironment;
    }
    //��������� ������ �����
    [HttpGet("/{fileId}")]
    public async Task<IActionResult> Load_R(int fileId)
    {
        //����� � �������� Excel
        var file = await _context.Files.FindAsync(fileId);
        if (file == null)
        {
            return NotFound();
        }
        using (var workbook = new XLWorkbook(file.Path))
        {
            var worksheet = workbook.Worksheets.First();
            var data = new List<Turnover>();
            for (int row = 9; row <= worksheet.LastRowUsed().RowNumber() - 2; row++)
            {
                if (int.TryParse(worksheet.Cell(row, 1).GetString(), out int df) && (Convert.ToInt32(worksheet.Cell(row, 1).GetString()) / 100) > 1)
                {
                    Turnover item = new()
                    {
                        Class_ID = 0,
                        Group_ID = 0,
                        Account = Convert.ToInt32(worksheet.Cell(row, 1).GetString()),
                        Start_Active = worksheet.Cell(row, 2).GetDouble(),
                        Start_Passive = worksheet.Cell(row, 3).GetDouble(),
                        Turn_Debit = worksheet.Cell(row, 4).GetDouble(),
                        Turn_Credit = worksheet.Cell(row, 5).GetDouble()
                    };
                    data.Add(item);
                }
            }
            MyViewModel viewModel = new();
            // ��������� ������ ��� �����������
            List<Constructor> pocket = viewModel.Print( data, _context);
            return View(pocket);
        }
    }
    // �������� �����
    [HttpPost]
    public async Task<IActionResult> Load(IFormFile fileExcel, [Bind("Id,Group_ID,Class_ID,Account,Start_Active,Start_Passive,Turn_Debit,Turn_Credit")] Turnover turnover, [Bind("Id,Name")] Oper_Class oper_Class, [Bind("Id,Number")] Oper_Class group)
    {
        //��������� � ���������� ����� � ����� �������
        string path = _appEnvironment.WebRootPath + "/Files/" + fileExcel.FileName;
        string directoryPath = Path.Combine(_appEnvironment.WebRootPath, "Files");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        using (var fileStream = new FileStream(path, FileMode.Create))
        {
            await fileExcel.CopyToAsync(fileStream);
        }
        //���������� ����� � ��
        FileModel file = new() { Name = fileExcel.FileName, Path = path };
        _context.Files.Add(file);
        _context.SaveChanges();
        //������ � ������ ������ � ��
        using (var workbook = new XLWorkbook(file.Path))
        {
            var worksheet = workbook.Worksheets.First();
            int count = 0; //ID
            int gr_count = 1;
            int str_count = 0;
            for (int row = 9; row <= worksheet.LastRowUsed().RowNumber() - 2; row++)
            {
                if (_context.Groups.IsNullOrEmpty())
                {
                    if (int.TryParse(worksheet.Cell(row, 1).GetString(), out int df) && (Convert.ToInt32(worksheet.Cell(row, 1).GetString()) / 100) < 1)
                    {
                        Group item = new()
                        {
                            Number = Convert.ToInt32(worksheet.Cell(row, 1).GetString())
                        };
                        var its = await _context.Groups.FirstOrDefaultAsync(f => f.Number == item.Number);
                        if (its == null)
                        {
                            _context.Add(item);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else break;
            }

            for (int row = 9; row <= worksheet.LastRowUsed().RowNumber() - 2; row++)
            {
                if (!int.TryParse(worksheet.Cell(row, 1).GetString(), out int df))
                {
                    if (!double.TryParse(worksheet.Cell(row, 2).GetString(), out double fg))
                    {
                        str_count++;
                        Oper_Class item = new()
                        {
                            Name = worksheet.Cell(row, 1).GetString()
                        };
                        var its = await _context.Oper_Classes.FirstOrDefaultAsync(f => f.Name == item.Name);
                        if (its == null)
                        {
                            _context.Add(item);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                if (int.TryParse(worksheet.Cell(row, 1).GetString(), out df) && (Convert.ToInt32(worksheet.Cell(row, 1).GetString()) / 100) < 1)
                {
                    gr_count++;
                    Group item = new()
                    {
                        Number = Convert.ToInt32(worksheet.Cell(row, 1).GetString())
                    };
                    var its = await _context.Groups.FirstOrDefaultAsync(f => f.Number == item.Number);
                    if (its == null)
                    {
                        _context.Add(item);
                        await _context.SaveChangesAsync();
                    }
                }
                if (int.TryParse(worksheet.Cell(row, 1).GetString(), out df) && (Convert.ToInt32(worksheet.Cell(row, 1).GetString()) / 100) > 1)
                {
                    count++;
                    Turnover item = new()
                    {
                        Class_ID = _context.Oper_Classes.FirstOrDefault(c => c.Id == str_count)?.Id,
                        Group_ID = _context.Groups.FirstOrDefault(c => c.Id == gr_count)?.Id,
                        Account = Convert.ToInt32(worksheet.Cell(row, 1).GetString()),
                        Start_Active = worksheet.Cell(row, 2).GetDouble(),
                        Start_Passive = worksheet.Cell(row, 3).GetDouble(),
                        Turn_Debit = worksheet.Cell(row, 4).GetDouble(),
                        Turn_Credit = worksheet.Cell(row, 5).GetDouble()
                    };

                    _context.Add(item);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index");
        }
    }
    public IActionResult Index()
    {
        return View(_context.Files.ToList());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    //���������� � ���� ����
    [HttpGet]
    public IActionResult Save_All()
    {
        //��������� ������ ������ � ���� ������ �� ���
        var files = _context.Files.ToList();
        var data = new List<Turnover>();
        foreach (var file in files)
        {
            using (var workbook = new XLWorkbook(file.Path))
            {
                var worksheet = workbook.Worksheets.First();
                for (int row = 9; row <= worksheet.LastRowUsed().RowNumber() - 2; row++)
                {
                    if (int.TryParse(worksheet.Cell(row, 1).GetString(), out int df) && (Convert.ToInt32(worksheet.Cell(row, 1).GetString()) / 100) > 1)
                    {
                        Turnover item = new()
                        {
                            Class_ID = 0,
                            Group_ID = 0,
                            Account = Convert.ToInt32(worksheet.Cell(row, 1).GetString()),
                            Start_Active = worksheet.Cell(row, 2).GetDouble(),
                            Start_Passive = worksheet.Cell(row, 3).GetDouble(),
                            Turn_Debit = worksheet.Cell(row, 4).GetDouble(),
                            Turn_Credit = worksheet.Cell(row, 5).GetDouble()
                        };
                        data.Add(item);
                    }
                }
            }
        }
        // ��������� ������ ��� �����������
        MyViewModel viewModel = new();
        List<Constructor> pocket = viewModel.Print(data, _context);
        MyViewModelimp viewModelimp = new();
        //���������� ������ � �����
        string path = viewModelimp.To_File(_appEnvironment, _context, pocket);
        //���������� ����� � ��
        FileModel filez = new() { Name = "CommonFil.xlsx", Path = path };
        _context.Files.Add(filez);
        _context.SaveChanges();
        return View(pocket);
    }
    //���������� ������ �����
    [HttpGet("Home/Save/{fileId}")]
    public async Task<IActionResult> Save(int fileId)
    {
        //����� ����� � ��
        var file = await _context.Files.FindAsync(fileId);
        if (file == null)
        {
            return NotFound();
        }
        //��������� ������ ��� �������� � ������� 
        var data = new List<Turnover>();
            using (var workbook = new XLWorkbook(file.Path))
            {
                var worksheet = workbook.Worksheets.First();
                for (int row = 9; row <= worksheet.LastRowUsed().RowNumber() - 2; row++)
                {
                    if (int.TryParse(worksheet.Cell(row, 1).GetString(), out int df) && (Convert.ToInt32(worksheet.Cell(row, 1).GetString()) / 100) > 1)
                    {
                        Turnover item = new()
                        {
                            Class_ID = 0,
                            Group_ID = 0,
                            Account = Convert.ToInt32(worksheet.Cell(row, 1).GetString()),
                            Start_Active = worksheet.Cell(row, 2).GetDouble(),
                            Start_Passive = worksheet.Cell(row, 3).GetDouble(),
                            Turn_Debit = worksheet.Cell(row, 4).GetDouble(),
                            Turn_Credit = worksheet.Cell(row, 5).GetDouble()
                        };
                        data.Add(item);
                    }
                }
            }
        // ��������� ������ ��� �����������
        MyViewModel viewModel = new();
        List<Constructor> pocket = viewModel.Print(data, _context);
        MyViewModelimp viewModelimp = new();
        //���������� ������ � �����
        string path = await viewModelimp.To_Filez(_appEnvironment, _context, pocket,fileId);
        //���������� ����� � ��
        FileModel filez = new() { Name = file.Name, Path = path };
        _context.Files.Add(filez);
        _context.SaveChanges();
        return View(pocket);
    }
}
