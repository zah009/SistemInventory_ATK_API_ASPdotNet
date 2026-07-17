using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.DTOs.Supplier;
using Atk.Services.Implementations;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
public class SupplierControllerTests
{
private List<SupplierResponseDto> GetSampleData()
{
return new List<SupplierResponseDto>
{
new SupplierResponseDto { Id = 1, NamaSupplier = "Supplier A", Alamat = "Alamat A", Telepon = "08123456789", Email = "[a@email.com](mailto:a@email.com)", CreatedAt = DateTime.Now },
new SupplierResponseDto { Id = 2, NamaSupplier = "Supplier B", Alamat = "Alamat B", Telepon = "08129876543", Email = "[b@email.com](mailto:b@email.com)", CreatedAt = DateTime.Now }
};
}


    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(GetSampleData());

        var controller = new SupplierController(mockService.Object);
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<SupplierResponseDto>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        var sample = GetSampleData()[0];
        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sample);

        var controller = new SupplierController(mockService.Object);
        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<SupplierResponseDto>(ok.Value);
        Assert.Equal("Supplier A", data.NamaSupplier);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNotExists()
    {
        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((SupplierResponseDto)null);

        var controller = new SupplierController(mockService.Object);
        var result = await controller.GetById(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Tidak ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task CreateBulk_ReturnsOk_WithCreatedSuppliers()
    {
        var dtos = new List<SupplierCreateDto>
        {
            new SupplierCreateDto { NamaSupplier = "Supplier C", Alamat = "Alamat C", Telepon = "08121111111", Email = "c@email.com" },
            new SupplierCreateDto { NamaSupplier = "Supplier D", Alamat = "Alamat D", Telepon = "08122222222", Email = "d@email.com" }
        };

        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.ExistsByName(It.IsAny<string>())).ReturnsAsync(false);
        mockService.Setup(s => s.CreateAsync(It.IsAny<SupplierCreateDto>()))
                   .ReturnsAsync((SupplierCreateDto dto) => new SupplierResponseDto
                   {
                       Id = new Random().Next(10, 100),
                       NamaSupplier = dto.NamaSupplier,
                       Alamat = dto.Alamat,
                       Telepon = dto.Telepon,
                       Email = dto.Email,
                       CreatedAt = DateTime.Now
                   });

        var controller = new SupplierController(mockService.Object);

        // reset _lastRequestTime agar rate limiting tidak aktif saat unit test
        var lastRequestTimeField = typeof(SupplierController).GetField("_lastRequestTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        lastRequestTimeField.SetValue(null, DateTime.MinValue);

        var result = await controller.CreateBulk(dtos);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(200, objectResult.StatusCode);

        var list = Assert.IsType<List<SupplierResponseDto>>(objectResult.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task CreateBulk_ReturnsBadRequest_WhenDuplicate()
    {
        var dtos = new List<SupplierCreateDto>
        {
            new SupplierCreateDto { NamaSupplier = "Supplier A", Alamat = "Alamat", Telepon = "0812", Email = "x@email.com" }
        };

        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.ExistsByName("Supplier A")).ReturnsAsync(true);

        var controller = new SupplierController(mockService.Object);

        // reset rate limiting
        var lastRequestTimeField = typeof(SupplierController).GetField("_lastRequestTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        lastRequestTimeField.SetValue(null, DateTime.MinValue);

        var result = await controller.CreateBulk(dtos);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("sudah ada", badRequest.Value.ToString());
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccessful()
    {
        var dto = new SupplierUpdateDto { NamaSupplier = "Supplier A Updated", Alamat = "Alamat Updated", Telepon = "0812999999", Email = "updated@email.com" };
        var updated = new SupplierResponseDto { Id = 1, NamaSupplier = "Supplier A Updated", Alamat = "Alamat Updated", Telepon = "0812999999", Email = "updated@email.com", CreatedAt = DateTime.Now };

        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(updated);

        var controller = new SupplierController(mockService.Object);
        var result = await controller.Update(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<SupplierResponseDto>(ok.Value);
        Assert.Equal("Supplier A Updated", data.NamaSupplier);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenSupplierMissing()
    {
        var dto = new SupplierUpdateDto { NamaSupplier = "Non Exist", Alamat = "Alamat", Telepon = "0812", Email = "x@email.com" };

        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.UpdateAsync(999, dto)).ReturnsAsync((SupplierResponseDto)null);

        var controller = new SupplierController(mockService.Object);
        var result = await controller.Update(999, dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Tidak Ditemukan", notFound.Value.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenSuccessful()
    {
        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var controller = new SupplierController(mockService.Object);
        var result = await controller.Delete(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)ok.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var mockService = new Mock<ISupplierServices>();
        mockService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var controller = new SupplierController(mockService.Object);
        var result = await controller.Delete(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Tidak Ditemukan", notFound.Value.ToString());
    }
}


}
