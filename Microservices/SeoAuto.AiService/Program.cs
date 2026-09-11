using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SeoAuto.AiService.Features;
using SeoAuto.AiService.Features.Gemini;
using SeoAuto.BuildingBlocks.Exceptions;
using SeoAuto.BuildingBlocks.Messaging;

var builder = WebApplication.CreateBuilder(args);

// 1. OpenAPI & Scalar
builder.Services.AddOpenApi();

// 2. Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Register GeminiClient with HttpClient and Resilience/Timeout
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(35); // FR-402: Giới hạn timeout 30-35s
})
.AddStandardResilienceHandler();

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

// 6. Map Endpoints
app.MapAiEndpoints();

app.Run();
