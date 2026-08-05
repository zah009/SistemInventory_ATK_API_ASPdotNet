using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Implementations
{
    public class PaymentService : IPayment
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            payment.CreatedAt = DateTime.Now;
            payment.UpdatedAt = DateTime.Now;

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.Include(p => p.Supplier).ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> UpdateStatusAsync(int id, PaymentStatus status)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return false;

            payment.Status = status;
            payment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadBuktiTransferAsync(int id, string filePath)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return false;

            payment.BuktiTransferFilePath = filePath;
            payment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Cari payment "target" yang jadi acuan untuk otomatis menambah/mengurangi
        // TotalHarga dari BarangMasuk.
        //  - Kalau pengadaanId ada: satu PengadaanBarang selalu dipetakan ke
        //    SATU Payment (tidak akan nyampur dengan pengadaan lain).
        //  - Kalau pengadaanId null: fallback ke perilaku lama (grouping per
        //    supplier + tanggal), tapi HANYA menyasar payment yang juga tidak
        //    terikat pengadaan (PengadaanId == null), supaya tidak diam-diam
        //    ikut menambah payment milik pengadaan lain.
        private async Task<Payment?> FindTargetPaymentAsync(int supplierId, int? pengadaanId, DateTime tanggal)
        {
            if (pengadaanId.HasValue)
            {
                return await _context.Payments
                    .FirstOrDefaultAsync(p => p.PengadaanId == pengadaanId.Value);
            }

            return await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.SupplierId == supplierId &&
                    p.PengadaanId == null &&
                    p.TanggalBayar.Date == tanggal.Date);
        }

        private async Task AddOrUpdatePaymentInternalAsync(int supplierId, int? pengadaanId, DateTime tanggal, decimal totalHarga)
        {
            var payment = await FindTargetPaymentAsync(supplierId, pengadaanId, tanggal);

            if (payment != null)
            {
                payment.TotalHarga += totalHarga;
                payment.UpdatedAt = DateTime.Now;
            }
            else
            {
                payment = new Payment
                {
                    SupplierId = supplierId,
                    PengadaanId = pengadaanId,
                    TotalHarga = totalHarga,
                    TanggalBayar = tanggal.Date,
                    Keterangan = pengadaanId.HasValue
                        ? $"Otomatis dari BarangMasuk (Pengadaan #{pengadaanId.Value})"
                        : "Otomatis dari BarangMasuk",
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _context.Payments.AddAsync(payment);
            }

            await _context.SaveChangesAsync();
        }

        // Kurangi total harga (misal ketika BarangMasuk diupdate/dihapus)
        public async Task<bool> ReducePaymentFromBarangMasukAsync(int supplierId, int? pengadaanId, DateTime tanggal, decimal totalHarga)
        {
            var payment = await FindTargetPaymentAsync(supplierId, pengadaanId, tanggal);
            if (payment == null) return false;

            payment.TotalHarga -= totalHarga;
            if (payment.TotalHarga < 0) payment.TotalHarga = 0;

            payment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        Task IPayment.AddOrUpdatePaymentFromBarangMasukAsync(int supplierId, int? pengadaanId, DateTime tanggal, decimal totalHarga)
        {
            return AddOrUpdatePaymentInternalAsync(supplierId, pengadaanId, tanggal, totalHarga);
        }
    }
}