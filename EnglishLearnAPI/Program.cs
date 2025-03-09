using EnglishLearningAPI.Data;
using EnglishLearningAPI.Models;
using EnglishLearningAPI.Services; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS to allow front-end access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:3000") 
            .AllowAnyMethod() 
            .AllowAnyHeader() 
            .AllowCredentials()); 
});

// Configuration database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuring ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Enable logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Configure JWT Authentication
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

// Add `HttpClient` for `AudioController.cs` to call the Deepgram API
builder.Services.AddHttpClient();

// Configuring the API Controller and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure that CORS is before Routing
app.UseCors("AllowFrontend");

app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
