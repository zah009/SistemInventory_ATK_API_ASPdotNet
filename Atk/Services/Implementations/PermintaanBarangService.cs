using Atk.Data;
using Atk.DTOs;
using Atk.DTOs.PermintaanBarang;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.Services
{
    public class PermintaanBarangService : IPermintaanBarang
    {
        private readonly ApplicationDbContext _context;

        public PermintaanBarangService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. CREATE - Divisi membuat permintaan barang
        // =====================================================
        public async Task<PermintaanBarang> CreateAsync(PermintaanBarangCreateDto dto, int userId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.JumlahDiminta <= 0) throw new Exception("Jumlah diminta harus lebih dari 0.");

            var permintaan = new PermintaanBarang
            {
                UserId = userId,
                BarangId = dto.BarangId,
                JumlahDiminta = dto.JumlahDiminta,
                Alasan = dto.Alasan,
                Status = StatusPermintaan.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.PermintaanBarangs.AddAsync(permintaan);
            await _context.SaveChangesAsync();

            return permintaan;
        }

        // =====================================================
        // 2. GET ALL - Admin/Divisi melihat daftar permintaan
        // =====================================================
        public async Task<List<PermintaanBarang>> GetAllAsync(StatusPermintaan? status = null)
        {
            var query = _context.PermintaanBarangs
                .Include(p => p.Barang)
                .Include(p => p.User)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // =====================================================
        // 3. GET BY ID 
        // =====================================================
        public async Task<PermintaanBarang?> GetByIdAsync(int id)
        {
            return await _context.PermintaanBarangs
                .Include(p => p.Barang)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // =====================================================
        // 4. UPDATE STATUS (Approve / Reject)
        // =====================================================
        //
        // CATATAN KONKURENSI:
        // Approve mengurangi stok Barang setelah mengecek stok cukup.
        // Kalau dua request approve untuk PERMINTAAN BERBEDA tapi BARANG
        // YANG SAMA datang hampir bersamaan, tanpa locking keduanya bisa
        // membaca stok "lama" yang sama sebelum salah satu meng-commit,
        // sehingga stok bisa jadi negatif (lost update / race condition).
        //
        // Diselesaikan dengan: membuka transaksi eksplisit, lalu mengunci
        // baris Barang yang relevan dengan SELECT ... WITH (UPDLOCK, ROWLOCK)
        // (pessimistic locking, khusus SQL Server) sebelum membaca stoknya.
        // Request approve kedua untuk Barang yang sama akan menunggu sampai
        // transaksi pertama commit/rollback, baru membaca stok yang sudah
        // ter-update — sehingga pengecekan "stok cukup" selalu akurat.
        //
        // CATATAN TESTING: transaksi eksplisit & raw SQL lock hint hanya
        // didukung provider relational (SQL Server). Provider InMemory
        // (dipakai di unit test) tidak mendukung keduanya, jadi kita
        // deteksi via Database.IsRelational() dan fallback ke query biasa
        // tanpa lock saat testing — locking sungguhan hanya relevan untuk
        // database production yang sesungguhnya menghadapi concurrent request.
        public async Task<bool> UpdateStatusAsync(int permintaanId, PermintaanBarangUpdateStatusDto dto)
        {
            var isRelational = _context.Database.IsRelational();

            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
            if (isRelational)
                transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var permintaan = await _context.PermintaanBarangs
                    .FirstOrDefaultAsync(p => p.Id == permintaanId);

                if (permintaan == null)
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    return false;
                }

                // Cegah approve/reject ganda (idempotency guard). Tanpa ini,
                // klik dua kali / retry network pada permintaan yang sama bisa
                // mengurangi stok dua kali dan membuat BarangKeluar duplikat.
                if (permintaan.Status != StatusPermintaan.Pending)
                    throw new InvalidOperationException(
                        $"Permintaan #{permintaanId} sudah diproses sebelumnya dengan status {permintaan.Status} dan tidak bisa diproses ulang.");

                permintaan.Status = dto.Status;
                permintaan.UpdatedAt = DateTime.Now;

                // =============== Jika disetujui (barang keluar otomatis) ===============
                if (dto.Status == StatusPermintaan.Disetujui)
                {
                    Barang? barang;

                    if (isRelational)
                    {
                        // Kunci baris Barang ini (UPDLOCK+ROWLOCK) supaya approval
                        // lain terhadap barang yang sama harus antre menunggu
                        // transaksi ini selesai, bukan membaca stok basi.
                        barang = await _context.Barangs
                            .FromSqlInterpolated($"SELECT * FROM Barangs WITH (UPDLOCK, ROWLOCK) WHERE Id = {permintaan.BarangId}")
                            .FirstOrDefaultAsync();
                    }
                    else
                    {
                        barang = await _context.Barangs
                            .FirstOrDefaultAsync(b => b.Id == permintaan.BarangId);
                    }

                    if (barang == null)
                        throw new KeyNotFoundException($"Barang dengan id {permintaan.BarangId} tidak ditemukan.");

                    if (barang.Stok < permintaan.JumlahDiminta)
                        throw new InvalidOperationException("Stok barang tidak mencukupi.");

                    barang.Stok -= permintaan.JumlahDiminta;

                    var barangKeluar = new BarangKeluar
                    {
                        PermintaanId = permintaan.Id,
                        BarangId = permintaan.BarangId,
                        JumlahKeluar = permintaan.JumlahDiminta,
                        TanggalKeluar = DateTime.Now,
                        Keterangan = dto.Keterangan ?? "Permintaan disetujui",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await _context.BarangKeluars.AddAsync(barangKeluar);
                }
                // =============== Jika ditolak ===============
                else if (dto.Status == StatusPermintaan.Ditolak)
                {
                    if (!string.IsNullOrWhiteSpace(dto.Keterangan))
                        permintaan.Alasan = dto.Keterangan;
                }

                await _context.SaveChangesAsync();
                if (transaction != null) await transaction.CommitAsync();

                return true;
            }
            catch
            {
                if (transaction != null) await transaction.RollbackAsync();
                throw;
            }
        }
    }
}