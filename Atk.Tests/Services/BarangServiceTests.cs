using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs;
using Atk.DTOs.Barang;
using Atk.Models;
using Atk.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Atk.Tests.Services
{
public class BarangServiceTests
{
private async Task<ApplicationDbContext> GetInMemoryDbContext()
{
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
.UseInMemoryDatabase(Guid.NewGuid().ToString()) // setiap test DB baru
.Options;


        var context = new ApplicationDbContext(options);

        // Seed data
        context.Barangs.Add(new Barang
        {
            KodeBarang = "B001",
            NamaBarang = "Pulpen",
            Stok = 10,
            Satuan = "pcs",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        await context.SaveChangesAsync();

        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBarang()
    {
        var context = await GetInMemoryDbContext();
        var service = new BarangService(context);

        var result = await service.GetAllAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsBarang_WhenExists()
    {
        var context = await GetInMemoryDbContext();
        var service = new BarangService(context);

        var barang = await service.GetByIdAsync(1);

        Assert.NotNull(barang);
        Assert.Equal("Pulpen", barang.NamaBarang);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var context = await GetInMemoryDbContext();
        var service = new BarangService(context);

        var barang = await service.GetByIdAsync(999);

        Assert.Null(barang);
    }

    [Fact]
    public async Task CreateAsync_AddsBarangSuccessfully()
    {
        var context = await GetInMemoryDbContext();
        var service = new BarangService(context);

        var dto = new BarangCreateDto
        {
            KodeBarang = "B002",
            NamaBarang = "Buku",
            Stok = 5,
            Satuan = "pcs"
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Buku", result.NamaBarang);
        Assert.Equal(2, await context.Barangs.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_RemovesBarangSuccessfully()
    {
        var context = await GetInMemoryDbContext();
        var service = new BarangService(context);

        var deleted = await service.DeleteAsync(1);

        Assert.True(deleted);
        Assert.Empty(context.Barangs);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var context = await GetInMemoryDbContext();
        var service = new BarangService(context);

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ExistsByName_ReturnsTrue_WhenExists()
    {
        var context = await GetInMemoryDbContext();
        var service = new BarangService(context);

        var exists = await service.ExistsByName("Pulpen");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByName_ReturnsFalse_WhenNotExists()
    {
        var context = await GetInMemoryDbContext();
        var service = new BarangService(context);

        var exists = await service.ExistsByName("Pensil");

        Assert.False(exists);
    }
}


}
