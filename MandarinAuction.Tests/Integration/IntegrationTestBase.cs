using MandarinAuction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Tests.Integration;

public class IntegrationTestBase : IDisposable
{
    protected readonly ApplicationDbContext DbContext;

    protected IntegrationTestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        DbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }
    
}