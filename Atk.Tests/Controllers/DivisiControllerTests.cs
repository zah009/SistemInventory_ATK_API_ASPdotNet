using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.DTOs.Divisi;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
    public class DivisiControllerTests
    {
        private readonly Mock<IDivisi> _mockService;
        private readonly DivisiController _controller;

        public DivisiControllerTests()
        {
            _mockService = new Mock<IDivisi>();
            _controller = new DivisiController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithData()
        {
            // Arrange
            var divisis = new List<Divisi>
            {
                new Divisi { Id = 1, Nama = "IT" },
                new Divisi { Id = 2, Nama = "HRD" }
            };

            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(divisis);

            // Act
            var result = await _controller.GetAll() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var returnedData = Assert.IsType<List<Divisi>>(result.Value);
            Assert.Equal(2, returnedData.Count);
        }

        [Fact]
        public async Task Create_ReturnsOk_WithCreatedDivisi()
        {
            // Arrange
            var dto = new DivisiCreateDto { Nama = "Finance" };
            var created = new Divisi { Id = 1, Nama = "Finance" };

            _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _controller.Create(dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var returned = Assert.IsType<Divisi>(result.Value);
            Assert.Equal("Finance", returned.Nama);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var dto = new DivisiCreateDto { Nama = "Updated" };
            var updated = new Divisi { Id = 1, Nama = "Updated" };

            _mockService.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(updated);

            // Act
            var result = await _controller.Update(1, dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var returned = Assert.IsType<Divisi>(result.Value);
            Assert.Equal("Updated", returned.Nama);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenDivisiDoesNotExist()
        {
            // Arrange
            var dto = new DivisiCreateDto { Nama = "Unknown" };
            _mockService.Setup(s => s.UpdateAsync(99, dto)).ReturnsAsync((Divisi)null);

            // Act
            var result = await _controller.Update(99, dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Deleted", result.Value);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenDivisiDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
