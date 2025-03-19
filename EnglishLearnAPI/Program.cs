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

//  **Configure CORS to allow React access**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:3000") 
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

//  **Configure Database (SQL Server)**
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  **Hangfire Task Scheduling**
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

//  **Register Services**
builder.Services.AddScoped<DiagramWordService>();
builder.Services.AddScoped<RecommendWordService>();

//  **ASP.NET Identity Configuration**
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//  **Logging System**
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

//  **JWT Token Authentication**
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
        ValidIssuer = jwtSettings["Issuer"],  
        ValidAudience = jwtSettings["Audience"],  
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),  
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true, 
        ClockSkew = TimeSpan.Zero 
    };
});

builder.Services.AddAuthorization();

//  **Add `HttpClient` (for RecommendWordService to call Python API)**
builder.Services.AddHttpClient();

//  **Configure API Controllers & Swagger**
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//  **Configure CORS (must be before `UseRouting()`)**
app.UseCors("AllowFrontend");

//  **Hangfire Dashboard**
app.UseHangfireDashboard();

//  **Routing & Authentication**
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//  **Swagger Documentation**
app.UseSwagger();
app.UseSwaggerUI();

//  **Map API Controllers**
app.MapControllers();

//  **Run the application**
app.Run();

