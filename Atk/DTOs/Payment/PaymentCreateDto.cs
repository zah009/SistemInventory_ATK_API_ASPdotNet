using System;
using System.ComponentModel.DataAnnotations;

namespace Atk.DTOs.Payment
{
    public class PaymentCreateDto
    {
        [Required]
        public int SupplierId { get; set; }

        // Opsional: kaitkan payment manual ini ke pengadaan tertentu.
        public int? PengadaanId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "TotalHarga harus lebih dari 0.")]
        public decimal TotalHarga { get; set; }

        [Required]
        public DateTime TanggalBayar { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        [MaxLength(255)]
        public string? BuktiTransfer { get; set; } // bisa path file
    }
}