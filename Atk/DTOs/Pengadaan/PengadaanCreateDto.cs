using System;
using System.ComponentModel.DataAnnotations;

namespace Atk.DTOs.Pengadaan
{
    public class PengadaanCreateDto
    {
        // Wajib: barang yang diajukan harus sudah ada di master Barang,
        // supaya BarangMasuk yang memenuhinya nanti bisa auto-linked.
        [Required]
        public int BarangId { get; set; }

        public string? Satuan { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "JumlahDiajukan harus lebih dari 0.")]
        public int JumlahDiajukan { get; set; }

        [Required]
        public DateTime TanggalPengajuan { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        [Required]
        public int SupplierId { get; set; }
    }
}