using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.AdminDashboard
{
    public class DashboardSummaryDto
    {
        public int TotalBarang { get; set; }
        public int TotalPermintaanPending { get; set; }
        public int TotalPermintaanDisetujui { get; set; }
        public int TotalPermintaanDitolak { get; set; }
        public int TotalBarangHampirHabis { get; set; }
    }
}