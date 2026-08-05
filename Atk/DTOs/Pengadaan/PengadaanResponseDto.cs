using System;
using SistemInventoriAtk.Models;

namespace Atk.DTOs.Pengadaan
{
    public class PengadaanResponseDto
    {
        public int Id { get; set; }
        public int BarangId { get; set; }
        public string? NamaBarang { get; set; }
        public string? Satuan { get; set; }
        public int JumlahDiajukan { get; set; }
        public int JumlahDiterima { get; set; }
        public StatusPengadaan Status { get; set; }
        public DateTime TanggalPengajuan { get; set; }
        public string? Keterangan { get; set; }
        public int SupplierId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}