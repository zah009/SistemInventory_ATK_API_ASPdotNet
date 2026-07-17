using System.ComponentModel.DataAnnotations;

namespace Atk.DTOs.Supplier
{
    public class SupplierResponseDto
    {
        public int Id { get; set; }
        [Required]
        public string? NamaSupplier { get; set; }
        public string? Alamat { get; set; }
        public string? Telepon { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
