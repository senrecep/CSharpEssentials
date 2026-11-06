using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class DbContextResultExtensionsTests : IDisposable
{
    private sealed class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class WidgetDbContext(DbContextOptions<WidgetDbContext> options) : DbContext(options)
    {
        public DbSet<Widget> Widgets => Set<Widget>();
    }

    private readonly WidgetDbContext _context = new(
        new DbContextOptionsBuilder<WidgetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public void Dispose() => _context.Dispose();

    private async Task SeedAsync(params Widget[] widgets)
    {
        _context.Widgets.AddRange(widgets);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task FirstOrDefaultAsResultAsync_Should_ReturnEntity_When_RowExists()
    {
        await SeedAsync(new Widget { Id = 1, Name = "first" }, new Widget { Id = 2, Name = "second" });

        Result<Widget> result = await _context.Widgets.OrderBy(w => w.Id).FirstOrDefaultAsResultAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("first");
    }

    [Fact]
    public async Task FirstOrDefaultAsResultAsync_Should_ReturnNotFound_When_NoRowMatches()
    {
        await SeedAsync(new Widget { Id = 1, Name = "first" });

        Result<Widget> result = await _context.Widgets.Where(w => w.Name == "missing").FirstOrDefaultAsResultAsync();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task FirstOrDefaultAsResultAsync_Should_ReturnGivenError_When_NoRowMatches()
    {
        Error notFound = Error.NotFound("Widget.Missing", "No widget.");

        Result<Widget> result = await _context.Widgets.FirstOrDefaultAsResultAsync(notFound);

        result.FirstError.Should().Be(notFound);
    }

    [Fact]
    public async Task SingleOrDefaultAsResultAsync_Should_ReturnEntity_When_OneRowMatches()
    {
        await SeedAsync(new Widget { Id = 1, Name = "first" }, new Widget { Id = 2, Name = "second" });

        Result<Widget> result = await _context.Widgets.Where(w => w.Id == 2).SingleOrDefaultAsResultAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("second");
    }

    [Fact]
    public async Task SingleOrDefaultAsResultAsync_Should_ReturnNotFound_When_NoRowMatches()
    {
        Result<Widget> result = await _context.Widgets.SingleOrDefaultAsResultAsync();

        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task SingleOrDefaultAsResultAsync_Should_ReturnGivenError_When_NoRowMatches()
    {
        Error notFound = Error.NotFound("Widget.Missing", "No widget.");

        Result<Widget> result = await _context.Widgets.SingleOrDefaultAsResultAsync(notFound);

        result.FirstError.Should().Be(notFound);
    }

    [Fact]
    public async Task SingleOrDefaultAsResultAsync_Should_Throw_When_SeveralRowsMatch()
    {
        await SeedAsync(new Widget { Id = 1, Name = "first" }, new Widget { Id = 2, Name = "second" });

        Func<Task> act = () => _context.Widgets.SingleOrDefaultAsResultAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task FindAsResultAsync_Should_ReturnEntity_When_KeyExists()
    {
        await SeedAsync(new Widget { Id = 7, Name = "seven" });

        Result<Widget> result = await _context.Widgets.FindAsResultAsync([7]);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("seven");
    }

    [Fact]
    public async Task FindAsResultAsync_Should_ReturnNotFound_When_KeyIsMissing()
    {
        Result<Widget> result = await _context.Widgets.FindAsResultAsync([42]);

        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task FindAsResultAsync_Should_ReturnGivenError_When_KeyIsMissing()
    {
        Error notFound = Error.NotFound("Widget.Missing", "No widget.");

        Result<Widget> result = await _context.Widgets.FindAsResultAsync([42], notFound);

        result.FirstError.Should().Be(notFound);
    }

    [Fact]
    public async Task SaveChangesAsResultAsync_Should_Succeed_And_Persist_When_SaveWorks()
    {
        _context.Widgets.Add(new Widget { Id = 1, Name = "saved" });

        Result result = await _context.SaveChangesAsResultAsync();

        result.IsSuccess.Should().BeTrue();
        _context.ChangeTracker.Clear();
        (await _context.Widgets.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsResultAsync_Should_ReturnUnknownError_When_SaveThrows()
    {
        await SeedAsync(new Widget { Id = 1, Name = "first" });
        _context.Widgets.Add(new Widget { Id = 1, Name = "duplicate" });

        Result result = await _context.SaveChangesAsResultAsync();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unknown);
    }

    [Fact]
    public async Task SaveChangesAsResultAsync_WithAcceptAll_Should_ReturnRowCount_When_SaveWorks()
    {
        _context.Widgets.AddRange(new Widget { Id = 1, Name = "a" }, new Widget { Id = 2, Name = "b" });

        Result<int> result = await _context.SaveChangesAsResultAsync(acceptAllChangesOnSuccess: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task SaveChangesAsResultAsync_WithAcceptAll_Should_ReturnUnknownError_When_SaveThrows()
    {
        await SeedAsync(new Widget { Id = 1, Name = "first" });
        _context.Widgets.Add(new Widget { Id = 1, Name = "duplicate" });

        Result<int> result = await _context.SaveChangesAsResultAsync(acceptAllChangesOnSuccess: true);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unknown);
    }
}
