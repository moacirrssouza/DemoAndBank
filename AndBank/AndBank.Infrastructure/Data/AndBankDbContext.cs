using AndBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AndBank.Infrastructure.Data;

public class AndBankDbContext : DbContext
{
    public AndBankDbContext(DbContextOptions<AndBankDbContext> options) : base(options) { }

    public DbSet<Position> Positions { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var entity = modelBuilder.Entity<Position>();
        entity.ToTable("positions");

        entity.HasKey(p => new { p.PositionId, p.Date });

        entity.Property(p => p.PositionId).HasColumnName("position_id").IsRequired();
        entity.Property(p => p.ProductId).HasColumnName("product_id").IsRequired();
        entity.Property(p => p.ClientId).HasColumnName("client_id").IsRequired();
        entity.Property(p => p.Date).HasColumnName("date").IsRequired();
        entity.Property(p => p.Value).HasColumnName("value").IsRequired().HasColumnType("numeric");
        entity.Property(p => p.Quantity).HasColumnName("quantity").IsRequired().HasColumnType("numeric");

        entity.HasIndex(p => p.ClientId).HasDatabaseName("idx_positions_client");
        entity.HasIndex(p => p.Value).HasDatabaseName("idx_positions_value");
        entity.HasIndex(p => new { p.PositionId, p.Date }).HasDatabaseName("idx_positions_positionid_date");
    }
}