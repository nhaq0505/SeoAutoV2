using System;
using SeoAuto.BuildingBlocks.Domain;

namespace SeoAuto.IdentityService.Domain.Entities;

public class User : ISoftDelete
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Soft Delete Properties
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
