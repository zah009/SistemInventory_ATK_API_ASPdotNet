using System.Threading.Tasks;
using Atk.Models;

public interface IAuthService
{
    string GenerateJwtToken(User user);

    Task<(bool Success, string Message, UserRole? Role, string? Nama, string? Divisi, int? UserId)>
        LoginAsync(string username, string password);
}
