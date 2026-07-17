using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs;
using Atk.DTOs.Barang;
using Atk.Models;

namespace Atk.Services.Interfaces
{
    public interface IBarang
    {
        public Task<IEnumerable<BarangResponseDto>> GetAllAsync();
        public Task<BarangResponseDto?> GetByIdAsync(int id);
        public Task<BarangResponseDto> CreateAsync(BarangCreateDto dto);
        public Task<BarangResponseDto> UpdateAsync(int id, BarangUpdateDto dto);
        public Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByName(string namaBarang);
    }
}