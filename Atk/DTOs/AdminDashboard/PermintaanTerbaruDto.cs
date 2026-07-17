using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.AdminDashboard
{
    public class PermintaanTerbaruDto
    {
        public int PermintaanId { get; set; }
        public string NamaPemohon { get; set; } = "";
        public string NamaBarang { get; set; } = "";
        public int Jumlah { get; set; }
        public string Status { get; set; } = "";
        public DateTime TanggalPermintaan { get; set; }
    }
}