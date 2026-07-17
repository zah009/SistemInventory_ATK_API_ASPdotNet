using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Supplier;

namespace Atk.Services.Implementations
{
    public interface ISupplierServices
    {
        Task<IEnumerable<SupplierResponseDto>> GetAllAsync();
        Task<SupplierResponseDto?> GetByIdAsync(int id);
        Task<SupplierResponseDto> CreateAsync(SupplierCreateDto dto);
        Task<SupplierResponseDto?> UpdateAsync(int id, SupplierUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByName(string namaSupplier);
    }
}