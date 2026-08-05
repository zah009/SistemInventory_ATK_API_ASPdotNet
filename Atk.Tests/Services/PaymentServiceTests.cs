using System;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.Models;
using Atk.Services.Implementations;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Atk.Tests.Services
{
public class PaymentServiceTests
{
private async Task<ApplicationDbContext> GetInMemoryDbContext()
{
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
.UseInMemoryDatabase(Guid.NewGuid().ToString())
.Options;

        var context = new ApplicationDbContext(options);

        // Seed supplier
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

        return context;
    }

    private PaymentService GetService(ApplicationDbContext context)
    {
        return new PaymentService(context);
    }

    [Fact]
    public async Task CreateAsync_AddsPaymentSuccessfully()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        var payment = new Payment
        {
            SupplierId = 1,
            TotalHarga = 1000,
            TanggalBayar = DateTime.Today,
            Status = PaymentStatus.Pending
        };

        var result = await service.CreateAsync(payment);

        Assert.NotNull(result);
        Assert.Equal(1000, result.TotalHarga);
        Assert.Single(context.Payments);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPaymentSuccessfully()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        var payment = await service.CreateAsync(new Payment
        {
            SupplierId = 1,
            TotalHarga = 500,
            TanggalBayar = DateTime.Today,
            Status = PaymentStatus.Pending
        });

        var deleted = await service.DeleteAsync(payment.Id);

        Assert.True(deleted);
        Assert.Empty(context.Payments);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPayments()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        await service.CreateAsync(new Payment
        {
            SupplierId = 1,
            TotalHarga = 1000,
            TanggalBayar = DateTime.Today,
            Status = PaymentStatus.Pending
        });

        var list = await service.GetAllAsync();

        Assert.Single(list);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPayment_WhenExists()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        var payment = await service.CreateAsync(new Payment
        {
            SupplierId = 1,
            TotalHarga = 1500,
            TanggalBayar = DateTime.Today,
            Status = PaymentStatus.Pending
        });

        var result = await service.GetByIdAsync(payment.Id);

        Assert.NotNull(result);
        Assert.Equal(1500, result.TotalHarga);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatusSuccessfully()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        var payment = await service.CreateAsync(new Payment
        {
            SupplierId = 1,
            TotalHarga = 2000,
            TanggalBayar = DateTime.Today,
            Status = PaymentStatus.Pending
        });

        var updated = await service.UpdateStatusAsync(payment.Id, PaymentStatus.Lunas);

        Assert.True(updated);
        var check = await context.Payments.FindAsync(payment.Id);
        Assert.Equal(PaymentStatus.Lunas, check.Status);
    }

    [Fact]
    public async Task UploadBuktiTransferAsync_UpdatesFilePath()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        var payment = await service.CreateAsync(new Payment
        {
            SupplierId = 1,
            TotalHarga = 2000,
            TanggalBayar = DateTime.Today,
            Status = PaymentStatus.Pending
        });

        var updated = await service.UploadBuktiTransferAsync(payment.Id, "/path/to/file.pdf");

        Assert.True(updated);
        var check = await context.Payments.FindAsync(payment.Id);
        Assert.Equal("/path/to/file.pdf", check.BuktiTransferFilePath);
    }

    // ================================================================
    // AddOrUpdatePaymentFromBarangMasukAsync / ReducePaymentFromBarangMasukAsync
    // sekarang punya parameter pengadaanId (int?) di posisi kedua.
    // Kita uji DUA skenario:
    //  1) pengadaanId = null  -> perilaku lama (grouping per supplier+tanggal)
    //  2) pengadaanId diisi   -> payment dikunci ke PengadaanId tertentu,
    //     TIDAK bercampur dengan payment lain di supplier+tanggal yang sama.
    // ================================================================

    [Fact]
    public async Task AddOrUpdatePaymentFromBarangMasukAsync_TanpaPengadaan_GroupingPerSupplierTanggal()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        // Add baru (pengadaanId = null)
        await ((IPayment)service).AddOrUpdatePaymentFromBarangMasukAsync(1, null, DateTime.Today, 500);
        var payment = context.Payments.FirstOrDefault();
        Assert.NotNull(payment);
        Assert.Null(payment.PengadaanId);
        Assert.Equal(500, payment.TotalHarga);

        // Update existing (masih pengadaanId = null, supplier & tanggal sama -> digabung)
        await ((IPayment)service).AddOrUpdatePaymentFromBarangMasukAsync(1, null, DateTime.Today, 300);
        payment = context.Payments.FirstOrDefault();
        Assert.Equal(800, payment.TotalHarga);
    }

    [Fact]
    public async Task AddOrUpdatePaymentFromBarangMasukAsync_DenganPengadaan_TidakBercampurDenganPengadaanLain()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        // Dua "pengadaan" berbeda (pengadaanId 10 & 20) ke supplier & tanggal yang SAMA
        await ((IPayment)service).AddOrUpdatePaymentFromBarangMasukAsync(1, 10, DateTime.Today, 500);
        await ((IPayment)service).AddOrUpdatePaymentFromBarangMasukAsync(1, 20, DateTime.Today, 700);

        // Harus jadi 2 payment terpisah, bukan digabung jadi 1 seperti perilaku lama
        Assert.Equal(2, context.Payments.Count());

        var paymentPengadaan10 = context.Payments.First(p => p.PengadaanId == 10);
        var paymentPengadaan20 = context.Payments.First(p => p.PengadaanId == 20);

        Assert.Equal(500, paymentPengadaan10.TotalHarga);
        Assert.Equal(700, paymentPengadaan20.TotalHarga);

        // Tambahan barang masuk lain untuk pengadaan 10 -> harus nambah ke payment 10 saja
        await ((IPayment)service).AddOrUpdatePaymentFromBarangMasukAsync(1, 10, DateTime.Today, 200);
        Assert.Equal(2, context.Payments.Count()); // tetap 2, bukan nambah payment baru
        paymentPengadaan10 = context.Payments.First(p => p.PengadaanId == 10);
        Assert.Equal(700, paymentPengadaan10.TotalHarga);
    }

    [Fact]
    public async Task ReducePaymentFromBarangMasukAsync_TanpaPengadaan_ReducesTotalHarga()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        await ((IPayment)service).AddOrUpdatePaymentFromBarangMasukAsync(1, null, DateTime.Today, 1000);

        var reduced = await service.ReducePaymentFromBarangMasukAsync(1, null, DateTime.Today, 400);
        var payment = context.Payments.FirstOrDefault();

        Assert.True(reduced);
        Assert.Equal(600, payment.TotalHarga);
    }

    [Fact]
    public async Task ReducePaymentFromBarangMasukAsync_DenganPengadaan_HanyaMenguraniPaymentPengadaanTerkait()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        await ((IPayment)service).AddOrUpdatePaymentFromBarangMasukAsync(1, 10, DateTime.Today, 1000);
        await ((IPayment)service).AddOrUpdatePaymentFromBarangMasukAsync(1, 20, DateTime.Today, 1000);

        var reduced = await service.ReducePaymentFromBarangMasukAsync(1, 10, DateTime.Today, 400);
        Assert.True(reduced);

        var paymentPengadaan10 = context.Payments.First(p => p.PengadaanId == 10);
        var paymentPengadaan20 = context.Payments.First(p => p.PengadaanId == 20);

        Assert.Equal(600, paymentPengadaan10.TotalHarga); // berkurang
        Assert.Equal(1000, paymentPengadaan20.TotalHarga); // tidak terpengaruh
    }

    [Fact]
    public async Task ReducePaymentFromBarangMasukAsync_ReturnsFalse_WhenNotFound()
    {
        var context = await GetInMemoryDbContext();
        var service = GetService(context);

        var reduced = await service.ReducePaymentFromBarangMasukAsync(999, null, DateTime.Today, 100);
        Assert.False(reduced);
    }
}


}