using Atk.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atk.Services.Interfaces
{
    public interface IPayment
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment> CreateAsync(Payment payment); // buat payment baru
        Task<bool> UpdateStatusAsync(int id, PaymentStatus status); // update status
        Task<bool> UploadBuktiTransferAsync(int id, string filePath); // upload bukti tf
        Task<bool> DeleteAsync(int id);

        // pengadaanId: kalau diisi, payment dicari/dibuat berdasarkan
        // PengadaanId (satu pengadaan = satu payment) supaya tidak nyampur
        // dengan pengadaan lain ke supplier yang sama pada tanggal yang sama.
        // Kalau null (barang masuk di luar alur pengadaan), fallback ke
        // perilaku lama: grouping per supplier + tanggal.
        Task AddOrUpdatePaymentFromBarangMasukAsync(int supplierId, int? pengadaanId, DateTime tanggalMasuk, decimal subtotal);
        Task<bool> ReducePaymentFromBarangMasukAsync(int supplierId, int? pengadaanId, DateTime tanggal, decimal totalHarga);
    }
}