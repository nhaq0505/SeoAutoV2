var builder = WebApplication.CreateBuilder(args);

// Cấu hình CORS cho phép Frontend (http://localhost:3000) gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 1. Đăng ký dịch vụ YARP vào hệ thống, đọc cấu hình từ appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("AllowAll");

// 2. Kích hoạt luồng xử lý của YARP
app.MapReverseProxy();

app.Run();