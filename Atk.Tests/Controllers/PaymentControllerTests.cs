using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Controllers;
using Atk.DTOs.Payment;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Atk.Tests.Controllers
{
    public class PaymentControllerTests
    {
        private PaymentController GetController(Mock<IPayment> mockService)
        {
            return new PaymentController(mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResultWithData()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.GetAllAsync())
                       .ReturnsAsync(new List<Payment>
                       {
                           new Payment { Id = 1, SupplierId = 1, TotalHarga = 1000, TanggalBayar = DateTime.Today }
                       });

            var controller = GetController(mockService);
            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<Payment>>(okResult.Value);
            Assert.Single(data);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.GetByIdAsync(1))
                       .ReturnsAsync(new Payment { Id = 1, SupplierId = 1, TotalHarga = 1000, TanggalBayar = DateTime.Today });

            var controller = GetController(mockService);
            var result = await controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var payment = Assert.IsType<Payment>(okResult.Value);
            Assert.Equal(1, payment.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotFound()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Payment?)null);

            var controller = GetController(mockService);
            var result = await controller.GetById(999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Tidak Ditemukan", notFound.Value.ToString());
        }

        [Fact]
        public async Task Create_ReturnsOk_WithCreatedPayment()
        {
            var mockService = new Mock<IPayment>();
            var dto = new PaymentCreateDto { SupplierId = 1, TotalHarga = 1000, TanggalBayar = DateTime.Today, Keterangan = "Test" };
            mockService.Setup(s => s.CreateAsync(It.IsAny<Payment>()))
                       .ReturnsAsync((Payment p) => { p.Id = 1; return p; });

            var controller = GetController(mockService);
            var result = await controller.Create(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var payment = Assert.IsType<Payment>(okResult.Value);
            Assert.Equal(1, payment.Id);
            Assert.Equal(dto.TotalHarga, payment.TotalHarga);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenSuccessful()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.UpdateStatusAsync(1, PaymentStatus.Lunas)).ReturnsAsync(true);

            var controller = GetController(mockService);
            var dto = new PaymentUpdateStatusDto { Status = PaymentStatus.Lunas };
            var result = await controller.UpdateStatus(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Berhasil", okResult.Value.ToString());
        }

        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenFailed()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.UpdateStatusAsync(999, PaymentStatus.Lunas)).ReturnsAsync(false);

            var controller = GetController(mockService);
            var dto = new PaymentUpdateStatusDto { Status = PaymentStatus.Lunas };
            var result = await controller.UpdateStatus(999, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Tidak Ditemukan", notFound.Value.ToString());
        }

        [Fact]
        public async Task UploadBuktiTransfer_ReturnsOk_WhenSuccessful()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.UploadBuktiTransferAsync(1, "/path/file.pdf")).ReturnsAsync(true);

            var controller = GetController(mockService);
            var dto = new PaymentUploadBuktiDto { FilePath = "/path/file.pdf" };
            var result = await controller.UploadBuktiTransfer(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Berhasil", okResult.Value.ToString());
        }

        [Fact]
        public async Task UploadBuktiTransfer_ReturnsNotFound_WhenFailed()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.UploadBuktiTransferAsync(999, "/path/file.pdf")).ReturnsAsync(false);

            var controller = GetController(mockService);
            var dto = new PaymentUploadBuktiDto { FilePath = "/path/file.pdf" };
            var result = await controller.UploadBuktiTransfer(999, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Tidak Ditemukan", notFound.Value.ToString());
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenSuccessful()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var controller = GetController(mockService);
            var result = await controller.Delete(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Berhasil", okResult.Value.ToString());
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenFailed()
        {
            var mockService = new Mock<IPayment>();
            mockService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

            var controller = GetController(mockService);
            var result = await controller.Delete(999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Tidak Ditemukan", notFound.Value.ToString());
        }
    }
}
