using Atk.Data;
using Atk.DTOs.AdminDashboard;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Implementations
{
    public class AdminDashboardService : IAdminDashboard
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            // ============================
            // 1. Summary
            // ============================
            var summary = new DashboardSummaryDto
            {
                TotalBarang = await _context.Barangs.CountAsync(),
                TotalPermintaanPending = await _context.PermintaanBarangs
                    .Where(p => p.Status == Models.StatusPermintaan.Pending)
                    .CountAsync(),
                TotalPermintaanDisetujui = await _context.PermintaanBarangs
                    .Where(p => p.Status == Models.StatusPermintaan.Disetujui)
                    .CountAsync(),
                TotalPermintaanDitolak = await _context.PermintaanBarangs
                    .Where(p => p.Status == Models.StatusPermintaan.Ditolak)
                    .CountAsync(),
                TotalBarangHampirHabis = await _context.Barangs
                    .Where(b => b.Stok <= 5)
                    .CountAsync()
            };

            // ============================
            // 2. Barang stok rendah
            // ============================
            var barangStokRendah = await _context.Barangs
                .Where(b => b.Stok <= 5)
                .Select(b => new BarangStokRendahDto
                {
                    BarangId = b.Id,
                    NamaBarang = b.NamaBarang,
                    Stok = b.Stok,
                    Satuan = b.Satuan
                })
                .ToListAsync();

            // ============================
            // 3. Permintaan terbaru (limit 5)
            // ============================
            var permintaanTerbaru = await _context.PermintaanBarangs
                .Include(p => p.User)
                .Include(p => p.Barang)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new PermintaanTerbaruDto
                {
                    PermintaanId = p.Id,
                    NamaPemohon = p.User.Nama,
                    NamaBarang = p.Barang.NamaBarang,
                    Jumlah = p.JumlahDiminta,
                    Status = p.Status.ToString(),
                    TanggalPermintaan = p.CreatedAt
                })
                .ToListAsync();


            // ============================
            // FINAL RESULT
            // ============================
            return new AdminDashboardDto
            {
                Summary = summary,
                BarangStokRendah = barangStokRendah,
                PermintaanTerbaru = permintaanTerbaru
            };
        }
    }
}
