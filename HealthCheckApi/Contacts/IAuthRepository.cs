namespace HealthCheckerApi.Contacts
{
    public interface IAuthRepository
    {
        string GenerateJwtToken(string username);
    }
}