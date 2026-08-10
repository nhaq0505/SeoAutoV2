using Microsoft.EntityFrameworkCore;
using SeoAuto.AuditService.Infrastructure.Database;
using SeoAuto.BuildingBlocks.Exceptions;
using SeoAuto.BuildingBlocks.Messaging;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký OpenAPI/Swagger
builder.Services.AddOpenApi();

// 2. Kích hoạt Bẫy lỗi toàn cục (Bắt mọi Exception và trả về chuẩn JSON ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AuditDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Kích hoạt RabbitMQ (Xe chở thư)
builder.Services.AddMessageBroker(builder.Configuration, typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 4. Bật Middleware bẫy lỗi lên
app.UseExceptionHandler();

app.UseHttpsRedirection();

// API Test thử nghiệm
app.MapGet("/api/audits/ping", () => "AuditService is running with RabbitMQ!");

app.Run();