using Atk.Data;
using Atk.DTOs;
using Atk.DTOs.PermintaanBarang;
using Atk.Models;
using Atk.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Atk.Tests.Services
{
    public class PermintaanBarangServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new ApplicationDbContext(options);

            // ===========================
            // SEED USER (FIXED)
            // ===========================
            db.Users.Add(new User
            {
                Id = 1,
                Username = "divisi1",
                Password = "hashed-password",
                Nama = "Divisi Satu",        // ← FIX: Required field
                Role = UserRole.Divisi       // ← FIX: Bukan string
            });

            // ===========================
            // SEED BARANG
            // ===========================
            db.Barangs.Add(new Barang
            {
                Id = 10,
                NamaBarang = "Kertas A4",
                Satuan = "pak",
                Stok = 50
            });

            db.SaveChanges();
            return db;
        }

        private PermintaanBarangService GetService(ApplicationDbContext context)
        {
            return new PermintaanBarangService(context);
        }

        // ===========================================================
        // TEST #1: CREATE PERMINTAAN
        // ===========================================================
        [Fact]
        public async Task CreateAsync_Should_Create_Permintaan()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var dto = new PermintaanBarangCreateDto
            {
                BarangId = 10,
                JumlahDiminta = 5,
                Alasan = "Butuh untuk meeting"
            };

            var result = await service.CreateAsync(dto, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal(10, result.BarangId);
            Assert.Equal(5, result.JumlahDiminta);
            Assert.Equal(StatusPermintaan.Pending, result.Status);
        }

        // ===========================================================
        // TEST #2: GET ALL PERMINTAAN
        // ===========================================================
        [Fact]
        public async Task GetAllAsync_Should_Return_List()
        {
            var db = GetDbContext();
            var service = GetService(db);

            db.PermintaanBarangs.Add(new PermintaanBarang
            {
                Id = 1,
                UserId = 1,
                BarangId = 10,
                JumlahDiminta = 3,
                Status = StatusPermintaan.Pending,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            var result = await service.GetAllAsync();
            Assert.Single(result);
        }

        // ===========================================================
        // TEST #3: APPROVE (stok berkurang + barang keluar)
        // ===========================================================
        [Fact]
        public async Task UpdateStatusAsync_Should_Approve_And_Reduce_Stock()
        {
            var db = GetDbContext();
            var service = GetService(db);

            db.PermintaanBarangs.Add(new PermintaanBarang
            {
                Id = 5,
                UserId = 1,
                BarangId = 10,
                JumlahDiminta = 10,
                Status = StatusPermintaan.Pending,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            var dto = new PermintaanBarangUpdateStatusDto
            {
                Status = StatusPermintaan.Disetujui,
                Keterangan = "Approved for meeting"
            };

            var success = await service.UpdateStatusAsync(5, dto);

            Assert.True(success);

            var permintaan = db.PermintaanBarangs.Find(5);
            var barang = db.Barangs.Find(10);
            var barangKeluar = db.BarangKeluars.FirstOrDefault();

            Assert.Equal(StatusPermintaan.Disetujui, permintaan.Status);
            Assert.Equal(40, barang.Stok); 
            Assert.NotNull(barangKeluar);
            Assert.Equal(10, barangKeluar.JumlahKeluar);
        }

        // ===========================================================
        // TEST #4: REJECT
        // ===========================================================
        [Fact]
        public async Task UpdateStatusAsync_Should_Reject_And_Save_Reason()
        {
            var db = GetDbContext();
            var service = GetService(db);

            db.PermintaanBarangs.Add(new PermintaanBarang
            {
                Id = 99,
                UserId = 1,
                BarangId = 10,
                JumlahDiminta = 5,
                Status = StatusPermintaan.Pending,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            var dto = new PermintaanBarangUpdateStatusDto
            {
                Status = StatusPermintaan.Ditolak,
                Keterangan = "Tidak ada stok"
            };

            var success = await service.UpdateStatusAsync(99, dto);

            Assert.True(success);

            var permintaan = db.PermintaanBarangs.Find(99);

            Assert.Equal(StatusPermintaan.Ditolak, permintaan.Status);
            Assert.Equal("Tidak ada stok", permintaan.Alasan);
        }

        // ===========================================================
        // TEST #5: NOT FOUND
        // ===========================================================
        [Fact]
        public async Task UpdateStatusAsync_Should_Return_False_When_NotFound()
        {
            var db = GetDbContext();
            var service = GetService(db);

            var dto = new PermintaanBarangUpdateStatusDto
            {
                Status = StatusPermintaan.Disetujui
            };

            var success = await service.UpdateStatusAsync(999, dto);

            Assert.False(success);
        }
    }
}
