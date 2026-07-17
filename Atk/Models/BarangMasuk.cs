using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.Models
{
    public class BarangMasuk
    {
         [Key]
        public int Id { get; set; }

        [Required]
        public int BarangId { get; set; }

        public int? SupplierId { get; set; }

        [Required]
        public int JumlahMasuk { get; set; }

        [Required, Column(TypeName = "decimal(15,2)")]
        public decimal HargaSatuan { get; set; }

        [Required]
        public DateTime TanggalMasuk { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigasi
        [ForeignKey("BarangId")]
        public Barang Barang { get; set; } = null!;

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }
    
    }
}