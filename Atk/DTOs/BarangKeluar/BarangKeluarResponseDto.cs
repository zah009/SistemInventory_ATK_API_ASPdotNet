using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.BarangKeluar
{
    public class BarangKeluarResponseDto
    {
        public int Id { get; set; } 
        public int PermintaanId { get; set; } 
        public int BarangId { get; set; } 
        public int JumlahKeluar { get; set; } 
        public DateTime TanggalKeluar { get; set; } 
        public string? Keterangan { get; set; } 
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; }
    }
}