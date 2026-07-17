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
            var divisi = new Divisi
            {
                Nama = dto.Nama
            };

            _context.Divisis.Add(divisi);
            await _context.SaveChangesAsync();
            return divisi;
        }

        public async Task<Divisi> UpdateAsync(int id, DivisiCreateDto dto)
        {
            var divisi = await _context.Divisis.FindAsync(id);
            if (divisi == null) return null;

            divisi.Nama = dto.Nama;

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