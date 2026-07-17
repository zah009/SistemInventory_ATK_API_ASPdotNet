using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.DTOs;
using Atk.DTOs.Barang;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
public class BarangControllerTests
{
private readonly Mock<IBarang> _mockService;
private readonly BarangController _controller;


    public BarangControllerTests()
    {
        _mockService = new Mock<IBarang>();
        _controller = new BarangController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfBarang()
    {
        // Arrange
        var data = new List<BarangResponseDto>
        {
            new BarangResponseDto { Id = 1, NamaBarang = "Pulpen", KodeBarang = "B001", Stok = 10, Satuan = "pcs" }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(data);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<BarangResponseDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenBarangExists()
    {
        var barang = new BarangResponseDto { Id = 1, NamaBarang = "Pulpen", KodeBarang = "B001", Stok = 10, Satuan = "pcs" };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(barang);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<BarangResponseDto>(okResult.Value);
        Assert.Equal("Pulpen", returnValue.NamaBarang);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenBarangDoesNotExist()
    {
        _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((BarangResponseDto?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateBulk_ReturnsBadRequest_WhenDtoIsEmpty()
    {
        var result = await _controller.CreateBulk(new List<BarangCreateDto>());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenDeleted()
    {
        _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenNotDeleted()
    {
        _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _controller.Delete(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}


}
