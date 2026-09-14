using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRMS_Peguit.infrastructure.data
{
    public class RealEstateDbContextFactory
        : IDesignTimeDbContextFactory<RealEstateDbContext>
    {
        public RealEstateDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RealEstateDbContext>();

            var connectionString =
                Environment.GetEnvironmentVariable("CRMS_CONNECTION")
                ?? "Server=(localdb)\\mssqllocaldb;Database=CRMS_Local;Trusted_Connection=True;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new RealEstateDbContext(optionsBuilder.Options);
        }
    }
}