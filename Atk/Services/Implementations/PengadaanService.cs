using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.Pengadaan;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using SistemInventoriAtk.Models;

namespace Atk.Services.Implementations
{
    public class PengadaanService : IPengadaan
    {
        private readonly ApplicationDbContext _context;
        public PengadaanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PengadaanResponseDto> CreateAsync(PengadaanCreateDto dto)
        {
            // Delegasi ke CreateInternalAsync (dipakai bersama oleh CreateBulkAsync)
            // supaya aturan validasi & snapshot data tidak duplikat di dua tempat.
            return await CreateInternalAsync(dto);
        }

        public async Task<List<PengadaanResponseDto>> CreateBulkAsync(IEnumerable<PengadaanCreateDto> dtos)
        {
            if (dtos == null) throw new ArgumentNullException(nameof(dtos));

            var dtoList = dtos.ToList();
            var result = new List<PengadaanResponseDto>();

            var isRelational = _context.Database.IsRelational();
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
            if (isRelational)
                transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var dto in dtoList)
                {
                    if (await HasOpenPengadaanAsync(dto.BarangId, dto.SupplierId))
                        throw new InvalidOperationException(
                            $"Barang id {dto.BarangId} sudah punya pengadaan aktif (belum Selesai/Dibatalkan) ke supplier id {dto.SupplierId}");

                    // Bagian bawah ini sama persis dengan logika CreateAsync,
                    // dijalankan di dalam satu transaksi supaya kalau salah
                    // satu item di batch gagal, semua item sebelumnya ikut
                    // di-rollback (all-or-nothing), bukan tersimpan sebagian.
                    var pengadaan = await CreateInternalAsync(dto);
                    result.Add(pengadaan);
                }

                if (transaction != null) await transaction.CommitAsync();
                return result;
            }
            catch
            {
                if (transaction != null) await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<PengadaanResponseDto> CreateInternalAsync(PengadaanCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (dto.JumlahDiajukan <= 0)
                throw new ArgumentException("Jumlah Diajukan harus lebih dari 0");

            var barang = await _context.Barangs.FindAsync(dto.BarangId);
            if (barang == null)
                throw new KeyNotFoundException($"Barang dengan id {dto.BarangId} tidak ditemukan.");

            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId);
            if (!supplierExists)
                throw new KeyNotFoundException($"Supplier dengan id {dto.SupplierId} tidak ditemukan.");

            var now = DateTime.Now;

            var pengadaan = new PengadaanBarang
            {
                BarangId = dto.BarangId,
                NamaBarang = barang.NamaBarang,
                Satuan = dto.Satuan ?? barang.Satuan,
                JumlahDiajukan = dto.JumlahDiajukan,
                JumlahDiterima = 0,
                Status = StatusPengadaan.Diajukan,
                TanggalPengajuan = dto.TanggalPengajuan,
                Keterangan = dto.Keterangan,
                SupplierId = dto.SupplierId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.PengadaanBarangs.Add(pengadaan);
            await _context.SaveChangesAsync();

            return ToDto(pengadaan);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var del = await _context.PengadaanBarangs.FirstOrDefaultAsync(d => d.Id == id);
            if (del == null) return false;

            // Cegah hapus pengadaan yang sudah punya barang masuk terkait,
            // supaya riwayat BarangMasuk tidak jadi orphan secara logis.
            var punyaBarangMasuk = await _context.BarangMasuks.AnyAsync(bm => bm.PengadaanId == id);
            if (punyaBarangMasuk)
                throw new InvalidOperationException(
                    "Pengadaan ini sudah punya riwayat Barang Masuk dan tidak bisa dihapus. Batalkan (Dibatalkan) saja jika perlu.");

            _context.PengadaanBarangs.Remove(del);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasOpenPengadaanAsync(int barangId, int supplierId)
        {
            return await _context.PengadaanBarangs.AnyAsync(p =>
                p.BarangId == barangId &&
                p.SupplierId == supplierId &&
                p.Status == StatusPengadaan.Diajukan);
        }

        public async Task<IEnumerable<PengadaanResponseDto>> GetAllAsync()
        {
            var list = await _context.PengadaanBarangs.AsNoTracking().ToListAsync();
            return list.Select(ToDto);
        }

        public async Task<PengadaanResponseDto?> GetByIdAsync(int id)
        {
            var p = await _context.PengadaanBarangs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return p == null ? null : ToDto(p);
        }

        public async Task<PengadaanResponseDto> UpdateAsync(int id, PengadaanUpdateDto dto)
        {
            var p = await _context.PengadaanBarangs.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) throw new KeyNotFoundException("PengadaanBarang tidak ditemukan");

            // Tidak boleh ganti barang/jumlah lagi kalau sudah mulai dipenuhi,
            // supaya JumlahDiterima yang sudah tercatat tetap konsisten.
            if (p.JumlahDiterima > 0 && (dto.BarangId != p.BarangId || dto.JumlahDiajukan < p.JumlahDiterima))
                throw new InvalidOperationException(
                    "Pengadaan sudah sebagian/seluruhnya dipenuhi lewat Barang Masuk, tidak bisa mengubah Barang atau menurunkan JumlahDiajukan di bawah JumlahDiterima.");

            var barang = await _context.Barangs.FindAsync(dto.BarangId);
            if (barang == null)
                throw new KeyNotFoundException($"Barang dengan id {dto.BarangId} tidak ditemukan.");

            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId);
            if (!supplierExists)
                throw new KeyNotFoundException($"Supplier dengan id {dto.SupplierId} tidak ditemukan.");

            p.BarangId = dto.BarangId;
            p.NamaBarang = barang.NamaBarang;
            p.Satuan = dto.Satuan ?? barang.Satuan;
            p.JumlahDiajukan = dto.JumlahDiajukan;
            p.TanggalPengajuan = dto.TanggalPengajuan;
            p.Keterangan = dto.Keterangan;
            p.SupplierId = dto.SupplierId;
            p.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return ToDto(p);
        }

        private static PengadaanResponseDto ToDto(PengadaanBarang p) => new PengadaanResponseDto
        {
            Id = p.Id,
            BarangId = p.BarangId,
            NamaBarang = p.NamaBarang,
            Satuan = p.Satuan,
            JumlahDiajukan = p.JumlahDiajukan,
            JumlahDiterima = p.JumlahDiterima,
            Status = p.Status,
            TanggalPengajuan = p.TanggalPengajuan,
            Keterangan = p.Keterangan,
            SupplierId = p.SupplierId,
            CreatedAt = p.CreatedAt
        };
    }
}