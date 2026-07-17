using Atk.Data;
using Atk.DTOs.Supplier;
using Atk.Models;
using Atk.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Interfaces
{
   public class SupplierService : ISupplierServices
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupplierResponseDto> CreateAsync(SupplierCreateDto dto)
        {
            if (dto == null) throw new ArgumentException(nameof(dto));

            // validasi
            if (string.IsNullOrWhiteSpace(dto.NamaSupplier))
                throw new  ArgumentException("Nama Supplier Harus di isi.", nameof(dto.NamaSupplier));
            
            var now = DateTime.Now;

            var supplier = new Supplier
            {
                 // mapping DTO -> Entity (perhatikan nama properti di model)
                namaSupplier = dto.NamaSupplier,
                Alamat = dto.Alamat ?? string.Empty,
                Telepon = dto.Telepon,
                Email = dto.Email,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            // Mapping 

            var response =  new SupplierResponseDto
            {
                Id = supplier.Id,
                NamaSupplier = supplier.namaSupplier,
                Alamat = supplier.Alamat,
                Telepon = supplier.Telepon,
                Email = supplier.Email,
                CreatedAt = supplier.CreatedAt
            };
            return response;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return false;
            }

            // hapus data
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByName(string namaSupplier)
        {
            return await _context.Suppliers.AnyAsync(s => s.namaSupplier == namaSupplier);
        }

        public async Task<IEnumerable<SupplierResponseDto>> GetAllAsync()
        {
            var supplier = await _context.Suppliers .AsNoTracking().ToListAsync();

            // mapping ke dto
            var result = supplier.Select(s => new SupplierResponseDto
            {
                Id = s.Id,
                NamaSupplier = s.namaSupplier,
                Alamat = s.Alamat,
                Telepon = s.Telepon,
                Email = s.Email,
                CreatedAt = s.CreatedAt,
            });

            return result;
        }

        public async Task<SupplierResponseDto?> GetByIdAsync(int id)
        {
            // 1. Ambil data supplier (AsNoTracking untuk query saja, tanpa tracking)
            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            // 2. Jika tidak ditemukan, return null
            if (supplier == null)
                return null;

            // 3. Mapping Entity -> Response DTO
            var response = new SupplierResponseDto
            {
                Id = supplier.Id,
                NamaSupplier = supplier.namaSupplier,
                Alamat = supplier.Alamat,
                Telepon = supplier.Telepon,
                Email = supplier.Email,
                CreatedAt = supplier.CreatedAt
            };

            // 4. Return DTO
            return response;
        }


        public async Task<SupplierResponseDto?> UpdateAsync(int id, SupplierUpdateDto dto)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null )
            {
                return null;
            }

             // 2. Update nilai (mapping DTO -> Entity)
            supplier.namaSupplier = dto.NamaSupplier;
            supplier.Alamat = dto.Alamat ?? supplier.Alamat;
            supplier.Telepon = dto.Telepon;
            supplier.Email = dto.Email ?? supplier.Email;
            supplier.UpdatedAt = DateTime.Now;

            // simpan perubahan
            await _context.SaveChangesAsync();

            var response = new SupplierResponseDto
            {
                Id = supplier.Id,
                NamaSupplier = supplier.namaSupplier,
                Alamat = supplier.Alamat,
                Telepon = supplier.Telepon,
                Email = supplier.Email,
                CreatedAt = supplier.CreatedAt
            };
            return response;
        }
    }
}