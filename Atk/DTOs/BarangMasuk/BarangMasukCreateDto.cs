using System;
using System.ComponentModel.DataAnnotations;

namespace Atk.DTOs.BarangMasuk
{
    public class BarangMasukCreateDto
    {
        [Required]
        public int BarangId { get; set; }

        public int? SupplierId { get; set; } // nullable, sesuai service

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "JumlahMasuk harus lebih dari 0.")]
        public int JumlahMasuk { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "HargaSatuan harus lebih dari 0.")]
        public decimal HargaSatuan { get; set; }

        [Required]
        public DateTime TanggalMasuk { get; set; }
    }
}
