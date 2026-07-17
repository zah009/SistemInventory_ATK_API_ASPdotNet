using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.DTOs.BarangMasuk;
using Atk.Models;

namespace Atk.Services.Interfaces
{
    public interface IBarangMasuk
    {
        Task<BarangMasukResponseDto> CreateAsync(BarangMasukCreateDto dto);
        Task<IEnumerable<BarangMasukResponseDto>> CreateBulkAsync(IEnumerable<BarangMasukCreateDto> dtos);
        Task<IEnumerable<BarangMasukResponseDto>> GetAllAsync();
        Task<BarangMasukResponseDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, BarangMasukUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<int> GetTotalBarangMasukByDateAsync(DateTime date);
    }
}
