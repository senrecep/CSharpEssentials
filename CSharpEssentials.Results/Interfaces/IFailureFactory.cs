using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern.Interfaces;

internal interface IFailureFactory<out TSelf>
{
    TSelf CreateFailure(Error[] errors);
}
