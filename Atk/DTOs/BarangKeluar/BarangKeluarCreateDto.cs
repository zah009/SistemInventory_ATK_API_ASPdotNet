using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.BarangKeluar
{
    public class BarangKeluarCreateDto
    {
        [Required]
        public int PermintaanId { get; set; }

        [Required]  
        public int BarangId { get; set; }  

        [Required]  
        [Range(1, int.MaxValue)]  
        public int JumlahKeluar { get; set; }  

        [Required]  
        public DateTime TanggalKeluar { get; set; }  

        [MaxLength(500)]  
        public string? Keterangan { get; set; }  
    }
}