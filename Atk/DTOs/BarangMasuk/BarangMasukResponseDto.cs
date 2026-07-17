using System;

namespace Atk.DTOs.BarangMasuk
{
    public class BarangMasukResponseDto
    {
        public int Id { get; set; }
        public int BarangId { get; set; }
        public int? SupplierId { get; set; }
        public int JumlahMasuk { get; set; }
        public decimal HargaSatuan { get; set; }
        public DateTime TanggalMasuk { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
