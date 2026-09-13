using System.Threading.Tasks;

namespace CRMS_Peguit.infrastructure.Services
{
    public interface ITenantDatabaseResolver
    {
        Task<TenantDatabaseInfo> GetDatabaseInfoAsync(int companyId);
    }
}
