using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.Pengadaan
{
    public class PengadaanUpdateDto
    {
        public string? NamaBarang {get; set;}
        public string? Satuan {get; set;}
        public int JumlahDiajukan {get; set;}
        public DateTime TanggalPengajuan {get; set;}
        public string? Keterangan {get; set;}
        public int SupplierId {get; set;}
    }
}