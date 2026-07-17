using Atk.Data;
using Atk.DTOs;
using Atk.DTOs.PermintaanBarang;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        public async Task<bool> UpdateStatusAsync(int permintaanId, PermintaanBarangUpdateStatusDto dto)
        {
            var permintaan = await _context.PermintaanBarangs
                .Include(p => p.Barang)
                .FirstOrDefaultAsync(p => p.Id == permintaanId);

            if (permintaan == null)
                return false;

            permintaan.Status = dto.Status;
            permintaan.UpdatedAt = DateTime.Now;

            // =============== Jika disetujui (barang keluar otomatis) ===============
            if (dto.Status == StatusPermintaan.Disetujui)
            {
                var barang = permintaan.Barang;

                if (barang.Stok < permintaan.JumlahDiminta)
                    throw new Exception("Stok barang tidak mencukupi.");

                // kurangi stok
                barang.Stok -= permintaan.JumlahDiminta;

                // catat barang keluar
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
            return true;
        }
    }
}
