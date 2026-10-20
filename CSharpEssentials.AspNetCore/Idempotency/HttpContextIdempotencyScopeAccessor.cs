using System.Security.Claims;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>Gives the user of the current request to idempotency scope resolvers outside the HTTP middleware, such as the Mediator behavior.</summary>
internal sealed class HttpContextIdempotencyScopeAccessor(IHttpContextAccessor httpContextAccessor) : IIdempotencyScopeAccessor
{
    public ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
}
