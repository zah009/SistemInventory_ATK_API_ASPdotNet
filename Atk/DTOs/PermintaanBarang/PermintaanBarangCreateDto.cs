using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs
{
    public class PermintaanBarangCreateDto
    {
        public int BarangId { get; set; }
        public int JumlahDiminta { get; set; }
        public string? Alasan { get; set; }
    }
}