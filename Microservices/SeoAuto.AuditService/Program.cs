using Microsoft.EntityFrameworkCore;
using SeoAuto.AuditService.Infrastructure.Database;
using SeoAuto.BuildingBlocks.Exceptions;
using SeoAuto.BuildingBlocks.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SeoAuto.AuditService.Features.Audits.SubmitAudit;
using SeoAuto.AuditService.Features.Audits.GetAuditById;
using SeoAuto.AuditService.Features.Audits.GetMyAudits;
using Scalar.AspNetCore;
using SeoAuto.AuditService.Infrastructure.ExternalService;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký OpenAPI/Swagger
builder.Services.AddOpenApi();

// 2. Kích hoạt Bẫy lỗi toàn cục (Bắt mọi Exception và trả về chuẩn JSON ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AuditDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<IPageSpeedService, PageSpeedService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2); // Google PageSpeed phân tích có thể tốn 30s - 1 phút
});
// 3. Kích hoạt RabbitMQ (Xe chở thư)
builder.Services.AddMessageBroker(builder.Configuration, typeof(Program).Assembly);
builder.Services.AddHttpClient<IHtmlService, HtmlService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 SeoAutoBot/1.0");
});


//Middlerware Authentication JWT
var secretKey = builder.Configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForSeoAutoV2ProjectMinimum32BytesLong!";
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? "SeoAutoV2";
var audience = builder.Configuration["JwtSettings:Audience"] ?? "SeoAutoV2Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// 4. Bật Middleware bẫy lỗi lên
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


// API Test thử nghiệm
app.MapGet("/api/audits/ping", () => "AuditService is running with RabbitMQ!");
app.MapSubmitAuditEndpoint(); // Đăng ký Endpoint Submit Audit
app.MapGetAuditByIdEndpoint(); // Đăng ký Endpoint Get Audit By Id
app.MapGetMyAuditsEndpoint();  // Đăng ký Endpoint Get My Audits

app.Run();