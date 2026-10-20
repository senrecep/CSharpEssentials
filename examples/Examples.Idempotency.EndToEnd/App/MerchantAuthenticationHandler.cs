using System.Security.Claims;
using System.Text.Encodings.Web;

using CSharpEssentials.Idempotency;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// Stands in for real machine authentication (client credentials, API keys): the <c>X-Merchant-Id</c> header becomes
/// the <c>client_id</c> claim, which <see cref="IdempotencyScopes.Client"/> turns into the idempotency scope, so two
/// merchants that pick the same key never see each other's responses.
/// </summary>
public sealed class MerchantAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Merchant";
    public const string HeaderName = "X-Merchant-Id";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? merchantId = Request.Headers[HeaderName];
        if (string.IsNullOrEmpty(merchantId))
            return Task.FromResult(AuthenticateResult.NoResult());

        ClaimsIdentity identity = new([new Claim(IdempotencyScopes.ClientIdClaimType, merchantId)], SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
