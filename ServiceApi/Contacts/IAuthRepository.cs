namespace ServiceApi.Contacts
{
    public interface IAuthRepository
    {
        string GenerateJwtToken(string username);
    }
}