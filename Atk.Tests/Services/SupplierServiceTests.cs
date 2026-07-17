using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.Supplier;
using Atk.Models;
using Atk.Services.Interfaces;
using Atk.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Atk.Tests.Services
{
public class SupplierServiceTests
{
private async Task<ApplicationDbContext> GetInMemoryDbContext()
{
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
.UseInMemoryDatabase(Guid.NewGuid().ToString()) // Setiap test pakai DB baru
.Options;

        var context = new ApplicationDbContext(options);

        // Seed data
        context.Suppliers.Add(new Supplier
        {
            Id = 1,
            namaSupplier = "Supplier A",
            Alamat = "Alamat A",
            Telepon = "08123456789",
            Email = "a@email.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        context.Suppliers.Add(new Supplier
        {
            Id = 2,
            namaSupplier = "Supplier B",
            Alamat = "Alamat B",
            Telepon = "08129876543",
            Email = "b@email.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllSuppliers()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var result = (await service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.NamaSupplier == "Supplier A");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSupplier_WhenExists()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var supplier = await service.GetByIdAsync(1);

        Assert.NotNull(supplier);
        Assert.Equal("Supplier A", supplier.NamaSupplier);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var supplier = await service.GetByIdAsync(999);

        Assert.Null(supplier);
    }

    [Fact]
    public async Task CreateAsync_AddsSupplierSuccessfully()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var dto = new SupplierCreateDto
        {
            NamaSupplier = "Supplier C",
            Alamat = "Alamat C",
            Telepon = "08121234567",
            Email = "c@email.com"
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Supplier C", result.NamaSupplier);
        Assert.Equal(3, await context.Suppliers.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesSupplierSuccessfully()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var dto = new SupplierUpdateDto
        {
            NamaSupplier = "Supplier A Updated",
            Alamat = "Alamat A Updated",
            Telepon = "08120000000",
            Email = "updated@email.com"
        };

        var result = await service.UpdateAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal("Supplier A Updated", result.NamaSupplier);
        Assert.Equal("Alamat A Updated", result.Alamat);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenSupplierNotFound()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var dto = new SupplierUpdateDto
        {
            NamaSupplier = "Non Exist",
            Alamat = "Alamat",
            Telepon = "0812",
            Email = "x@email.com"
        };

        var result = await service.UpdateAsync(999, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesSupplierSuccessfully()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var deleted = await service.DeleteAsync(1);

        Assert.True(deleted);
        Assert.Equal(1, await context.Suppliers.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenSupplierNotFound()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ExistsByName_ReturnsTrue_WhenExists()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var exists = await service.ExistsByName("Supplier A");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByName_ReturnsFalse_WhenNotExists()
    {
        var context = await GetInMemoryDbContext();
        var service = new SupplierService(context);

        var exists = await service.ExistsByName("Non Exist");

        Assert.False(exists);
    }
}

}
