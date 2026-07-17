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
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.NamaBarang))
                throw new ArgumentException("Nama Barang harus diisi");
            
             // Validasi jumlah diajukan
            if (dto.JumlahDiajukan <= 0)
                throw new ArgumentException("Jumlah Diajukan harus lebih dari 0");

            var now = DateTime.Now;

            var pengadaan = new PengadaanBarang
            {
                NamaBarang = dto.NamaBarang,
                Satuan = dto.Satuan,
                JumlahDiajukan = dto.JumlahDiajukan,
                TanggalPengajuan = dto.TanggalPengajuan,
                Keterangan = dto.Keterangan,
                SupplierId = dto.SupplierId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.PengadaanBarangs.Add(pengadaan);
            await _context.SaveChangesAsync();

            return new PengadaanResponseDto
            {
                Id = pengadaan.Id,
                NamaBarang = pengadaan.NamaBarang,
                Satuan = pengadaan.Satuan,
                JumlahDiajukan = pengadaan.JumlahDiajukan,
                TanggalPengajuan = pengadaan.TanggalPengajuan,
                Keterangan = pengadaan.Keterangan,
                SupplierId = pengadaan.SupplierId,
                CreatedAt = pengadaan.CreatedAt
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var Del = await _context.PengadaanBarangs.FirstOrDefaultAsync(d => d.Id == id);
            if (Del == null) return false;

            _context.PengadaanBarangs.Remove(Del);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByName(string namaBarang)
        {
            return await _context.PengadaanBarangs.AnyAsync(s => s.NamaBarang == namaBarang);
        }


        public async Task<IEnumerable<PengadaanResponseDto>> GetAllAsync()
        {
            var list = await _context.PengadaanBarangs.AsNoTracking().ToListAsync();

            return list.Select(p => new PengadaanResponseDto
            {
                Id = p.Id,
                NamaBarang = p.NamaBarang,
                Satuan = p.Satuan,
                JumlahDiajukan = p.JumlahDiajukan,
                TanggalPengajuan = p.TanggalPengajuan,
                Keterangan = p.Keterangan,
                SupplierId = p.SupplierId,
                CreatedAt = p.CreatedAt
            });
        }

        public async Task<PengadaanResponseDto?> GetByIdAsync(int id)
        {
            var p = await _context.PengadaanBarangs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p == null ) return null;

            return new PengadaanResponseDto
            {
                 Id = p.Id,
                NamaBarang = p.NamaBarang,
                Satuan = p.Satuan,
                JumlahDiajukan = p.JumlahDiajukan,
                TanggalPengajuan = p.TanggalPengajuan,
                Keterangan = p.Keterangan,
                SupplierId = p.SupplierId,
                CreatedAt = p.CreatedAt
            };
        }

        public async Task<PengadaanResponseDto> UpdateAsync(int id, PengadaanUpdateDto dto)
        {
            var p = await _context.PengadaanBarangs.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) throw new KeyNotFoundException("PengadaanBarang tidak ditemukan");

            p.NamaBarang = dto.NamaBarang;
            p.Satuan = dto.Satuan;
            p.JumlahDiajukan = dto.JumlahDiajukan;
            p.TanggalPengajuan = dto.TanggalPengajuan;
            p.Keterangan = dto.Keterangan;
            p.SupplierId = dto.SupplierId;
            p.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new PengadaanResponseDto
            {
                Id = p.Id,
                NamaBarang = p.NamaBarang,
                Satuan = p.Satuan,
                JumlahDiajukan = p.JumlahDiajukan,
                TanggalPengajuan = p.TanggalPengajuan,
                Keterangan = p.Keterangan,
                SupplierId = p.SupplierId,
                CreatedAt = p.CreatedAt
            };
        }
    }
}