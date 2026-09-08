using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.infrastructure.data;

namespace CRMS_Peguit.winforms.Models.Services
{
    public static class LocalDb
    {
        public static string ConnectionString =>
            DbConfiguration.GetLocalConnectionString();

        public static RealEstateDbContext CreateContext(int tenantId = 1)
        {
            var options = new DbContextOptionsBuilder<RealEstateDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            return new RealEstateDbContext(options, tenantId: tenantId);
        }
    }
}