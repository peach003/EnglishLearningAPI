using EnglishLearningAPI.Data;
using EnglishLearningAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1️⃣ 配置 CORS 允许前端访问
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .WithOrigins("http://localhost:3000") // ✅ 确保 React 前端能访问
            .AllowAnyMethod() // ✅ 允许所有 HTTP 方法
            .AllowAnyHeader() // ✅ 允许所有 Headers
            .AllowCredentials()); // ✅ 允许携带身份凭证 (Cookies, Auth Headers)
});

// ✅ 2️⃣ 配置数据库
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 3️⃣ 配置 ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ✅ 4️⃣ 启用日志
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug); 

// ✅ 5️⃣ 配置 JWT 认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = jwtSettings["Key"];

if (string.IsNullOrEmpty(key))
{
    throw new InvalidOperationException("JWT Key is missing in configuration.");
}

var keyBytes = Encoding.UTF8.GetBytes(key);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// ✅ 6️⃣ 配置 API 控制器和 Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ 7️⃣ 确保 `CORS` 在 `Routing` 之前
app.UseCors("AllowFrontend");

app.UseRouting();
app.UseAuthentication(); // 启用 JWT 认证
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();


