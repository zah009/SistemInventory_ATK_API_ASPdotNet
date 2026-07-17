using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.Pengadaan;
using Atk.Models;
using Microsoft.EntityFrameworkCore;
using SistemInventoriAtk.Models;
using Xunit;

namespace Atk.Tests.Services
{
public class PengadaanServiceTests
{
private async Task<ApplicationDbContext> GetInMemoryDbContext()
{
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
.UseInMemoryDatabase(Guid.NewGuid().ToString())
.Options;


        var context = new ApplicationDbContext(options);

        // Seed data
        context.PengadaanBarangs.AddRange(
            new PengadaanBarang
            {
                Id = 1,
                NamaBarang = "Pulpen Biru",
                Satuan = "pcs",
                JumlahDiajukan = 12,
                CreatedAt = DateTime.Now
            },
            new PengadaanBarang
            {
                Id = 2,
                NamaBarang = "Buku Tulis",
                Satuan = "pcs",
                JumlahDiajukan = 5,
                CreatedAt = DateTime.Now
            }
        );

        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task GetAll_ReturnsAllPengadaanBarang()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.NamaBarang == "Pulpen Biru");
        Assert.Contains(result, x => x.NamaBarang == "Buku Tulis");
    }

    [Fact]
    public async Task GetById_ReturnsCorrectItem()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var item = await service.GetByIdAsync(1);

        Assert.NotNull(item);
        Assert.Equal("Pulpen Biru", item.NamaBarang);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenNotFound()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var item = await service.GetByIdAsync(999);

        Assert.Null(item);
    }

    [Fact]
    public async Task Create_AddsNewPengadaanBarang()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var newItem = new PengadaanCreateDto
        {
            NamaBarang = "Pensil",
            Satuan = "pcs",
            JumlahDiajukan = 10
        };

        var created = await service.CreateAsync(newItem);

        Assert.NotNull(created);
        Assert.Equal("Pensil", created.NamaBarang);

        var allItems = await service.GetAllAsync();
        Assert.Equal(3, allItems.Count());
    }

    [Fact]
    public async Task Update_ChangesExistingItem()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var item = await service.GetByIdAsync(1);
        var updateDto = new PengadaanUpdateDto
        {
            NamaBarang = item.NamaBarang,
            Satuan = item.Satuan,
            JumlahDiajukan = 20
        };

        await service.UpdateAsync(item.Id, updateDto);

        var updatedItem = await service.GetByIdAsync(1);
        Assert.Equal(20, updatedItem.JumlahDiajukan);
    }

    [Fact]
    public async Task Delete_RemovesItem()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var success = await service.DeleteAsync(1);

        Assert.True(success);

        var item = await service.GetByIdAsync(1);
        Assert.Null(item);
    }

    [Fact]
    public async Task Delete_ReturnsFalse_WhenItemNotExist()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var success = await service.DeleteAsync(999);

        Assert.False(success);
    }
}


}
