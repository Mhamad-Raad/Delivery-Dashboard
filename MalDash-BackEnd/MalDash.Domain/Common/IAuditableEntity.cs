namespace MalDash.Domain.Common
{
    public interface IAuditableEntity
    {
        Guid? CreatedBy { get; set; }
        DateTime CreatedAt { get; set; }
        Guid? LastModifiedBy { get; set; }
        DateTime? LastModifiedAt { get; set; }
    }
}