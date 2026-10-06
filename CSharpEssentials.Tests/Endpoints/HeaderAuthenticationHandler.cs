using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Tests.Endpoints;

internal sealed class HeaderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Header";
    public const string RolesHeader = "X-Roles";
    public const string ClaimsHeader = "X-Claims";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RolesHeader, out var roles) && !Request.Headers.ContainsKey(ClaimsHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims =
        [
            new(ClaimTypes.Name, "user"),
            .. roles.SelectMany(static value => value!.Split(',')).Select(static role => new Claim(ClaimTypes.Role, role)),
            .. Request.Headers[ClaimsHeader].SelectMany(static value => value!.Split(',')).Select(static type => new Claim(type, "true")),
        ];
        ClaimsPrincipal principal = new(new ClaimsIdentity(claims, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
