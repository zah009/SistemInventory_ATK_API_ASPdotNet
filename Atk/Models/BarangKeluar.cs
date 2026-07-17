using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Atk.Models
{
    public class BarangKeluar
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PermintaanId { get; set; }

        [Required]
        public int BarangId { get; set; }

        [Required]
        public int JumlahKeluar { get; set; }

        [Required]
        public DateTime TanggalKeluar { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Properti navigasi harus sesuai dengan yang digunakan di OnModelCreating
        [ForeignKey("PermintaanId")]
        public PermintaanBarang? PermintaanBarang { get; set; }


        [ForeignKey("BarangId")]
        public Barang Barang { get; set; } = null!;
    }
}
