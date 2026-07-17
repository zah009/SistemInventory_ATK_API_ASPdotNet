using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs;
using Atk.DTOs.Barang;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Implementations
{
    public class BarangService : IBarang
    {
        private readonly ApplicationDbContext _context;
         public BarangService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BarangResponseDto> CreateAsync(BarangCreateDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto)); 
            if (string.IsNullOrWhiteSpace(dto.KodeBarang)) 
                throw new ArgumentException("KodeBarang harus diisi"); 
            if (string.IsNullOrWhiteSpace(dto.NamaBarang)) 
                throw new ArgumentException("NamaBarang harus diisi"); 
            if (dto.Stok < 0) 
                throw new ArgumentException("Stok minimal 0"); 
            if (string.IsNullOrWhiteSpace(dto.Satuan)) 
                throw new ArgumentException("Satuan harus diisi"); 

            var now = DateTime.Now; 
            var barang = new Barang 
            { 
                KodeBarang = dto.KodeBarang, 
                NamaBarang = dto.NamaBarang, 
                Stok = dto.Stok, 
                Satuan = dto.Satuan, 
                CreatedAt = now, 
                UpdatedAt = now 
            }; 
            
            _context.Barangs.Add(barang); 
            await _context.SaveChangesAsync(); 

            return new BarangResponseDto 
            { 
                Id = barang.Id, 
                KodeBarang = barang.KodeBarang, 
                NamaBarang = barang.NamaBarang, 
                Stok = barang.Stok, 
                Satuan = barang.Satuan, 
                CreatedAt = barang.CreatedAt, 
                UpdatedAt = barang.UpdatedAt 
                }; 
        } 

        public async Task<bool> DeleteAsync(int id)
        {
            var barang = await _context.Barangs.FirstOrDefaultAsync(b => b.Id == id); 
            
            if (barang == null) return false; 

            _context.Barangs.Remove(barang); 
            await _context.SaveChangesAsync(); 
            return true;
        }

        public async Task<bool> ExistsByName(string namaBarang)
        {
            return await _context.Barangs.AnyAsync(b => b.NamaBarang == namaBarang);
        }

        public async Task<IEnumerable<BarangResponseDto>> GetAllAsync()
        {
            var list = await _context.Barangs.AsNoTracking().ToListAsync(); 
            return list.Select(b => new BarangResponseDto 
            { 
                Id = b.Id, 
                KodeBarang = b.KodeBarang, 
                NamaBarang = b.NamaBarang, 
                Stok = b.Stok, 
                Satuan = b.Satuan, 
                CreatedAt = b.CreatedAt, 
                UpdatedAt = b.UpdatedAt 
            });

        }

        public async Task<BarangResponseDto?> GetByIdAsync(int id)
        {
            var b = await _context.Barangs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id); 
            if (b == null) return null; 

            return new BarangResponseDto 
            { 
                Id = b.Id, 
                KodeBarang = b.KodeBarang, 
                NamaBarang = b.NamaBarang, 
                Stok = b.Stok, 
                Satuan = b.Satuan, 
                CreatedAt = b.CreatedAt, 
                UpdatedAt = b.UpdatedAt 
            };
        }

        public async Task<BarangResponseDto> UpdateAsync(int id, BarangUpdateDto dto)
        {
            var b = await _context.Barangs.FirstOrDefaultAsync(x => x.Id == id); 
            if (b == null) 
                throw new KeyNotFoundException("Barang tidak ditemukan"); 
            
            b.KodeBarang = dto.KodeBarang; 
            b.NamaBarang = dto.NamaBarang; 
            b.Stok = dto.Stok; 
            b.Satuan = dto.Satuan; 
            b.UpdatedAt = DateTime.Now; 
            
            await _context.SaveChangesAsync(); 
            return new BarangResponseDto 
            { 
                Id = b.Id, 
                KodeBarang = b.KodeBarang, 
                NamaBarang = b.NamaBarang, 
                Stok = b.Stok, 
                Satuan = b.Satuan, 
                CreatedAt = b.CreatedAt, 
                UpdatedAt = b.UpdatedAt 
            };
        }
    }
}