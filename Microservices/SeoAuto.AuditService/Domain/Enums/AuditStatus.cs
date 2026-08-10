namespace SeoAuto.AuditService.Domain.Enums;

public enum AuditStatus
{
    Pending = 0,    // Đang chờ xử lý
    Processing = 1, // Đang phân tích
    Completed = 2,  // Hoàn thành
    Failed = 3      // Thất bại / Lỗi
}