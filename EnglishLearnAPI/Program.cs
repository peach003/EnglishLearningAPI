using EnglishLearningAPI.Data;
using EnglishLearningAPI.Models;
using EnglishLearningAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// ✅ **配置 CORS 允许 React 访问**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:3000") // **允许前端访问**
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// ✅ **配置数据库（SQL Server）**
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ **Hangfire 任务调度**
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// ✅ **注册服务**
builder.Services.AddScoped<DiagramWordService>();
builder.Services.AddScoped<RecommendWordService>();

// ✅ **ASP.NET Identity 配置**
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ✅ **日志系统**
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ✅ **JWT 令牌身份验证**
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = jwtSettings["Issuer"],  // 发行者
        ValidAudience = jwtSettings["Audience"],  // 受众
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),  // 签名密钥
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true, // 验证令牌有效期
        ClockSkew = TimeSpan.Zero // 允许时间偏差（默认5分钟，设置为0）
    };
});

builder.Services.AddAuthorization();

// ✅ **添加 `HttpClient`（用于 RecommendWordService 调用 Python API）**
builder.Services.AddHttpClient();

// ✅ **配置 API 控制器 & Swagger**
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ **配置 CORS（必须在 `UseRouting()` 之前）**
app.UseCors("AllowFrontend");

// ✅ **Hangfire 仪表盘**
app.UseHangfireDashboard();

// ✅ **路由 & 身份验证**
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ✅ **Swagger 文档**
app.UseSwagger();
app.UseSwaggerUI();

// ✅ **映射 API 控制器**
app.MapControllers();

// ✅ **运行应用**
app.Run();
