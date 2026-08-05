using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.Divisi;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Implementations
{
    public class DivisiService : IDivisi
    {
        private readonly ApplicationDbContext _context;
        public DivisiService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Divisi>> GetAllAsync()
        {
            return await _context.Divisis.ToListAsync();
        }

        public async Task<Divisi> CreateAsync(DivisiCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nama))
                throw new ArgumentException("Nama divisi harus diisi.");

            // Cegah divisi duplikat (case-insensitive + abaikan spasi di
            // pinggir) — tanpa ini gampang kejadian "IT" dan "it " dianggap
            // dua divisi berbeda gara-gara typo/inkonsistensi input admin.
            var namaTrim = dto.Nama.Trim();
            var sudahAda = await _context.Divisis
                .AnyAsync(d => d.Nama != null && d.Nama.ToLower() == namaTrim.ToLower());
            if (sudahAda)
                throw new InvalidOperationException($"Divisi '{namaTrim}' sudah ada.");

            var divisi = new Divisi
            {
                Nama = namaTrim
            };

            _context.Divisis.Add(divisi);
            await _context.SaveChangesAsync();
            return divisi;
        }

        public async Task<Divisi> UpdateAsync(int id, DivisiCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nama))
                throw new ArgumentException("Nama divisi harus diisi.");

            var divisi = await _context.Divisis.FindAsync(id);
            if (divisi == null) return null;

            var namaTrim = dto.Nama.Trim();

            if (!string.Equals(divisi.Nama?.Trim(), namaTrim, StringComparison.OrdinalIgnoreCase))
            {
                var dipakaiDivisiLain = await _context.Divisis
                    .AnyAsync(d => d.Id != id && d.Nama != null && d.Nama.ToLower() == namaTrim.ToLower());
                if (dipakaiDivisiLain)
                    throw new InvalidOperationException($"Divisi '{namaTrim}' sudah ada.");
            }

            divisi.Nama = namaTrim;

            await _context.SaveChangesAsync();
            return divisi;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var divisi = await _context.Divisis.FindAsync(id);
            if (divisi == null) return false;

            _context.Divisis.Remove(divisi);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}