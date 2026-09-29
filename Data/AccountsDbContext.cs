using Microsoft.EntityFrameworkCore;
using AccountEntity = Account.Domain.Account;

namespace Account.Data;

public class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : DbContext(options)
{
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccountEntity>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id)
                .HasColumnName("id");

            entity.Property(a => a.Email)
                .HasColumnName("email")
                .HasMaxLength(320)
                .IsRequired();

            entity.HasIndex(a => a.Email)
                .IsUnique();

            entity.Property(a => a.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(a => a.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(a => a.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
        });
    }
}
