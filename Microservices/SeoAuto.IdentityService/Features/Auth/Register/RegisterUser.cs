using Microsoft.EntityFrameworkCore;
using SeoAuto.IdentityService.Domain.Entities;
using SeoAuto.IdentityService.Infrastructure.Database;

namespace SeoAuto.IdentityService.Features.Auth.Register;

public record RegisterUserRequest(string Email, string Password, string FullName);
public record RegisterUserResponse(Guid UserId, string Message);

public static class RegisterUserEndpoint
{
    public static void MapRegisterUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterUserRequest request, AppDbContext dbContext) =>
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { Message = "Email và Mật khẩu không được để trống." });
            }

            // 2. Kiểm tra Email đã tồn tại chưa
            var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return Results.Conflict(new { Message = "Email này đã được sử dụng." });
            }

            // 3. Mã hóa mật khẩu bằng BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 4. Tạo User mới
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/users/{newUser.Id}", new RegisterUserResponse(newUser.Id, "Đăng ký tài khoản thành công!"));
        })
        .WithName("RegisterUser")
        .WithTags("Auth");
    }
}
