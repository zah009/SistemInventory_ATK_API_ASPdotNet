using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.DTOs.BarangMasuk;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
public class BarangMasukControllerTests
{
private List<BarangMasukResponseDto> GetSampleDataDto()
{
return new List<BarangMasukResponseDto>
{
    new BarangMasukResponseDto { Id = 1, BarangId = 1, JumlahMasuk = 5, TanggalMasuk = DateTime.Now },
    new BarangMasukResponseDto { Id = 2, BarangId = 2, JumlahMasuk = 3, TanggalMasuk = DateTime.Now }
    };
}


    [Fact]
    public async Task GetAll_ReturnsOkResult_WithList()
    {
        var mockService = new Mock<IBarangMasuk>();
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(GetSampleDataDto());

        var controller = new BarangMasukController(mockService.Object);
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<BarangMasukResponseDto>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenDataExists()
    {
        var sample = GetSampleDataDto()[0];
        var mockService = new Mock<IBarangMasuk>();
        mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sample);

        var controller = new BarangMasukController(mockService.Object);
        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<BarangMasukResponseDto>(ok.Value);
        Assert.Equal(5, data.JumlahMasuk);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenDataDoesNotExist()
    {
        var mockService = new Mock<IBarangMasuk>();
        mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((BarangMasukResponseDto)null);

        var controller = new BarangMasukController(mockService.Object);
        var result = await controller.GetById(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("tidak ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task Create_ReturnsOk_WithCreatedData()
    {
        var dto = new BarangMasukCreateDto { BarangId = 1, JumlahMasuk = 3, TanggalMasuk = DateTime.Now };
        var created = new BarangMasukResponseDto { Id = 10, BarangId = 1, JumlahMasuk = 3, TanggalMasuk = DateTime.Now };

        var mockService = new Mock<IBarangMasuk>();
        mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var controller = new BarangMasukController(mockService.Object);
        var result = await controller.Create(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<BarangMasukResponseDto>(ok.Value);
        Assert.Equal(3, data.JumlahMasuk);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenDeleted()
    {
        var mockService = new Mock<IBarangMasuk>();
        mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var controller = new BarangMasukController(mockService.Object);
        var result = await controller.Delete(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("berhasil dihapus", ok.Value.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenDataMissing()
    {
        var mockService = new Mock<IBarangMasuk>();
        mockService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var controller = new BarangMasukController(mockService.Object);
        var result = await controller.Delete(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("tidak ditemukan", notFound.Value.ToString());
    }
}


}
