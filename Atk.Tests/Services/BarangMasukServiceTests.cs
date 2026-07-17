using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.BarangMasuk;
using Atk.Models;
using Atk.Services.Implementations;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Atk.Tests.Services
{
    public class BarangMasukServiceTests
    {
        private async Task<ApplicationDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var barang = new Barang
            {
                Id = 1,
                KodeBarang = "B001",
                NamaBarang = "Pulpen",
                Stok = 10,
                Satuan = "pcs",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Barangs.Add(barang);
            await context.SaveChangesAsync();

            context.BarangMasuks.Add(new BarangMasuk
            {
                Id = 1,
                BarangId = barang.Id,
                JumlahMasuk = 5,
                HargaSatuan = 2000,
                TanggalMasuk = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
            await context.SaveChangesAsync();

            return context;
        }

        private BarangMasukService GetService(ApplicationDbContext context)
        {
            var paymentMock = new Mock<IPayment>();

            // AddOrUpdatePaymentFromBarangMasukAsync -> Task
            paymentMock.Setup(p => p.AddOrUpdatePaymentFromBarangMasukAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>()))
                    .Returns(Task.CompletedTask);

            // ReducePaymentFromBarangMasukAsync -> Task<bool>
            paymentMock.Setup(p => p.ReducePaymentFromBarangMasukAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>()))
                    .ReturnsAsync(true); // bisa true atau false sesuai test case


            return new BarangMasukService(context, paymentMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllBarangMasuk()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var result = await service.GetAllAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsBarangMasuk_WhenExists()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var bm = await service.GetByIdAsync(1);

            Assert.NotNull(bm);
            Assert.Equal(5, bm.JumlahMasuk);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var bm = await service.GetByIdAsync(999);

            Assert.Null(bm);
        }

        [Fact]
        public async Task CreateAsync_AddsBarangMasukSuccessfully()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var dto = new BarangMasukCreateDto
            {
                BarangId = 1,
                JumlahMasuk = 3,
                HargaSatuan = 1000,
                TanggalMasuk = DateTime.Now
            };

            var result = await service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(3, result.JumlahMasuk);
            Assert.Equal(2, await context.BarangMasuks.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_RemovesBarangMasukSuccessfully()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var deleted = await service.DeleteAsync(1);

            Assert.True(deleted);
            Assert.Empty(context.BarangMasuks);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var deleted = await service.DeleteAsync(999);

            Assert.False(deleted);
        }
    }
}
