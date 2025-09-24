using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Test.Application.Interfaces;
using Test.Application.Mappers;
using Test.Application.Services;
using Test.Application.Services.Interfaces;
using Test.Core.Interfaces;
using Test.Infrastructure.Data;
using Test.Infrastructure.Data.Repositories;
using Test.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Чтение connection string по имени "DBContext" из appsettings.json.
// В продакшне проверяйте, что conn не null/пустой и логируйте, если нет.
var conn = builder.Configuration.GetConnectionString("DBContext");

builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(conn)
);

// Применяем вашу конфигурацию Mapster (если настроена)
MapsterConfig.ConfigureGlobal(); // если есть конфиг (вы его ранее создавали)
var config = TypeAdapterConfig.GlobalSettings;

// Регистрируем TypeAdapterConfig как singleton — это статическая/глобальная конфигурация.
builder.Services.AddSingleton(config);

// Регистрируем IMapper (MapsterMapper.Mapper).
// Mapper потокобезопасен, поэтому его можно регистрировать как singleton:
// здесь вы регистрируете как scoped — это работает, но singleton будет чуть эффективнее.
builder.Services.AddScoped<IMapper>(sp => new MapsterMapper.Mapper(config));

var filesBasePath = Path.Combine(builder.Environment.WebRootPath ?? Directory.GetCurrentDirectory(), "Files");

// Infrastructure
// Общий репозиторий для всех сущностей
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// UnitOfWork - scoped, т.к. зависит от DbContext (который по умолчанию scoped)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Сервис парсинга Excel (NPOI/ClosedXML)
builder.Services.AddScoped<IExcelProcessor, ExcelProcessor>();

// FileService: вы создаёте инстанс вручную, передаёте путь для хранения.
// В продакшне лучше передавать путь через IOptions<FileOptions> или IConfiguration.
builder.Services.AddScoped<IFileService>(sp => new FileService(Path.Combine(builder.Environment.WebRootPath, "Files")));

// Application
builder.Services.AddScoped<IExcelProcessingService, ExcelProcessingService>();
builder.Services.AddScoped<ITurnoverService, TurnoverService>();

// Сервис, управляющий загрузкой/обработкой файлов в приложении
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllersWithViews()
    // .AddRazorRuntimeCompilation() // опционально: позволяет править cshtml без перезапуска в разработке
    ;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // В девелопменте показываем подробную страницу ошибок и Swagger UI
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Небольшое замечание: для продакшна добавьте глобальный обработчик ошибок и логирование,
// app.UseExceptionHandler("/Home/Error"); app.UseHsts(); и пр.

app.UseStaticFiles(); // Отдача статичных файлов (wwwroot)
app.UseRouting();

// Если используете аутентификацию, сначала UseAuthentication(), затем UseAuthorization()
app.UseAuthorization();

// Маршрутизация MVC контроллеров
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// dotnet ef migrations add InitialCreate --project ./Test.Infrastructure/Test.Infrastructure.csproj --startup-project ./Test.UI/Test.UI.csproj --context Test.Infrastructure.Data.DBContext -o Migrations
// dotnet ef database update --project ./Test.Infrastructure/Test.Infrastructure.csproj --startup-project ./Test.UI/Test.UI.csproj --context Test.Infrastructure.Data.DBContext
