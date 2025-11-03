using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AndBank.Infrastructure.Data;

public class AndBankDbContextFactory : IDesignTimeDbContextFactory<AndBankDbContext>
{
    public AndBankDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AndBankDbContext>();
        builder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=dev#123456",
            b => b.MigrationsAssembly("AndBank.Infrastructure"));
        return new AndBankDbContext(builder.Options);
    }
}