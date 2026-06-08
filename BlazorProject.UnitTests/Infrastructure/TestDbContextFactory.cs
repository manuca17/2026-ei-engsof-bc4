using BlazorProject.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorProject.UnitTests.Infrastructure;

internal sealed class TestEiEngsofContext : EiEngsofContext
{
    public TestEiEngsofContext(DbContextOptions<EiEngsofContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
}

internal sealed class TestDbContextFactory : IDbContextFactory<EiEngsofContext>
{
    private readonly DbContextOptions<EiEngsofContext> _options;

    public TestDbContextFactory(DbContextOptions<EiEngsofContext> options)
    {
        _options = options;
    }

    public EiEngsofContext CreateDbContext()
    {
        return new TestEiEngsofContext(_options);
    }

    public Task<EiEngsofContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<EiEngsofContext>(new TestEiEngsofContext(_options));
    }
}