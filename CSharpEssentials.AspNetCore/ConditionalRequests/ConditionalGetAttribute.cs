namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Turns on conditional GET for an MVC controller or action: a successful <c>GET</c>/<c>HEAD</c> response with a value gets
/// <c>ETag</c>/<c>Last-Modified</c> from the value's validators, and a matching <c>If-None-Match</c> (or, without it, a not
/// older <c>If-Modified-Since</c>) returns <c>304 Not Modified</c>. Minimal API endpoints and groups use
/// <see cref="ConditionalRequestExtensions.WithConditionalGet{TBuilder}(TBuilder)"/>, which adds this attribute as metadata
/// (also honored on MVC endpoints, for example <c>MapControllers().WithConditionalGet()</c>).
/// Requires <see cref="ConditionalRequestExtensions.AddConditionalRequests"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ConditionalGetAttribute : Attribute;
