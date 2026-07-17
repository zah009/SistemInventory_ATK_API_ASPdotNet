using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.Barang
{
    public class BarangUpdateDto
    {
        public string KodeBarang { get; set; } = string.Empty; 
        public string NamaBarang { get; set; } = string.Empty; 
        public int Stok { get; set; } 
        public string Satuan { get; set; } = string.Empty;
    }
}