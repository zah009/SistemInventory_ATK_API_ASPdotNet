using System.ComponentModel.DataAnnotations;
using SistemInventoriAtk.Models;
using System.Collections.Generic;
namespace Atk.Models
{
    public class Supplier
    {
        [Key]
        public int Id {get; set;}
        [Required]
        [MaxLength (255)]
        public string namaSupplier {get; set;} = string.Empty;
        [MaxLength(255)]
        public string Alamat {get; set;} = string.Empty;
        [Required]
        [MaxLength(16)]
        public string Telepon { get; set; } = string.Empty;
        [EmailAddress]
        [MaxLength(255)]
        public string? Email {get; set;}  = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<PengadaanBarang> PengadaanBarang { get; set; } = new List<PengadaanBarang>();
        public ICollection<BarangMasuk> BarangMasuks { get; set; } = new List<BarangMasuk>();

    }
}