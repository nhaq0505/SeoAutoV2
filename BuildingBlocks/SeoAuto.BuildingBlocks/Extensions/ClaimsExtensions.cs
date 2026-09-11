using System.Security.Claims;
using SeoAuto.BuildingBlocks.Exceptions;

namespace SeoAuto.BuildingBlocks.Extensions;

public static class ClaimsExtensions
{
    /// <summary>
    /// Trích xuất và parse an toàn UserId (Guid) từ ClaimsPrincipal (hỗ trợ cả ClaimTypes.NameIdentifier và "sub").
    /// Ném ra UnAuthorizedException nếu token không hợp lệ hoặc thiếu UserId.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new UnAuthorizedException("UserId không hợp lệ hoặc không tìm thấy trong token.");
        }
        return userId;
    }
}
