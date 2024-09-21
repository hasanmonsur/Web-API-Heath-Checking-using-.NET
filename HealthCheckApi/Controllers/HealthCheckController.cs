using HealthCheckerApi.Contacts;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheckerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly IHealthCheckRepository _repository;

        public HealthCheckController(IHealthCheckRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetHealthStatus()
        {
            var healthStatus = await _repository.GetLatestHealthStatusAsync();
            if (healthStatus == null)
            {
                return NotFound("No health status found.");
            }
            return Ok(healthStatus);
        }
    }
}
