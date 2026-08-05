using System;
using System.Collections.Generic;
using System.Linq;
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
new PengadaanResponseDto { Id = 1, BarangId = 1, NamaBarang = "Pulpen", Satuan = "pcs", JumlahDiajukan = 10, SupplierId = 1, CreatedAt = DateTime.Now },
new PengadaanResponseDto { Id = 2, BarangId = 2, NamaBarang = "Buku Tulis", Satuan = "pcs", JumlahDiajukan = 5, SupplierId = 1, CreatedAt = DateTime.Now }
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
        var data = ok.Value.GetType().GetProperty("data")?.GetValue(ok.Value);
        var list = Assert.IsAssignableFrom<IEnumerable<PengadaanResponseDto>>(data);
        Assert.Equal(2, list.Count());
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
        var data = ok.Value.GetType().GetProperty("data")?.GetValue(ok.Value);
        var item = Assert.IsType<PengadaanResponseDto>(data);
        Assert.Equal("Pulpen", item.NamaBarang);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNotExists()
    {
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((PengadaanResponseDto)null);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.GetById(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("tidak ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task CreateBulk_ReturnsOk_WithCreatedItems()
    {
        var dtos = new List<PengadaanCreateDto>
        {
            new PengadaanCreateDto { BarangId = 3, Satuan = "pcs", JumlahDiajukan = 12, SupplierId = 1, TanggalPengajuan = DateTime.Today },
            new PengadaanCreateDto { BarangId = 4, Satuan = "pcs", JumlahDiajukan = 8, SupplierId = 1, TanggalPengajuan = DateTime.Today }
        };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.HasOpenPengadaanAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
        mockService.Setup(s => s.CreateAsync(It.IsAny<PengadaanCreateDto>()))
            .ReturnsAsync((PengadaanCreateDto dto) => new PengadaanResponseDto
            {
                Id = new Random().Next(10, 100),
                BarangId = dto.BarangId,
                Satuan = dto.Satuan,
                JumlahDiajukan = dto.JumlahDiajukan,
                SupplierId = dto.SupplierId,
                CreatedAt = DateTime.Now
            });

        var controller = new PengadaanController(mockService.Object);
        ResetRateLimit(controller);

        var result = await controller.CreateBulk(dtos);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = ok.Value.GetType().GetProperty("data")?.GetValue(ok.Value);
        var list = Assert.IsAssignableFrom<List<PengadaanResponseDto>>(data);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task CreateBulk_ReturnsBadRequest_WhenAdaPengadaanAktifUntukBarangSupplierYangSama()
    {
        var dtos = new List<PengadaanCreateDto>
        {
            new PengadaanCreateDto { BarangId = 1, Satuan = "pcs", JumlahDiajukan = 10, SupplierId = 1, TanggalPengajuan = DateTime.Today }
        };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.HasOpenPengadaanAsync(1, 1)).ReturnsAsync(true);

        var controller = new PengadaanController(mockService.Object);
        ResetRateLimit(controller);

        var result = await controller.CreateBulk(dtos);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("pengadaan aktif", badRequest.Value.ToString());
    }

    [Fact]
    public async Task CreateBulk_ReturnsNotFound_WhenBarangTidakDitemukan()
    {
        var dtos = new List<PengadaanCreateDto>
        {
            new PengadaanCreateDto { BarangId = 999, Satuan = "pcs", JumlahDiajukan = 10, SupplierId = 1, TanggalPengajuan = DateTime.Today }
        };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.HasOpenPengadaanAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
        mockService.Setup(s => s.CreateAsync(It.IsAny<PengadaanCreateDto>()))
            .ThrowsAsync(new KeyNotFoundException("Barang dengan id 999 tidak ditemukan."));

        var controller = new PengadaanController(mockService.Object);
        ResetRateLimit(controller);

        var result = await controller.CreateBulk(dtos);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("tidak ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccessful()
    {
        var dto = new PengadaanUpdateDto { BarangId = 1, Satuan = "pcs", JumlahDiajukan = 15, SupplierId = 1, TanggalPengajuan = DateTime.Today };
        var updated = new PengadaanResponseDto { Id = 1, BarangId = 1, NamaBarang = "Pulpen Updated", Satuan = "pcs", JumlahDiajukan = 15, SupplierId = 1, CreatedAt = DateTime.Now };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(updated);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Update(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var responseData = ok.Value.GetType().GetProperty("data")?.GetValue(ok.Value);
        var data = Assert.IsType<PengadaanResponseDto>(responseData);
        Assert.Equal("Pulpen Updated", data.NamaBarang);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        var dto = new PengadaanUpdateDto { BarangId = 1, Satuan = "pcs", JumlahDiajukan = 5, SupplierId = 1, TanggalPengajuan = DateTime.Today };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.UpdateAsync(999, dto))
            .ThrowsAsync(new KeyNotFoundException("PengadaanBarang tidak ditemukan"));

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Update(999, dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("tidak ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenSudahDipenuhiSebagian()
    {
        var dto = new PengadaanUpdateDto { BarangId = 2, Satuan = "pcs", JumlahDiajukan = 5, SupplierId = 1, TanggalPengajuan = DateTime.Today };

        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.UpdateAsync(1, dto))
            .ThrowsAsync(new InvalidOperationException("Pengadaan sudah sebagian/seluruhnya dipenuhi lewat Barang Masuk."));

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Update(1, dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("sudah sebagian", badRequest.Value.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenSuccessful()
    {
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Delete(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Berhasil", ok.Value.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Delete(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("tidak ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenPunyaRiwayatBarangMasuk()
    {
        var mockService = new Mock<IPengadaan>();
        mockService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Pengadaan ini sudah punya riwayat Barang Masuk dan tidak bisa dihapus."));

        var controller = new PengadaanController(mockService.Object);
        var result = await controller.Delete(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("riwayat Barang Masuk", badRequest.Value.ToString());
    }
}


}