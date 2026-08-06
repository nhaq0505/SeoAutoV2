namespace SeoAuto.BuildingBlocks.Domain;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
