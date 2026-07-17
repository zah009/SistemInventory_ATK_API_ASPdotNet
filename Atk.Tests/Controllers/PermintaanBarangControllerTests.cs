using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.DTOs;
using Atk.DTOs.PermintaanBarang;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
public class PermintaanBarangControllerTests
{
// Helper untuk membuat HttpContext dengan user tertentu
private static ControllerContext CreateContext(int userId, string role)
{
var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
new Claim(ClaimTypes.Role, role)
}, "mock"));


        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    // =======================================================
    // 1. CREATE (ROLE DIVISI)
    // =======================================================
    [Fact]
    public async Task Create_ReturnsOk_WithCreatedPermintaan()
    {
        var mockService = new Mock<IPermintaanBarang>();

        var dto = new PermintaanBarangCreateDto
        {
            BarangId = 1,
            JumlahDiminta = 3,
            Alasan = "Perlu ATK"
        };

        var created = new PermintaanBarang
        {
            Id = 100,
            UserId = 1,
            BarangId = 1,
            JumlahDiminta = 3,
            Status = StatusPermintaan.Pending
        };

        mockService.Setup(s => s.CreateAsync(dto, 1)).ReturnsAsync(created);

        var controller = new PermintaanBarangController(mockService.Object)
        {
            ControllerContext = CreateContext(1, "Divisi")
        };

        var result = await controller.Create(dto) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var data = result.Value as PermintaanBarang;
        Assert.Equal(100, data.Id);
        Assert.Equal(1, data.UserId);
    }

    // =======================================================
    // 2. GET ALL — ROLE DIVISI HARUS FILTER USER SENDIRI
    // =======================================================
    [Fact]
    public async Task GetAll_Divisi_ReturnsOnlyOwnRequests()
    {
        var mockService = new Mock<IPermintaanBarang>();

        var data = new List<PermintaanBarang>
        {
            new PermintaanBarang { Id = 1, UserId = 1 },
            new PermintaanBarang { Id = 2, UserId = 2 }
        };

        mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(data);

        var controller = new PermintaanBarangController(mockService.Object)
        {
            ControllerContext = CreateContext(1, "Divisi")
        };

        var result = await controller.GetAll() as OkObjectResult;

        var list = result.Value as List<PermintaanBarang>;

        Assert.Single(list);
        Assert.Equal(1, list[0].UserId);
    }

    // =======================================================
    // 3. GET ALL — ROLE ADMIN MENDAPAT SEMUA DATA
    // =======================================================
    [Fact]
    public async Task GetAll_Admin_ReturnsAllRequests()
    {
        var mockService = new Mock<IPermintaanBarang>();

        var data = new List<PermintaanBarang>
        {
            new PermintaanBarang { Id = 1, UserId = 1 },
            new PermintaanBarang { Id = 2, UserId = 2 }
        };

        mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(data);

        var controller = new PermintaanBarangController(mockService.Object)
        {
            ControllerContext = CreateContext(99, "Admin")
        };

        var result = await controller.GetAll() as OkObjectResult;

        var list = result.Value as List<PermintaanBarang>;

        Assert.Equal(2, list.Count);
    }

    // =======================================================
    // 4. UPDATE STATUS — NOT FOUND
    // =======================================================
    [Fact]
    public async Task UpdateStatus_ReturnsNotFound_WhenNoRecord()
    {
        var mockService = new Mock<IPermintaanBarang>();

        mockService.Setup(s => s.UpdateStatusAsync(999, It.IsAny<PermintaanBarangUpdateStatusDto>()))
                   .ReturnsAsync(false);

        var controller = new PermintaanBarangController(mockService.Object)
        {
            ControllerContext = CreateContext(1, "Admin")
        };

        var dto = new PermintaanBarangUpdateStatusDto { Status = StatusPermintaan.Ditolak };

        var result = await controller.UpdateStatus(999, dto) as NotFoundObjectResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    // =======================================================
    // 5. UPDATE STATUS — SUCCESS
    // =======================================================
    [Fact]
    public async Task UpdateStatus_ReturnsOk_WhenApproved()
    {
        var mockService = new Mock<IPermintaanBarang>();

        mockService.Setup(s => s.UpdateStatusAsync(1, It.IsAny<PermintaanBarangUpdateStatusDto>()))
                   .ReturnsAsync(true);

        var controller = new PermintaanBarangController(mockService.Object)
        {
            ControllerContext = CreateContext(1, "Admin")
        };

        var dto = new PermintaanBarangUpdateStatusDto
        {
            Status = StatusPermintaan.Disetujui
        };

        var result = await controller.UpdateStatus(1, dto) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var msg = result.Value.GetType().GetProperty("message")
                              .GetValue(result.Value, null).ToString();

        Assert.Contains("disetujui", msg);
    }
}


}
