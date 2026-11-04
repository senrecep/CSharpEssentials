using Refit;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

public interface IUsersApi
{
    [Get("/users")]
    Task<User[]> GetUsersAsync();

    [Post("/users")]
    Task<User> CreateUserAsync([Body] NewUser user);

    [Get("/missing")]
    Task<IApiResponse<User>> GetMissingAsync();
}
