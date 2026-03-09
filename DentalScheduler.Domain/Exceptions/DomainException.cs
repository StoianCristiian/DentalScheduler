namespace DentalScheduler.Domain.Exceptions;

/// <summary>
/// Exceptie aruncata cand o operatie nu este permisa din cauza starii domeniului.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

