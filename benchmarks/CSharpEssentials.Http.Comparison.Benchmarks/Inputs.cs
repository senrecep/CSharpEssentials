using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>Inputs shared by every library: one Web options instance, the stub's bodies and the request values.</summary>
[SuppressMessage("Major Code Smell", "S1075:URIs should not be hardcoded", Justification = "Fixed address of the in-memory stub; no request leaves the process.")]
internal static class Inputs
{
    public static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);

    public static readonly Uri BaseAddress = new("https://bench.local/");

    public static readonly Uri UsersUri = new("users", UriKind.Relative);

    public static readonly Uri MissingUri = new("missing", UriKind.Relative);

    public static readonly NewUser NewUser = new("Ada", "ada@bench.local", 36);

    public static readonly User CreatedUser = new(42, "Ada", "ada@bench.local", 36);

    public static readonly byte[] SingleUserBody = JsonSerializer.SerializeToUtf8Bytes(CreatedUser, WebOptions);

    public static readonly byte[] NotFoundBody = Encoding.UTF8.GetBytes("""{"title":"Not Found","status":404}""");

    public const string NewUserJson = """{"name":"Ada","email":"ada@bench.local","age":36}""";

    public static readonly SearchQuery Search = new() { Q = "c# tips", Page = 2, Include = "profile" };

    public static readonly Dictionary<string, string?> SearchDictionary = new()
    {
        ["Q"] = "c# tips",
        ["Page"] = "2",
        ["Include"] = "profile",
    };

    public const string SearchQueryString = "Q=c%23%20tips&Page=2&Include=profile";

    public const string SearchUriString = "https://bench.local/search";

    public static readonly Uri SearchUri = new(SearchUriString);

    public static readonly Task<int> Done = Task.FromResult(42);

    public static readonly Task<Result<int>> DoneResult = Task.FromResult(Result<int>.Success(42));

    public static readonly Task<Result<int>> FailedResult = Task.FromResult(Result<int>.Failure(Error.Unexpected("Bench.Transient")));

    public static User[] Users(int count) =>
        [.. Enumerable.Range(1, count).Select(i => new User(i, $"user{i}", $"user{i}@bench.local", 20 + i % 50))];

    public static byte[] UsersBody(int count) => JsonSerializer.SerializeToUtf8Bytes(Users(count), WebOptions);

    public static string Describe(User[] users) => $"{users.Length}|{users[0]}|{users[^1]}";

    public static T Run<T>(Task<T> task) => task.GetAwaiter().GetResult();

    public static T Run<T>(ValueTask<T> task) => task.AsTask().GetAwaiter().GetResult();
}
