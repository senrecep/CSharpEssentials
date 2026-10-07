namespace CSharpEssentials.Mediator;

/// <summary>
/// Controls what <see cref="ValidationBehavior{TRequest, TResponse}"/> does with a request.
/// </summary>
public enum ValidationMode
{
    /// <summary>Validation failures stop the request and are returned as a failed result.</summary>
    Enforce = 0,

    /// <summary>Validators run and failures are reported to the observers, but the request continues.</summary>
    LogOnly = 1,

    /// <summary>Validators do not run.</summary>
    Off = 2
}
