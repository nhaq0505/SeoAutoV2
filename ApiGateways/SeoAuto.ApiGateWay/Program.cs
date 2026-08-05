var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký dịch vụ YARP vào hệ thống, đọc cấu hình từ appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 2. Kích hoạt luồng xử lý của YARP
app.MapReverseProxy();

app.Run();