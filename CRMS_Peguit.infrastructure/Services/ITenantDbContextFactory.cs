using System.Threading.Tasks;
using CRMS_Peguit.infrastructure.data;

namespace CRMS_Peguit.infrastructure.Services
{
    public interface ITenantDbContextFactory
    {
        Task<RealEstateDbContext> CreateAsync(int companyId);
    }
}
