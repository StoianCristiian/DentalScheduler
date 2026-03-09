namespace DentalScheduler.Domain.Common;

/// <summary>
/// Clasa de baza pentru toate entitatile domeniului.
/// Orice entitate are un Id unic de tip Guid.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
}

