using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.BarangMasuk;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using SistemInventoriAtk.Models;

namespace Atk.Services.Implementations
{
    public class BarangMasukService : IBarangMasuk
    {
        private readonly ApplicationDbContext _context;
        private readonly IPayment _paymentService;

        public BarangMasukService(ApplicationDbContext context, IPayment paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        // Validasi & terapkan efek samping ke PengadaanBarang (JumlahDiterima,
        // Status) kalau BarangMasuk ini mereferensikan sebuah pengadaan.
        // Mengembalikan SupplierId final yang dipakai (auto-fill dari
        // pengadaan kalau dto tidak mengisi SupplierId sendiri).
        private async Task<int?> ApplyPengadaanLinkAsync(int? pengadaanId, int barangId, int? supplierId, int jumlahMasuk)
        {
            if (!pengadaanId.HasValue) return supplierId;

            var pengadaan = await _context.PengadaanBarangs.FindAsync(pengadaanId.Value);
            if (pengadaan == null)
                throw new KeyNotFoundException($"Pengadaan dengan id {pengadaanId.Value} tidak ditemukan.");

            if (pengadaan.BarangId != barangId)
                throw new ArgumentException(
                    $"BarangId ({barangId}) tidak sesuai dengan Pengadaan #{pengadaanId.Value} (barang id {pengadaan.BarangId}).");

            if (supplierId.HasValue && supplierId.Value != pengadaan.SupplierId)
                throw new ArgumentException(
                    $"SupplierId ({supplierId.Value}) tidak sesuai dengan Pengadaan #{pengadaanId.Value} (supplier id {pengadaan.SupplierId}).");

            if (pengadaan.Status == StatusPengadaan.Selesai || pengadaan.Status == StatusPengadaan.Dibatalkan)
                throw new InvalidOperationException(
                    $"Pengadaan #{pengadaanId.Value} sudah berstatus {pengadaan.Status} dan tidak bisa menerima Barang Masuk baru.");

            if (pengadaan.JumlahDiterima + jumlahMasuk > pengadaan.JumlahDiajukan)
                throw new InvalidOperationException(
                    $"Jumlah masuk melebihi sisa pengadaan. Diajukan: {pengadaan.JumlahDiajukan}, sudah diterima: {pengadaan.JumlahDiterima}, sisa: {pengadaan.JumlahDiajukan - pengadaan.JumlahDiterima}.");

            pengadaan.JumlahDiterima += jumlahMasuk;
            pengadaan.Status = pengadaan.JumlahDiterima >= pengadaan.JumlahDiajukan
                ? StatusPengadaan.Selesai
                : StatusPengadaan.Disetujui;
            pengadaan.UpdatedAt = DateTime.Now;

            return pengadaan.SupplierId;
        }

        // Balikkan efek ApplyPengadaanLinkAsync, dipakai saat BarangMasuk
        // diupdate atau dihapus.
        private async Task RevertPengadaanLinkAsync(int? pengadaanId, int jumlahMasuk)
        {
            if (!pengadaanId.HasValue) return;

            var pengadaan = await _context.PengadaanBarangs.FindAsync(pengadaanId.Value);
            if (pengadaan == null) return; // sudah terhapus, tidak ada yang perlu di-revert

            pengadaan.JumlahDiterima -= jumlahMasuk;
            if (pengadaan.JumlahDiterima < 0) pengadaan.JumlahDiterima = 0;

            // Selesai -> balik ke Disetujui kalau jumlahnya sudah tidak lagi penuh.
            if (pengadaan.Status == StatusPengadaan.Selesai && pengadaan.JumlahDiterima < pengadaan.JumlahDiajukan)
                pengadaan.Status = StatusPengadaan.Disetujui;

            // Kalau semua barang masuk terkait dibatalkan, balik ke Diajukan.
            if (pengadaan.JumlahDiterima == 0 && pengadaan.Status == StatusPengadaan.Disetujui)
                pengadaan.Status = StatusPengadaan.Diajukan;

            pengadaan.UpdatedAt = DateTime.Now;
        }

        public async Task<BarangMasukResponseDto> CreateAsync(BarangMasukCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.JumlahMasuk <= 0) throw new ArgumentException("JumlahMasuk harus > 0");

            var now = DateTime.Now;

            var barang = await _context.Barangs.FindAsync(dto.BarangId);
            if (barang == null)
                throw new KeyNotFoundException($"Barang dengan id {dto.BarangId} tidak ditemukan.");

            var supplierId = await ApplyPengadaanLinkAsync(dto.PengadaanId, dto.BarangId, dto.SupplierId, dto.JumlahMasuk);

            var entity = new BarangMasuk
            {
                BarangId = dto.BarangId,
                SupplierId = supplierId,
                PengadaanId = dto.PengadaanId,
                JumlahMasuk = dto.JumlahMasuk,
                HargaSatuan = dto.HargaSatuan,
                TanggalMasuk = dto.TanggalMasuk,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.BarangMasuks.AddAsync(entity);

            barang.Stok += dto.JumlahMasuk;

            decimal subtotal = dto.JumlahMasuk * dto.HargaSatuan;

            if (supplierId.HasValue)
            {
                await _paymentService.AddOrUpdatePaymentFromBarangMasukAsync(supplierId.Value, dto.PengadaanId, dto.TanggalMasuk, subtotal);
            }

            await _context.SaveChangesAsync();

            return ToDto(entity);
        }

        public async Task<IEnumerable<BarangMasukResponseDto>> CreateBulkAsync(IEnumerable<BarangMasukCreateDto> dtos)
        {
            if (dtos == null) throw new ArgumentNullException(nameof(dtos));

            var result = new List<BarangMasukResponseDto>();
            var now = DateTime.Now;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var dto in dtos)
                {
                    if (dto.JumlahMasuk <= 0)
                        throw new ArgumentException("JumlahMasuk harus lebih dari 0.");

                    var barang = await _context.Barangs.FindAsync(dto.BarangId);
                    if (barang == null)
                        throw new KeyNotFoundException($"Barang id {dto.BarangId} tidak ditemukan.");

                    var supplierId = await ApplyPengadaanLinkAsync(dto.PengadaanId, dto.BarangId, dto.SupplierId, dto.JumlahMasuk);

                    var entity = new BarangMasuk
                    {
                        BarangId = dto.BarangId,
                        SupplierId = supplierId,
                        PengadaanId = dto.PengadaanId,
                        JumlahMasuk = dto.JumlahMasuk,
                        HargaSatuan = dto.HargaSatuan,
                        TanggalMasuk = dto.TanggalMasuk,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    await _context.BarangMasuks.AddAsync(entity);
                    barang.Stok += dto.JumlahMasuk;

                    decimal subtotal = dto.JumlahMasuk * dto.HargaSatuan;
                    if (supplierId.HasValue)
                    {
                        await _paymentService.AddOrUpdatePaymentFromBarangMasukAsync(supplierId.Value, dto.PengadaanId, dto.TanggalMasuk, subtotal);
                    }

                    result.Add(ToDto(entity));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<BarangMasukResponseDto>> GetAllAsync()
        {
            var list = await _context.BarangMasuks
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return list.Select(ToDto);
        }

        public async Task<BarangMasukResponseDto?> GetByIdAsync(int id)
        {
            var p = await _context.BarangMasuks
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return p == null ? null : ToDto(p);
        }

        public async Task<bool> UpdateAsync(int id, BarangMasukUpdateDto dto)
        {
            var entity = await _context.BarangMasuks.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            var oldSubtotal = entity.JumlahMasuk * entity.HargaSatuan;
            var oldSupplierId = entity.SupplierId;
            var oldPengadaanId = entity.PengadaanId;
            var oldJumlah = entity.JumlahMasuk;

            // Kembalikan efek lama dulu (stok & pengadaan & payment)
            var oldBarang = await _context.Barangs.FindAsync(entity.BarangId);
            if (oldBarang != null)
                oldBarang.Stok -= oldJumlah;

            await RevertPengadaanLinkAsync(oldPengadaanId, oldJumlah);

            if (oldSupplierId.HasValue)
            {
                await _paymentService.ReducePaymentFromBarangMasukAsync(oldSupplierId.Value, oldPengadaanId, entity.TanggalMasuk, oldSubtotal);
            }

            // Terapkan efek baru
            var newBarang = await _context.Barangs.FindAsync(dto.BarangId);
            if (newBarang == null)
                throw new KeyNotFoundException($"Barang dengan id {dto.BarangId} tidak ditemukan.");

            var newSupplierId = await ApplyPengadaanLinkAsync(dto.PengadaanId, dto.BarangId, dto.SupplierId, dto.JumlahMasuk);

            entity.BarangId = dto.BarangId;
            entity.SupplierId = newSupplierId;
            entity.PengadaanId = dto.PengadaanId;
            entity.JumlahMasuk = dto.JumlahMasuk;
            entity.HargaSatuan = dto.HargaSatuan;
            entity.TanggalMasuk = dto.TanggalMasuk;
            entity.UpdatedAt = DateTime.Now;

            newBarang.Stok += dto.JumlahMasuk;

            var newSubtotal = dto.JumlahMasuk * dto.HargaSatuan;
            if (newSupplierId.HasValue)
            {
                await _paymentService.AddOrUpdatePaymentFromBarangMasukAsync(newSupplierId.Value, dto.PengadaanId, dto.TanggalMasuk, newSubtotal);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.BarangMasuks.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            var subtotal = entity.JumlahMasuk * entity.HargaSatuan;

            var barang = await _context.Barangs.FindAsync(entity.BarangId);
            if (barang != null)
                barang.Stok -= entity.JumlahMasuk;

            await RevertPengadaanLinkAsync(entity.PengadaanId, entity.JumlahMasuk);

            if (entity.SupplierId.HasValue)
            {
                await _paymentService.ReducePaymentFromBarangMasukAsync(entity.SupplierId.Value, entity.PengadaanId, entity.TanggalMasuk, subtotal);
            }

            _context.BarangMasuks.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalBarangMasukByDateAsync(DateTime date)
        {
            var d = date.Date;
            return await _context.BarangMasuks
                .Where(b => b.TanggalMasuk.Date == d)
                .SumAsync(b => b.JumlahMasuk);
        }

        private static BarangMasukResponseDto ToDto(BarangMasuk entity) => new BarangMasukResponseDto
        {
            Id = entity.Id,
            BarangId = entity.BarangId,
            SupplierId = entity.SupplierId,
            PengadaanId = entity.PengadaanId,
            JumlahMasuk = entity.JumlahMasuk,
            HargaSatuan = entity.HargaSatuan,
            TanggalMasuk = entity.TanggalMasuk,
            CreatedAt = entity.CreatedAt
        };
    }
}