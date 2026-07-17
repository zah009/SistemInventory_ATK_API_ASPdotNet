using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.AdminDashboard
{
    public class BarangStokRendahDto
    {
        public int BarangId { get; set; }
        public string NamaBarang { get; set; } = "";
        public int Stok { get; set; }
        public string Satuan { get; set; } = "";
    }
}