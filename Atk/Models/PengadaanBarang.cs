using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Atk.Models;

namespace SistemInventoriAtk.Models
{
    public class PengadaanBarang
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string NamaBarang { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Satuan { get; set; }

        [Required]
        // Pastikan nilai lebih dari 0 di sisi aplikasi
        [Range(1, int.MaxValue, ErrorMessage = "Jumlah Diajukan harus lebih dari 0.")]
        public int JumlahDiajukan { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TanggalPengajuan { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        // Foreign Key ke Supplier
        [Required]
        public int SupplierId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        // PERBAIKAN KRITIS: Jadikan Navigation Property 'Supplier' nullable.
        // Ini mengatasi error validasi 400 saat client hanya mengirim SupplierId.
        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; } 
    }
}