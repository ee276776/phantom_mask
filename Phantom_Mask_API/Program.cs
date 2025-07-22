using Microsoft.EntityFrameworkCore;
using PhantomMaskAPI.Data;
using PhantomMaskAPI.Services;
using PhantomMaskAPI.Configuration;
using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Repositories;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 配置 URL 和端口 (for Docker)
builder.WebHost.UseUrls("http://0.0.0.0:80");

// 添加服務到容器
builder.Services.AddControllers();

// 配置 Entity Framework
var secureConnectionService = new SecureConnectionService(
    builder.Configuration, 
    new EncryptionService(),
    LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SecureConnectionService>()
);

builder.Services.AddDbContext<PhantomMaskContext>(options =>
{
    options.UseNpgsql(secureConnectionService.GetConnectionString());
});

// 註冊加密服務
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddSingleton<SecureConnectionService>();

// 註冊 Repository 層
builder.Services.AddScoped<IPharmacyRepository, PharmacyRepository>();
builder.Services.AddScoped<IMaskRepository, MaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();

// 註冊 Service 層
builder.Services.AddScoped<IPharmacyService, PharmacyService>();
builder.Services.AddScoped<IMaskService, MaskService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IRelevanceService, RelevanceService>();


// 配置 Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "PhantomMask API", 
        Version = "v1",
        Description = "口罩購買系統 API - 提供藥局、口罩和購買相關功能"
    });
    
    // 添加 XML 註釋
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhantomMask API V1");
        c.RoutePrefix = string.Empty; // 設置 Swagger UI 在根路徑
    });
}

// 移除 HTTPS 重定向 (Docker 中使用 HTTP)
// app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 確保資料庫已創建
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PhantomMaskContext>();
    try 
    {
        await context.Database.CanConnectAsync();
        Console.WriteLine("✅ 資料庫連線成功!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 資料庫連線失敗: {ex.Message}");
    }
}

Console.WriteLine("🚀 PhantomMask API 已啟動!");
Console.WriteLine("📋 Swagger UI: http://localhost");
Console.WriteLine("🔗 API Base URL: http://localhost/api");

app.Run();
