using CSharpEssentials.Errors;

namespace CSharpEssentials.Exceptions;

public class DomainException(Error error) : Exception(error.Description)
{
    public Error Error => error;
}
