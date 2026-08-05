using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.DTOs.Pengadaan;

namespace Atk.Services.Interfaces
{
    public interface IPengadaan
    {
        public Task<IEnumerable<PengadaanResponseDto>> GetAllAsync();
        public Task<PengadaanResponseDto?> GetByIdAsync(int id);
        public Task<PengadaanResponseDto> CreateAsync(PengadaanCreateDto dto);
        public Task<List<PengadaanResponseDto>> CreateBulkAsync(IEnumerable<PengadaanCreateDto> dtos);
        public Task<PengadaanResponseDto> UpdateAsync(int id, PengadaanUpdateDto dto);
        public Task<bool> DeleteAsync(int id);

        // Menggantikan ExistsByName lama (yang memblokir nama barang yang
        // sama untuk SELAMANYA — padahal re-order itu wajar dalam bisnis
        // pengadaan). Sekarang cek: apakah barang ini masih punya pengadaan
        // yang berstatus "Diajukan" (belum Selesai/Dibatalkan) ke supplier
        // yang sama, supaya tidak dobel-order tanpa sengaja.
        Task<bool> HasOpenPengadaanAsync(int barangId, int supplierId);
    }
}