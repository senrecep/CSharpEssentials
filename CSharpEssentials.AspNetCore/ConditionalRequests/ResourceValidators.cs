using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The validators of a resource representation (RFC 9110 section 8.8): sent as <c>ETag</c> and <c>Last-Modified</c> and compared
/// with the request's <c>If-None-Match</c>, <c>If-Modified-Since</c> and <c>If-Match</c> headers.
/// </summary>
/// <param name="ETag">The entity tag, strong or weak; <see langword="null"/> for none.</param>
/// <param name="LastModified">The last modification time; sent and compared with second precision. <see langword="null"/> for none.</param>
public sealed record ResourceValidators(EntityTagHeaderValue? ETag, DateTimeOffset? LastModified);
