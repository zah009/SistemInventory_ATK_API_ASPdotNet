using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.Data;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
public class AuthControllerTests
{
private AuthController GetController(IAuthService authService, ApplicationDbContext context)
{
return new AuthController(authService, context);
}


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

        context.Users.Add(new User
        {
            Id = 2,
            Username = "divisi",
            Password = BCrypt.Net.BCrypt.HashPassword("password456"),
            Nama = "Divisi User",
            NamaDivisi = "IT",
            Role = UserRole.Divisi
        });

        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenLoginFails()
    {
        var context = await GetInMemoryDbContext();
        var authMock = new Mock<IAuthService>();

        authMock.Setup(a => a.LoginAsync("admin", "wrongPassword"))
                .ReturnsAsync((false, "Password salah", null, null, null, null));

        var controller = GetController(authMock.Object, context);

        var dto = new LoginDto { Username = "admin", Password = "wrongPassword" };

        var result = await controller.Login(dto);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var value = unauthorizedResult.Value!;
        var messageProp = value.GetType().GetProperty("message")!.GetValue(value);

        Assert.Equal("Password salah", messageProp);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenLoginSuccessful()
    {
        var context = await GetInMemoryDbContext();
        var authMock = new Mock<IAuthService>();

        authMock.Setup(a => a.LoginAsync("admin", "password123"))
                .ReturnsAsync((true, "Login berhasil", UserRole.Admin, "Admin User", null, 1));

        authMock.Setup(a => a.GenerateJwtToken(It.IsAny<User>()))
                .Returns("TestJwtToken123");

        var controller = GetController(authMock.Object, context);
        var dto = new LoginDto { Username = "admin", Password = "password123" };

        var result = await controller.Login(dto);
        var okResult = Assert.IsType<OkObjectResult>(result);

        var value = okResult.Value!;
        var messageProp = value.GetType().GetProperty("message")!.GetValue(value);
        var dataProp = value.GetType().GetProperty("data")!.GetValue(value)!;
        var tokenProp = dataProp.GetType().GetProperty("token")!.GetValue(dataProp);

        Assert.Equal("Login berhasil", messageProp);
        Assert.Equal("TestJwtToken123", tokenProp);
    }

    [Fact]
    public void Logout_ReturnsOk()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        var authMock = new Mock<IAuthService>();

        var controller = GetController(authMock.Object, context);

        var result = controller.Logout();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var messageProp = okResult.Value!.GetType().GetProperty("message")!.GetValue(okResult.Value);

        Assert.Equal("Logout berhasil, silakan hapus token di client.", messageProp);
    }
}


}
