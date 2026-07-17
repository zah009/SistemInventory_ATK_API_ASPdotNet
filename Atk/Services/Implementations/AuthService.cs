using System.Threading.Tasks;
using Atk.Data;
using Atk.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ===============================
    // LOGIN LOGIC
    // ===============================
    public async Task<(bool Success, string Message, UserRole? Role, string? Nama, string? Divisi, int? UserId)>
        LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
            return (false, "Username tidak ditemukan", null, null, null, null);

        bool validPassword;
        try
        {
            validPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
        }
        catch
        {
            return (false, "Password hash invalid di database", null, null, null, null);
        }

        if (!validPassword)
            return (false, "Password salah", null, null, null, null);

        return (true, "Login berhasil", user.Role, user.Nama, user.NamaDivisi, user.Id);
    }

    // ===============================
    // JWT GENERATOR
    // ===============================
    public string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "YourVerySecretKey123";
        var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim("nama", user.Nama ?? ""),
            new Claim("divisi", user.NamaDivisi ?? ""),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(6),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }
}
