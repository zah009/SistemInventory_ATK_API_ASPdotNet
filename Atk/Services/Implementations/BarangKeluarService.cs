using Atk.Data;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services
{
    public class BarangKeluarService : IBarangKeluar
    {
        private readonly ApplicationDbContext _context;

        public BarangKeluarService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ TAMBAHKAN .ThenInclude(p => p.User)
        public async Task<List<BarangKeluar>> GetAllAsync()
        {
            return await _context.BarangKeluars
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                    .ThenInclude(p => p.User)  // ✅ TAMBAH BARIS INI!
                .OrderByDescending(x => x.TanggalKeluar)
                .ToListAsync();
        }
    
        // ✅ TAMBAHKAN JUGA DI SINI
        public async Task<BarangKeluar?> GetByIdAsync(int id)
        {
            return await _context.BarangKeluars
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                    .ThenInclude(p => p.User)  // ✅ TAMBAH BARIS INI!
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // ✅ TAMBAHKAN JUGA DI SINI
        public async Task<List<BarangKeluar>> GetByPermintaanAsync(int permintaanId)
        {
            return await _context.BarangKeluars
                .Where(x => x.PermintaanId == permintaanId)
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                    .ThenInclude(p => p.User)  // ✅ TAMBAH BARIS INI!
                .ToListAsync();
        }

        // ✅ TAMBAHKAN JUGA DI SINI
        public async Task<List<BarangKeluar>> GetByBarangAsync(int barangId)
        {
            return await _context.BarangKeluars
                .Where(x => x.BarangId == barangId)
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                    .ThenInclude(p => p.User)  // ✅ TAMBAH BARIS INI!
                .ToListAsync();
        }
    }
}