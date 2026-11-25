namespace CSharpEssentials.Locking.Testing;

/// <summary>Thrown by a <see cref="ResourceLockContract"/> scenario when the lock under test breaks the contract.</summary>
public sealed class ResourceLockContractException : Exception
{
    /// <summary>Creates the exception.</summary>
    public ResourceLockContractException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">What the lock did wrong.</param>
    public ResourceLockContractException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception the lock threw.</summary>
    /// <param name="message">What the lock did wrong.</param>
    /// <param name="innerException">The exception the lock threw.</param>
    public ResourceLockContractException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
