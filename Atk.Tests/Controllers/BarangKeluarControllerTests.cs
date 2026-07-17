using Atk.Controllers;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Atk.Tests.Controllers
{
    public class BarangKeluarControllerTests
    {
        private readonly Mock<IBarangKeluar> _serviceMock;
        private readonly BarangKeluarController _controller;

        public BarangKeluarControllerTests()
        {
            _serviceMock = new Mock<IBarangKeluar>();
            _controller = new BarangKeluarController(_serviceMock.Object);
        }

        // ============================================================
        // TEST 1: GetAll
        // ============================================================
        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            var fakeData = new List<BarangKeluar>
            {
                new BarangKeluar { Id = 1, BarangId = 1, PermintaanId = 10 },
                new BarangKeluar { Id = 2, BarangId = 2, PermintaanId = 20 }
            };

            _serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(fakeData);

            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var list = Assert.IsType<List<BarangKeluar>>(result.Value);
            Assert.Equal(2, list.Count);
        }

        // ============================================================
        // TEST 2: GetById SUCCESS
        // ============================================================
        [Fact]
        public async Task GetById_ShouldReturnOk_WhenItemExists()
        {
            var data = new BarangKeluar { Id = 1, BarangId = 1 };

            _serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(data);

            var result = await _controller.GetById(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.IsType<BarangKeluar>(result.Value);
        }

        // ============================================================
        // TEST 3: GetById NOT FOUND
        // ============================================================
        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            _serviceMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((BarangKeluar?)null);

            var result = await _controller.GetById(999) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);

            var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            string message = doc.RootElement.GetProperty("message").GetString();

            Assert.Equal("Barang id tidak ditemukan", message);

        }

        // ============================================================
        // TEST 4: GetByPermintaan
        // ============================================================
        [Fact]
        public async Task GetByPermintaan_ShouldReturnOk_WithList()
        {
            var fakeData = new List<BarangKeluar>
            {
                new BarangKeluar { Id = 1, PermintaanId = 10 },
                new BarangKeluar { Id = 2, PermintaanId = 10 }
            };

            _serviceMock.Setup(x => x.GetByPermintaanAsync(10)).ReturnsAsync(fakeData);

            var result = await _controller.GetByPermintaan(10) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var list = Assert.IsType<List<BarangKeluar>>(result.Value);
            Assert.Equal(2, list.Count);
        }

        // ============================================================
        // TEST 5: GetByBarang
        // ============================================================
        [Fact]
        public async Task GetByBarang_ShouldReturnOk_WithList()
        {
            var fakeData = new List<BarangKeluar>
            {
                new BarangKeluar { Id = 1, BarangId = 5 },
                new BarangKeluar { Id = 2, BarangId = 5 }
            };

            _serviceMock.Setup(x => x.GetByBarangAsync(5)).ReturnsAsync(fakeData);

            var result = await _controller.GetByBarang(5) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var list = Assert.IsType<List<BarangKeluar>>(result.Value);
            Assert.Equal(2, list.Count);
        }
    }
}