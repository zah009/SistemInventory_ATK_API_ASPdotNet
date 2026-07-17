using Atk.Models;
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
        Task AddOrUpdatePaymentFromBarangMasukAsync(int value, DateTime tanggalMasuk, decimal subtotal);
        Task<bool> ReducePaymentFromBarangMasukAsync(int supplierId, DateTime tanggal, decimal totalHarga);
    }
}
