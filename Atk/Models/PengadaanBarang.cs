using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Atk.Models;

namespace SistemInventoriAtk.Models
{
    public enum StatusPengadaan
    {
        Diajukan,
        Disetujui,
        Selesai,
        Dibatalkan
    }

    public class PengadaanBarang
    {
        [Key]
        public int Id { get; set; }

        // FK ke master Barang. Wajib diisi supaya BarangMasuk & Payment
        // yang memenuhi pengadaan ini bisa dikaitkan secara otomatis.
        [Required]
        public int BarangId { get; set; }

        [ForeignKey("BarangId")]
        public Barang? Barang { get; set; }

        // Snapshot nama barang saat pengajuan (audit trail).
        // Bukan lagi sumber kebenaran utama — itu ada di Barang via BarangId.
        [Required]
        [MaxLength(255)]
        public string NamaBarang { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Satuan { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Jumlah Diajukan harus lebih dari 0.")]
        public int JumlahDiajukan { get; set; }

        // Akumulasi jumlah yang sudah masuk lewat BarangMasuk yang
        // mereferensikan pengadaan ini. Dipakai untuk menentukan
        // status Selesai dan mencegah over-fulfillment.
        public int JumlahDiterima { get; set; } = 0;

        [Required]
        public StatusPengadaan Status { get; set; } = StatusPengadaan.Diajukan;

        [Required]
        [DataType(DataType.Date)]
        public DateTime TanggalPengajuan { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        [Required]
        public int SupplierId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }
    }
}