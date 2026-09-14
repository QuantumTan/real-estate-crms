using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.domain.entities;

namespace CRMS_Peguit.infrastructure.data
{
    public class MasterCrmsDbContext : DbContext
    {
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<CompanyDatabase> CompanyDatabases => Set<CompanyDatabase>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<SuperAdmin> SuperAdmins => Set<SuperAdmin>();
        public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();

        public MasterCrmsDbContext(
            DbContextOptions<MasterCrmsDbContext> options
        ) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Company>(entity =>
            {
                entity.HasKey(x => x.CompanyId);

                entity.Property(x => x.CompanyCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.CompanyName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.HasIndex(x => x.CompanyCode)
                    .IsUnique();
            });

            builder.Entity<CompanyDatabase>(entity =>
            {
                entity.HasKey(x => x.CompanyDatabaseId);

                entity.Property(x => x.ServerName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.DatabaseName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.CredentialKey)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Device>(entity =>
            {
                entity.HasKey(x => x.DeviceId);

                entity.Property(x => x.DeviceCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DeviceName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.HasOne(x => x.Company)
                    .WithMany(c => c.Devices)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.CompanyId, x.DeviceCode })
                    .IsUnique();
            });

            builder.Entity<Subscription>(entity =>
            {
                entity.HasKey(x => x.SubscriptionId);

                entity.Property(x => x.PlanName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.BillingAmount).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Status).HasMaxLength(50);

                entity.HasOne(x => x.Company)
                    .WithMany(c => c.Subscriptions)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SuperAdmin>(entity =>
            {
                entity.HasKey(x => x.SuperAdminId);

                entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
                entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
                entity.Property(x => x.FirstName).HasMaxLength(100);
                entity.Property(x => x.LastName).HasMaxLength(100);

                entity.HasIndex(x => x.Email).IsUnique();
            });

            builder.Entity<GlobalSetting>(entity =>
            {
                entity.HasKey(x => x.GlobalSettingId);
                entity.Property(x => x.SettingKey).HasMaxLength(200).IsRequired();
                entity.Property(x => x.SettingValue).HasMaxLength(2000);
                entity.HasIndex(x => x.SettingKey).IsUnique();

                entity.HasOne(x => x.UpdatedBySuperAdmin)
                    .WithMany()
                    .HasForeignKey(x => x.UpdatedBySuperAdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}