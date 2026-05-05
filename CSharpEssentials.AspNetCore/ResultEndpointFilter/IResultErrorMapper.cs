using CSharpEssentials.Errors;

namespace CSharpEssentials.AspNetCore;

public interface IResultErrorMapper
{
    Microsoft.AspNetCore.Http.IResult Map(Error[] errors);
}
