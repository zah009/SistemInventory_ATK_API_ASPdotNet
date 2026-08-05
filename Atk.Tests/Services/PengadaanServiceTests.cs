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

        // Seed Barang master (PengadaanBarang sekarang WAJIB referensi BarangId)
        context.Barangs.AddRange(
            new Barang { Id = 1, KodeBarang = "B001", NamaBarang = "Pulpen Biru", Satuan = "pcs", Stok = 0, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Barang { Id = 2, KodeBarang = "B002", NamaBarang = "Buku Tulis", Satuan = "pcs", Stok = 0, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Barang { Id = 3, KodeBarang = "B003", NamaBarang = "Pensil", Satuan = "pcs", Stok = 0, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        );

        // Seed Supplier
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

        await context.SaveChangesAsync();

        // Seed data PengadaanBarang
        context.PengadaanBarangs.AddRange(
            new PengadaanBarang
            {
                Id = 1,
                BarangId = 1,
                NamaBarang = "Pulpen Biru",
                Satuan = "pcs",
                JumlahDiajukan = 12,
                SupplierId = 1,
                TanggalPengajuan = DateTime.Today,
                CreatedAt = DateTime.Now
            },
            new PengadaanBarang
            {
                Id = 2,
                BarangId = 2,
                NamaBarang = "Buku Tulis",
                Satuan = "pcs",
                JumlahDiajukan = 5,
                SupplierId = 1,
                TanggalPengajuan = DateTime.Today,
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
        Assert.Equal(1, item.BarangId);
        Assert.Equal(StatusPengadaan.Diajukan, item.Status);
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
    public async Task Create_AddsNewPengadaanBarang_DenganBarangIdValid()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var newItem = new PengadaanCreateDto
        {
            BarangId = 3, // "Pensil", sudah di-seed di master Barang
            Satuan = "pcs",
            JumlahDiajukan = 10,
            SupplierId = 1,
            TanggalPengajuan = DateTime.Today
        };

        var created = await service.CreateAsync(newItem);

        Assert.NotNull(created);
        // NamaBarang otomatis di-snapshot dari master Barang, bukan input bebas
        Assert.Equal("Pensil", created.NamaBarang);
        Assert.Equal(StatusPengadaan.Diajukan, created.Status);
        Assert.Equal(0, created.JumlahDiterima);

        var allItems = await service.GetAllAsync();
        Assert.Equal(3, allItems.Count());
    }

    [Fact]
    public async Task Create_ThrowsKeyNotFoundException_WhenBarangIdTidakAda()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var newItem = new PengadaanCreateDto
        {
            BarangId = 999, // tidak ada di master Barang
            Satuan = "pcs",
            JumlahDiajukan = 10,
            SupplierId = 1,
            TanggalPengajuan = DateTime.Today
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(newItem));
    }

    [Fact]
    public async Task Create_ThrowsKeyNotFoundException_WhenSupplierIdTidakAda()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var newItem = new PengadaanCreateDto
        {
            BarangId = 3,
            Satuan = "pcs",
            JumlahDiajukan = 10,
            SupplierId = 999, // tidak ada di master Supplier
            TanggalPengajuan = DateTime.Today
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(newItem));
    }

    [Fact]
    public async Task Update_ChangesExistingItem()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var item = await service.GetByIdAsync(1);
        var updateDto = new PengadaanUpdateDto
        {
            BarangId = item.BarangId,
            Satuan = item.Satuan,
            JumlahDiajukan = 20,
            SupplierId = item.SupplierId,
            TanggalPengajuan = item.TanggalPengajuan
        };

        await service.UpdateAsync(item.Id, updateDto);

        var updatedItem = await service.GetByIdAsync(1);
        Assert.Equal(20, updatedItem.JumlahDiajukan);
    }

    [Fact]
    public async Task Update_ThrowsKeyNotFoundException_WhenItemTidakAda()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var updateDto = new PengadaanUpdateDto
        {
            BarangId = 1,
            Satuan = "pcs",
            JumlahDiajukan = 20,
            SupplierId = 1,
            TanggalPengajuan = DateTime.Today
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(999, updateDto));
    }

    [Fact]
    public async Task Update_ThrowsInvalidOperationException_KetikaSudahSebagianDipenuhiDanBarangIdDiganti()
    {
        var context = await GetInMemoryDbContext();

        // Tandai pengadaan #1 sudah sebagian dipenuhi
        var pengadaan = await context.PengadaanBarangs.FindAsync(1);
        pengadaan.JumlahDiterima = 5;
        await context.SaveChangesAsync();

        var service = new Atk.Services.Implementations.PengadaanService(context);

        var updateDto = new PengadaanUpdateDto
        {
            BarangId = 2, // coba ganti barang padahal sudah sebagian diterima
            Satuan = "pcs",
            JumlahDiajukan = 20,
            SupplierId = 1,
            TanggalPengajuan = DateTime.Today
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(1, updateDto));
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

    [Fact]
    public async Task Delete_ThrowsInvalidOperationException_WhenPunyaRiwayatBarangMasuk()
    {
        var context = await GetInMemoryDbContext();

        context.BarangMasuks.Add(new BarangMasuk
        {
            BarangId = 1,
            PengadaanId = 1,
            JumlahMasuk = 5,
            HargaSatuan = 1000,
            TanggalMasuk = DateTime.Today,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        await context.SaveChangesAsync();

        var service = new Atk.Services.Implementations.PengadaanService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(1));
    }

    [Fact]
    public async Task HasOpenPengadaanAsync_ReturnsTrue_WhenAdaPengadaanStatusDiajukan()
    {
        var context = await GetInMemoryDbContext();
        var service = new Atk.Services.Implementations.PengadaanService(context);

        var hasOpen = await service.HasOpenPengadaanAsync(barangId: 1, supplierId: 1);

        Assert.True(hasOpen);
    }

    [Fact]
    public async Task HasOpenPengadaanAsync_ReturnsFalse_WhenStatusSudahSelesai()
    {
        var context = await GetInMemoryDbContext();

        var pengadaan = await context.PengadaanBarangs.FindAsync(1);
        pengadaan.Status = StatusPengadaan.Selesai;
        await context.SaveChangesAsync();

        var service = new Atk.Services.Implementations.PengadaanService(context);

        var hasOpen = await service.HasOpenPengadaanAsync(barangId: 1, supplierId: 1);

        Assert.False(hasOpen);
    }
}


}