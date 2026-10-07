using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Entity.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CSharpEssentials.EntityFrameworkCore.Tests;

public sealed class NamedQueryFilterExtensionsTests : IDisposable
{
    private const string CurrentTenant = "acme";

    private static readonly string[] SoftDeleteOnly = [QueryFilterNames.SoftDelete];

    private readonly SqliteConnection _connection;

    public NamedQueryFilterExtensionsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Helpers_Should_Register_Named_SoftDelete_Filter_When_Configured(bool modelWide)
    {
        using DocumentDbContext context = CreateContext(RegistrationFor(modelWide));

        IReadOnlyCollection<IQueryFilter> filters = context.Model.FindEntityType(typeof(Document))!.GetDeclaredQueryFilters();

        filters.Select(static filter => filter.Key).Should().BeEquivalentTo(QueryFilterNames.SoftDelete, QueryFilterNames.Tenant);
        filters.Should().OnlyContain(static filter => !filter.IsAnonymous);
    }

    [Fact]
    public void ApplyNamedSoftDeleteQueryFilter_Should_Skip_Entities_That_Are_Not_Soft_Deletable()
    {
        using DocumentDbContext context = CreateContext(FilterRegistration.ModelBuilder);

        IEntityType tag = context.Model.FindEntityType(typeof(Tag))!;

        tag.FindDeclaredQueryFilter(QueryFilterNames.SoftDelete).Should().BeNull();
    }

    [Fact]
    public void ApplyNamedSoftDeleteQueryFilter_Should_Register_The_Filter_On_The_Root_When_Entity_Has_Derived_Types()
    {
        using DocumentDbContext context = CreateContext(FilterRegistration.ModelBuilder);

        context.Model.FindEntityType(typeof(Invoice))!.GetDeclaredQueryFilters().Should().BeEmpty();
        context.Model.FindEntityType(typeof(Document))!.FindDeclaredQueryFilter(QueryFilterNames.SoftDelete).Should().NotBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Query_Should_Hide_Soft_Deleted_Rows_When_Named_Filter_Is_Active(bool modelWide)
    {
        await SeedAsync(RegistrationFor(modelWide));
        await using DocumentDbContext context = CreateContext(RegistrationFor(modelWide));

        List<string> names = await context.Documents.OrderBy(static document => document.Name).Select(static document => document.Name).ToListAsync();

        names.Should().Equal("active", "invoice");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IgnoreQueryFilters_Should_Return_Soft_Deleted_Rows_And_Keep_Tenant_Filter_When_Only_SoftDelete_Is_Ignored(bool modelWide)
    {
        await SeedAsync(RegistrationFor(modelWide));
        await using DocumentDbContext context = CreateContext(RegistrationFor(modelWide));

        List<string> names = await context.Documents
            .IgnoreQueryFilters(SoftDeleteOnly)
            .OrderBy(static document => document.Name)
            .Select(static document => document.Name)
            .ToListAsync();

        names.Should().Equal("active", "deleted", "invoice");
    }

    [Fact]
    public async Task IgnoreSoftDeleteQueryFilter_Should_Return_Soft_Deleted_Rows_And_Keep_Tenant_Filter()
    {
        await SeedAsync(FilterRegistration.EntityTypeBuilder);
        await using DocumentDbContext context = CreateContext(FilterRegistration.EntityTypeBuilder);

        List<string> names = await context.Documents
            .IgnoreSoftDeleteQueryFilter()
            .OrderBy(static document => document.Name)
            .Select(static document => document.Name)
            .ToListAsync();

        names.Should().Equal("active", "deleted", "invoice");
    }

    [Fact]
    [SuppressMessage(
        "CSharpEssentials.EntityFrameworkCore",
        "CSE3001:IgnoreQueryFilters() ignores every query filter",
        Justification = "The test pins down that the parameterless overload drops every filter, the tenant filter included.")]
    public async Task IgnoreQueryFilters_Should_Return_Every_Row_When_No_Filter_Name_Is_Given()
    {
        await SeedAsync(FilterRegistration.EntityTypeBuilder);
        await using DocumentDbContext context = CreateContext(FilterRegistration.EntityTypeBuilder);

        int count = await context.Documents.IgnoreQueryFilters().CountAsync();

        count.Should().Be(4);
    }

    [Fact]
    public void Model_Should_Reject_Anonymous_Filter_When_Named_SoftDelete_Filter_Is_Registered()
    {
        using DocumentDbContext context = CreateContext(FilterRegistration.Mixed);

        Func<IModel> act = () => context.Model;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ApplyNamedSoftDeleteQueryFilter_Should_Skip_Owned_Types()
    {
        using DocumentDbContext context = CreateContext(FilterRegistration.OwnedType);

        IEntityType attachment = context.Model.FindEntityType(typeof(Folder))!.FindNavigation(nameof(Folder.Attachment))!.TargetEntityType;

        attachment.IsOwned().Should().BeTrue();
        attachment.GetDeclaredQueryFilters().Should().BeEmpty();
        context.Model.FindEntityType(typeof(Document))!.FindDeclaredQueryFilter(QueryFilterNames.SoftDelete).Should().NotBeNull();
    }

    [Fact]
    public async Task IgnoreSoftDeleteQueryFilter_Should_Keep_Hiding_Soft_Deleted_Rows_When_Soft_Delete_Filter_Is_Anonymous()
    {
        await SeedAsync(FilterRegistration.AnonymousSoftDelete);
        await using DocumentDbContext context = CreateContext(FilterRegistration.AnonymousSoftDelete);

        List<string> names = await context.Documents
            .IgnoreSoftDeleteQueryFilter()
            .OrderBy(static document => document.Name)
            .Select(static document => document.Name)
            .ToListAsync();

        names.Should().Equal("active", "invoice", "other-tenant");
    }

    [Fact]
    public void ApplyNamedSoftDeleteQueryFilter_Should_Throw_When_Entity_Already_Has_Anonymous_Filter()
    {
        using DocumentDbContext context = CreateContext(FilterRegistration.AnonymousThenNamed);

        Func<IModel> act = () => context.Model;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already has an anonymous query filter*'SoftDelete'*");
    }

    private static FilterRegistration RegistrationFor(bool modelWide) =>
        modelWide ? FilterRegistration.ModelBuilder : FilterRegistration.EntityTypeBuilder;

    private DocumentDbContext CreateContext(FilterRegistration registration) =>
        new(
            new DbContextOptionsBuilder<DocumentDbContext>()
                .UseSqlite(_connection)
                .ReplaceService<IModelCacheKeyFactory, RegistrationModelCacheKeyFactory>()
                .Options,
            registration);

    private async Task SeedAsync(FilterRegistration registration)
    {
        await using DocumentDbContext context = CreateContext(registration);
        await context.Database.EnsureCreatedAsync();
        context.Documents.AddRange(
            new Document { Name = "active", TenantId = CurrentTenant },
            new Document { Name = "deleted", TenantId = CurrentTenant, IsDeleted = true },
            new Document { Name = "other-tenant", TenantId = "globex" },
            new Invoice { Name = "invoice", TenantId = CurrentTenant });
        await context.SaveChangesAsync();
    }

    private enum FilterRegistration
    {
        EntityTypeBuilder,
        ModelBuilder,
        Mixed,
        OwnedType,
        AnonymousSoftDelete,
        AnonymousThenNamed,
    }

    internal class Document : ISoftDeletableBase
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string TenantId { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
    }

    internal sealed class Invoice : Document;

    internal sealed class Tag
    {
        public int Id { get; set; }
    }

    internal sealed class Folder
    {
        public int Id { get; set; }

        public Attachment? Attachment { get; set; }
    }

    internal sealed class Attachment : ISoftDeletableBase
    {
        public string FileName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
    }

    private sealed class DocumentDbContext(DbContextOptions<DocumentDbContext> options, FilterRegistration registration) : DbContext(options)
    {
        public FilterRegistration Registration { get; } = registration;

        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>();
            modelBuilder.Entity<Invoice>();
            if (Registration is FilterRegistration.AnonymousSoftDelete or FilterRegistration.AnonymousThenNamed)
            {
                modelBuilder.ApplySoftDeleteQueryFilter();
                if (Registration == FilterRegistration.AnonymousThenNamed)
                    modelBuilder.ApplyNamedSoftDeleteQueryFilter();

                return;
            }

            modelBuilder.Entity<Document>().HasQueryFilter(QueryFilterNames.Tenant, static document => document.TenantId == CurrentTenant);
            if (Registration is FilterRegistration.ModelBuilder or FilterRegistration.OwnedType)
            {
                if (Registration == FilterRegistration.OwnedType)
                    modelBuilder.Entity<Folder>().OwnsOne(static folder => folder.Attachment);

                modelBuilder.ApplyNamedSoftDeleteQueryFilter();
                return;
            }

            modelBuilder.Entity<Document>().HasSoftDeleteQueryFilter();
            if (Registration == FilterRegistration.Mixed)
                modelBuilder.Entity<Document>().HasQueryFilter(static document => document.Name != string.Empty);
        }
    }

    // Each registration builds a different model for the same context type, so the registration is part of the model cache key.
    private sealed class RegistrationModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) =>
            (context.GetType(), ((DocumentDbContext)context).Registration, designTime);
    }
}
