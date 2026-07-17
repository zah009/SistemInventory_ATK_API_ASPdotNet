using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.DTOs.Pengadaan;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
public class PengadaanControllerTests
{
private List<PengadaanResponseDto> GetSampleData()
{
return new List<PengadaanResponseDto>
{
new PengadaanResponseDto { Id = 1, NamaBarang = "Pulpen", Satuan = "pcs", JumlahDiajukan = 10, CreatedAt = DateTime.Now },
new PengadaanResponseDto { Id = 2, NamaBarang = "Buku Tulis", Satuan = "pcs", JumlahDiajukan = 5, CreatedAt = DateTime.Now }
};
}


    private void ResetRateLimit(PengadaanController controller)
    {
        typeof(PengadaanController)
            .GetField("_lastRequestTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            .SetValue(null, DateTime.MinValue);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(GetSampleData());

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<PengadaanResponseDto>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        var sample = GetSampleData()[0];
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sample);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<PengadaanResponseDto>(ok.Value);
        Assert.Equal("Pulpen", data.NamaBarang);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNotExists()
    {
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((PengadaanResponseDto)null);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.GetById(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("id tidak ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task CreateBulk_ReturnsOk_WithCreatedItems()
    {
        var dtos = new List<PengadaanCreateDto>
        {
            new PengadaanCreateDto { NamaBarang = "Pensil", Satuan = "pcs", JumlahDiajukan = 12 },
            new PengadaanCreateDto { NamaBarang = "Penghapus", Satuan = "pcs", JumlahDiajukan = 8 }
        };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.ExistsByName(It.IsAny<string>())).ReturnsAsync(false);
        mockService.Setup(s => s.CreateAsync(It.IsAny<PengadaanCreateDto>()))
            .ReturnsAsync((PengadaanCreateDto dto) => new PengadaanResponseDto
            {
                Id = new Random().Next(10, 100),
                NamaBarang = dto.NamaBarang,
                Satuan = dto.Satuan,
                JumlahDiajukan = dto.JumlahDiajukan,
                CreatedAt = DateTime.Now
            });

        var controller = new PengadaanController(mockService.Object);
        ResetRateLimit(controller);

        var result = await controller.CreateBulk(dtos);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<PengadaanResponseDto>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task CreateBulk_ReturnsBadRequest_WhenDuplicate()
    {
        var dtos = new List<PengadaanCreateDto>
        {
            new PengadaanCreateDto { NamaBarang = "Pulpen", Satuan = "pcs", JumlahDiajukan = 10 }
        };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.ExistsByName("Pulpen")).ReturnsAsync(true);

        var controller = new PengadaanController(mockService.Object);
        ResetRateLimit(controller);

        var result = await controller.CreateBulk(dtos);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("sudah ada", badRequest.Value.ToString());
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccessful()
    {
        var dto = new PengadaanUpdateDto { NamaBarang = "Pulpen Updated", Satuan = "pcs", JumlahDiajukan = 15 };
        var updated = new PengadaanResponseDto { Id = 1, NamaBarang = "Pulpen Updated", Satuan = "pcs", JumlahDiajukan = 15, CreatedAt = DateTime.Now };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(updated);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Update(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<PengadaanResponseDto>(ok.Value);
        Assert.Equal("Pulpen Updated", data.NamaBarang);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        var dto = new PengadaanUpdateDto { NamaBarang = "NonExist", Satuan = "pcs", JumlahDiajukan = 5 };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.UpdateAsync(999, dto)).ReturnsAsync((PengadaanResponseDto)null);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Update(999, dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Supplier Tidak Ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenSuccessful()
    {
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Delete(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)ok.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Delete(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Data Tidak Terhapus", notFound.Value.ToString());
    }
}


}
