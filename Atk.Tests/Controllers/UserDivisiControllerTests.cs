using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.DTOs.Users;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
public class UserDivisiControllerTests
{
private UserDivisiController GetController(Mock<IUserService> serviceMock)
{
return new UserDivisiController(serviceMock.Object);
}


    [Fact]
    public async Task GetAll_ReturnsOkWithUsers()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.GetAllAsync())
                   .ReturnsAsync(new List<User>
                   {
                       new User { Id = 1, Username = "user1", Nama = "User Satu", NamaDivisi = "IT" }
                   });

        var controller = GetController(serviceMock);

        var result = await controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);

        Assert.Single(users);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenUserExists()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.GetByIdAsync(1))
                   .ReturnsAsync(new User { Id = 1, Username = "user1" });

        var controller = GetController(serviceMock);

        var result = await controller.GetById(1);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<User>(okResult.Value);

        Assert.Equal(1, user.Id);
        Assert.Equal("user1", user.Username);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenUserNotFound()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.GetByIdAsync(999))
                   .ReturnsAsync((User)null);

        var controller = GetController(serviceMock);

        var result = await controller.GetById(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkWithCreatedUser()
    {
        var serviceMock = new Mock<IUserService>();
        var dto = new UserCreateDivisiDto { Username = "user2", Password = "secret", Nama = "User Dua", NamaDivisi = "HR" };

        serviceMock.Setup(s => s.CreateDivisiUserAsync(dto))
                   .ReturnsAsync(new User { Id = 2, Username = "user2" });

        var controller = GetController(serviceMock);

        var result = await controller.Create(dto);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<User>(okResult.Value);

        Assert.Equal(2, user.Id);
        Assert.Equal("user2", user.Username);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUpdateSuccessful()
    {
        var serviceMock = new Mock<IUserService>();
        var dto = new UserUpdateDivisiDto { Username = "user1-upd", Nama = "User 1 Updated", NamaDivisi = "Finance" };
        serviceMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(true);

        var controller = GetController(serviceMock);

        var result = await controller.Update(1, dto);
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Ambil properti "message" dari anonymous object
        var value = okResult.Value!;
        var message = value.GetType().GetProperty("message")!.GetValue(value)?.ToString();

        Assert.Equal("User Berhasil Di Update", message);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenUpdateFails()
    {
        var serviceMock = new Mock<IUserService>();
        var dto = new UserUpdateDivisiDto { Username = "notfound", Nama = "None", NamaDivisi = "None" };
        serviceMock.Setup(s => s.UpdateAsync(999, dto)).ReturnsAsync(false);

        var controller = GetController(serviceMock);

        var result = await controller.Update(999, dto);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenDeleteSuccessful()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var controller = GetController(serviceMock);

        var result = await controller.Delete(1);
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Ambil properti "message" dari anonymous object
        var value = okResult.Value!;
        var message = value.GetType().GetProperty("message")!.GetValue(value)?.ToString();

        Assert.Equal("User Berhasil di Hapus", message);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenDeleteFails()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var controller = GetController(serviceMock);

        var result = await controller.Delete(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}


}
