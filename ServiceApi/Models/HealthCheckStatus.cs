namespace ServiceApi.Models
{
    public class HealthCheckStatus
    {
        public int Id { get; set; }
        public string CheckName { get; set; }

        public string Status { get; set; }

        public string Description { get; set; }

        public double DurationMs { get; set; }

        public DateTime CheckedAt { get; set; }
    }
}
