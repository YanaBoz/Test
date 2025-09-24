using Test.Core.Interfaces;
using Test.Infrastructure.Data.Repositories;
using Test.Infrastructure.Services;
using Test.Application.Services;
using Test.Data;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IExcelProcessor, ExcelProcessor>();
builder.Services.AddScoped<ExcelProcessingService>();
builder.Services.AddScoped<TurnoverService>();

builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Конфигурация pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();