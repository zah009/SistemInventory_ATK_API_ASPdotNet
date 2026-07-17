using System;
using System.Threading.Tasks;
using System.Linq;
using Atk.Data;
using Atk.DTOs.Divisi;
using Atk.Models;
using Atk.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Atk.Tests.Services
{
    public class DivisiServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            return context;
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllDivisi()
        {
            var context = GetDbContext();

            context.Divisis.AddRange(
                new Divisi { Id = 1, Nama = "Keuangan" },
                new Divisi { Id = 2, Nama = "HRD" }
            );
            await context.SaveChangesAsync();

            var service = new DivisiService(context);

            var result = await service.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CreateAsync_AddsNewDivisi()
        {
            var context = GetDbContext();
            var service = new DivisiService(context);

            var dto = new DivisiCreateDto { Nama = "IT Support" };

            var result = await service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("IT Support", result.Nama);
            Assert.Equal(1, context.Divisis.Count());
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingDivisi()
        {
            var context = GetDbContext();

            context.Divisis.Add(new Divisi { Id = 1, Nama = "Old" });
            await context.SaveChangesAsync();

            var service = new DivisiService(context);

            var dto = new DivisiCreateDto { Nama = "Updated" };

            var result = await service.UpdateAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal("Updated", result.Nama);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenDivisiNotFound()
        {
            var context = GetDbContext();
            var service = new DivisiService(context);

            var dto = new DivisiCreateDto { Nama = "Anything" };

            var result = await service.UpdateAsync(99, dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenDivisiExists()
        {
            var context = GetDbContext();

            context.Divisis.Add(new Divisi { Id = 1, Nama = "Test" });
            await context.SaveChangesAsync();

            var service = new DivisiService(context);

            var result = await service.DeleteAsync(1);

            Assert.True(result);
            Assert.Equal(0, context.Divisis.Count());
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenDivisiNotFound()
        {
            var context = GetDbContext();
            var service = new DivisiService(context);

            var result = await service.DeleteAsync(123);

            Assert.False(result);
        }
    }
}
