using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<SeoAuto.IdentityService.Infrastructure.Database.AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Global Exception Handler từ BuildingBlocks
builder.Services.AddExceptionHandler<SeoAuto.BuildingBlocks.Exceptions.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Cấu hình JWT Authentication Middleware
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/identity/test", () => new 
{
    Message = "Hello from Identity Service (chạy trên .NET 10)!",
    Timestamp = DateTime.UtcNow
});

SeoAuto.IdentityService.Features.Auth.Register.RegisterUserEndpoint.MapRegisterUserEndpoint(app);
SeoAuto.IdentityService.Features.Auth.Login.LoginUserEndpoint.MapLoginUserEndpoint(app);
SeoAuto.IdentityService.Features.Users.GetProfile.GetProfileEndpoint.MapGetProfileEndpoint(app);
SeoAuto.IdentityService.Features.Users.UpdateProfile.UpdateProfileEndpoint.MapUpdateProfileEndPoint(app);
SeoAuto.IdentityService.Features.Users.ChangePassword.ChangePasswordEndpoint.MapChangePasswordEndpoint(app);
SeoAuto.IdentityService.Features.Auth.RefreshTokenEndpoint.MapRefreshTokenEndpoint(app);

app.Run();
