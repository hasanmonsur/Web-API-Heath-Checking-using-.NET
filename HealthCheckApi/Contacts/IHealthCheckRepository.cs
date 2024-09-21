using HealthCheckerApi.Models;

namespace HealthCheckerApi.Contacts
{
    public interface IHealthCheckRepository
    {
        public Task<List<HealthCheckStatus>> GetLatestHealthStatusAsync();
    }
}