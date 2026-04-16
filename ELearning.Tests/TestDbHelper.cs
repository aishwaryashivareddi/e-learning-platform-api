using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Tests;

public static class TestDbHelper
{
    public static ELearningDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ELearningDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var ctx = new ELearningDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
