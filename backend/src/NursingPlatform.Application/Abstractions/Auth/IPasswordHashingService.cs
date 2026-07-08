namespace NursingPlatform.Application.Abstractions.Auth;

public interface IPasswordHashingService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
