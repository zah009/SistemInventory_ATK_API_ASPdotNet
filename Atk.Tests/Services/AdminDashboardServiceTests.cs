using Atk.Data;
using Atk.Models;
using Atk.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Atk.Tests.Services
{
    public class AdminDashboardServiceTests
    {
        private ApplicationDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnCorrectSummary()
        {
            // ======================
            // 1. Arrange
            // ======================
            var db = GetDb();

            // Seed Users (WAJIB PAKAI PASSWORD)
            var user = new User
            {
                Id = 1,
                Nama = "Admin Test",
                Username = "admin",
                Password = "hashedpassword123" // FIX: tambahkan password
            };
            db.Users.Add(user);

            // Seed Barang
            db.Barangs.AddRange(
                new Barang
                {
                    Id = 1,
                    NamaBarang = "Kertas A4",
                    Stok = 2,       // stok rendah
                    Satuan = "pak"
                },
                new Barang
                {
                    Id = 2,
                    NamaBarang = "Pulpen",
                    Stok = 100,
                    Satuan = "pcs"
                }
            );

            // Seed Permintaan
            db.PermintaanBarangs.AddRange(
                new PermintaanBarang
                {
                    Id = 1,
                    BarangId = 1,
                    UserId = 1,
                    JumlahDiminta = 3,
                    Status = StatusPermintaan.Pending,
                    CreatedAt = DateTime.Now.AddHours(-1)
                },
                new PermintaanBarang
                {
                    Id = 2,
                    BarangId = 2,
                    UserId = 1,
                    JumlahDiminta = 1,
                    Status = StatusPermintaan.Disetujui,
                    CreatedAt = DateTime.Now.AddHours(-2)
                },
                new PermintaanBarang
                {
                    Id = 3,
                    BarangId = 2,
                    UserId = 1,
                    JumlahDiminta = 1,
                    Status = StatusPermintaan.Ditolak,
                    CreatedAt = DateTime.Now.AddHours(-3)
                }
            );

            await db.SaveChangesAsync();

            var service = new AdminDashboardService(db);

            // ======================
            // 2. Act
            // ======================
            var result = await service.GetDashboardAsync();

            // ======================
            // 3. Assert
            // ======================

            // --- Summary ---
            Assert.Equal(2, result.Summary.TotalBarang);
            Assert.Equal(1, result.Summary.TotalPermintaanPending);
            Assert.Equal(1, result.Summary.TotalPermintaanDisetujui);
            Assert.Equal(1, result.Summary.TotalPermintaanDitolak);
            Assert.Equal(1, result.Summary.TotalBarangHampirHabis);  // hanya stok <= 5

            // --- Barang Stok Rendah ---
            Assert.Single(result.BarangStokRendah);
            Assert.Equal("Kertas A4", result.BarangStokRendah[0].NamaBarang);

            // --- Permintaan Terbaru (max 5) ---
            Assert.Equal(3, result.PermintaanTerbaru.Count);
            Assert.Equal(1, result.PermintaanTerbaru[0].PermintaanId); // newest by CreatedAt DESC
        }

        [Fact]
        public async Task GetDashboardAsync_ShouldReturnEmpty_WhenNoData()
        {
            var db = GetDb();
            var service = new AdminDashboardService(db);

            var result = await service.GetDashboardAsync();

            Assert.Equal(0, result.Summary.TotalBarang);
            Assert.Equal(0, result.Summary.TotalPermintaanPending);
            Assert.Empty(result.BarangStokRendah);
            Assert.Empty(result.PermintaanTerbaru);
        }
    }
}
