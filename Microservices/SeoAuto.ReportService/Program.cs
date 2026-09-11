using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SeoAuto.BuildingBlocks.Exceptions;
using SeoAuto.BuildingBlocks.Messaging;
using SeoAuto.ReportService.Features.Projects;
using SeoAuto.ReportService.Features.Reports;
using SeoAuto.ReportService.Features.Websites;
using SeoAuto.ReportService.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

// 1. OpenAPI & Scalar
builder.Services.AddOpenApi();

// 2. Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. PostgreSQL DbContext
builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Message Broker (RabbitMQ & MassTransit)
builder.Services.AddMessageBroker(builder.Configuration, typeof(Program).Assembly);

// 5. JWT Authentication & Authorization
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

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/reports/ping", () => "ReportService is running on port 5003!");

// Đăng ký các nhóm Endpoints cho Projects, Websites và Reports
app.MapProjectsEndpoints();
app.MapWebsitesEndpoints();
app.MapReportsEndpoints();

app.Run();

