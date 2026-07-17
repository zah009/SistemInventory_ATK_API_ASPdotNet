using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Atk.Models
{
    public enum PaymentStatus
    {
        Pending,
        Lunas,
        Ditolak
    }

    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        [Column(TypeName = "decimal(15, 2)")]
        public decimal TotalHarga { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TanggalBayar { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? BuktiTransferFilePath { get; set; } // path file upload

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigasi ke Supplier
        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; } = null!;
    }
}
