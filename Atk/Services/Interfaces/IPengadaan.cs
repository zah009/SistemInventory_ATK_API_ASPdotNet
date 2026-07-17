using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Pengadaan;
using SistemInventoriAtk.Models;

namespace Atk.Services.Interfaces
{
    public interface IPengadaan
    {
        public Task<IEnumerable<PengadaanResponseDto>> GetAllAsync();
        public Task<PengadaanResponseDto?> GetByIdAsync(int id);
        public Task<PengadaanResponseDto> CreateAsync(PengadaanCreateDto dto);
        public Task<PengadaanResponseDto> UpdateAsync(int id, PengadaanUpdateDto dto);
        public Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByName(string namaBarang);
    }
}