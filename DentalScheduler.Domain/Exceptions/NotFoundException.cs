namespace DentalScheduler.Domain.Exceptions;

/// <summary>
/// Exceptie aruncata cand o entitate nu este gasita in baza de date.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Entitatea '{entityName}' cu cheia '{key}' nu a fost gasita.")
    {
    }
}

