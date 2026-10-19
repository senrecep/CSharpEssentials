using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

internal sealed record CapturedLog(string Category, LogLevel Level, EventId EventId, string Message);
