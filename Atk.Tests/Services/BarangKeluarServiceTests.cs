using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Implementations
{
    public class BarangKeluarService : IBarangKeluar
    {
        private readonly ApplicationDbContext _context;

        public BarangKeluarService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ PERBAIKAN: Include nested User.Divisi
        public async Task<List<BarangKeluar>> GetAllAsync()
        {
            return await _context.BarangKeluars
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                    .ThenInclude(p => p.User)           // ✅ Include User
                            // ✅ Include Divisi dari User
                .OrderByDescending(x => x.TanggalKeluar)
                .ToListAsync();
        }

        public async Task<BarangKeluar?> GetByIdAsync(int id)
        {
            return await _context.BarangKeluars
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                    .ThenInclude(p => p.User)
                       
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<BarangKeluar>> GetByPermintaanAsync(int permintaanId)
        {
            return await _context.BarangKeluars
                .Where(x => x.PermintaanId == permintaanId)
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                    .ThenInclude(p => p.User)
                 
                .ToListAsync();
        }

        public async Task<List<BarangKeluar>> GetByBarangAsync(int barangId)
        {
            return await _context.BarangKeluars
                .Where(x => x.BarangId == barangId)
                .Include(x => x.Barang)
                .Include(x => x.PermintaanBarang)
                    .ThenInclude(p => p.User)
                        
                .ToListAsync();
        }
    }
}