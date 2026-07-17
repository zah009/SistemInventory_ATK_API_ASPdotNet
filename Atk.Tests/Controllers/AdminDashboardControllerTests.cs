using Atk.Controllers;
using Atk.DTOs.AdminDashboard;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Atk.Tests.Controllers
{
    public class AdminDashboardControllerTests
    {
        private readonly Mock<IAdminDashboard> _serviceMock;
        private readonly AdminDashboardController _controller;

        public AdminDashboardControllerTests()
        {
            _serviceMock = new Mock<IAdminDashboard>();
            _controller = new AdminDashboardController(_serviceMock.Object);
        }

        // ============================================================
        // TEST 1: Return 200 OK + object
        // ============================================================
        [Fact]
        public async Task GetDashboard_ShouldReturnOk_WithDashboardDto()
        {
            // Arrange
            var fakeDto = new AdminDashboardDto
            {
                Summary = new DashboardSummaryDto
                {
                    TotalBarang = 10,
                    TotalPermintaanPending = 2,
                    TotalPermintaanDisetujui = 5,
                    TotalPermintaanDitolak = 3,
                    TotalBarangHampirHabis = 1
                }
            };

            _serviceMock.Setup(x => x.GetDashboardAsync())
                .ReturnsAsync(fakeDto);

            // Act
            var result = await _controller.GetDashboard() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var dto = Assert.IsType<AdminDashboardDto>(result.Value);
            Assert.Equal(10, dto.Summary.TotalBarang);
            Assert.Equal(2, dto.Summary.TotalPermintaanPending);
        }

        // ============================================================
        // TEST 2: If service returns null → should still return OK(null)
        // ============================================================
        [Fact]
        public async Task GetDashboard_ShouldReturnOk_WhenServiceReturnsNull()
        {
            // Arrange
            _serviceMock.Setup(x => x.GetDashboardAsync())
                .ReturnsAsync((AdminDashboardDto?)null);

            // Act
            var result = await _controller.GetDashboard() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Null(result.Value);
        }
    }
}
