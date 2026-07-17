using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.Models
{
    public class Barang
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string KodeBarang { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string NamaBarang { get; set; } = string.Empty;

        [Required]
        public int Stok { get; set; } = 0;

        [Required, MaxLength(50)]
        public string Satuan { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        // Navigasi
         public ICollection<BarangMasuk> BarangMasuks { get; set; } = new List<BarangMasuk>();
    public ICollection<BarangKeluar> BarangKeluars { get; set; } = new List<BarangKeluar>();
    public ICollection<PermintaanBarang> PermintaanBarangs { get; set; } = new List<PermintaanBarang>();


    }
}