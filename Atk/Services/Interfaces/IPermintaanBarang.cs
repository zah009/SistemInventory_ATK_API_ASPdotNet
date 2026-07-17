using Atk.DTOs;
using Atk.DTOs.Payment;
using Atk.DTOs.PermintaanBarang;
using Atk.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atk.Services.Interfaces
{
    public interface IPermintaanBarang
    {
        // Divisi membuat permintaan barang
        Task<PermintaanBarang> CreateAsync(PermintaanBarangCreateDto dto, int userId);

        // Admin atau Divisi bisa melihat daftar permintaan (opsional filter status)
        Task<List<PermintaanBarang>> GetAllAsync(StatusPermintaan? status = null);

        // Admin approve / reject permintaan
        Task<bool> UpdateStatusAsync(int permintaanId, PermintaanBarangUpdateStatusDto dto);

        // (Optional) Ambil permintaan berdasarkan Id
        Task<PermintaanBarang?> GetByIdAsync(int id);
    }

}
