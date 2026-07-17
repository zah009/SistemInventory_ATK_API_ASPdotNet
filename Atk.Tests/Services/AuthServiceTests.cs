using System;
using System.Threading.Tasks;
using Atk.Data;
using Atk.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Atk.Tests.Services
{
public class AuthServiceTests
{
private async Task<ApplicationDbContext> GetInMemoryDbContext()
{
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
.UseInMemoryDatabase(Guid.NewGuid().ToString())
.Options;


        var context = new ApplicationDbContext(options);

        // Seed user
        context.Users.Add(new User
        {
            Id = 1,
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
            Nama = "Admin User",
            Role = UserRole.Admin
        });

        await context.SaveChangesAsync();
        return context;
    }

    private IConfiguration GetTestConfiguration()
    {
        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string> {
            {"Jwt:Key", "ThisIsATestSecretKeyForJWT1234567890"} // 36+ chars
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }


    [Fact]
    public async Task LoginAsync_ReturnsFalse_WhenUsernameNotFound()
    {
        var context = await GetInMemoryDbContext();
        var config = GetTestConfiguration();
        var service = new AuthService(context, config);

        var result = await service.LoginAsync("nonexistent", "anyPassword");

        Assert.False(result.Success);
        Assert.Equal("Username tidak ditemukan", result.Message);
        Assert.Null(result.Role);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFalse_WhenPasswordIncorrect()
    {
        var context = await GetInMemoryDbContext();
        var config = GetTestConfiguration();
        var service = new AuthService(context, config);

        var result = await service.LoginAsync("admin", "wrongPassword");

        Assert.False(result.Success);
        Assert.Equal("Password salah", result.Message);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTrue_WhenLoginSuccessful()
    {
        var context = await GetInMemoryDbContext();
        var config = GetTestConfiguration();
        var service = new AuthService(context, config);

        var result = await service.LoginAsync("admin", "password123");

        Assert.True(result.Success);
        Assert.Equal("Login berhasil", result.Message);
        Assert.Equal(UserRole.Admin, result.Role);
        Assert.Equal("Admin User", result.Nama);
    }

    [Fact]
    public void GenerateJwtToken_ReturnsValidToken()
    {
        var config = GetTestConfiguration();
        var service = new AuthService(null!, config); // context tidak dipakai di sini

        var user = new User
        {
            Id = 1,
            Username = "admin",
            Nama = "Admin User",
            NamaDivisi = null,
            Role = UserRole.Admin
        };

        var token = service.GenerateJwtToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));

        // Optional: decode token
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal("1", jwtToken.Claims.First(c => c.Type == "nameid" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        Assert.Equal("admin", jwtToken.Claims.First(c => c.Type == "username").Value);
    }
}


}
