namespace DentalScheduler.Domain.Common;

/// <summary>
/// Interfata pentru entitati care au timestamp-uri de audit.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

