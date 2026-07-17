using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.AdminDashboard
{
    public class AdminDashboardDto
    {
        public DashboardSummaryDto Summary { get; set; } = new DashboardSummaryDto();

        public List<BarangStokRendahDto> BarangStokRendah { get; set; }
            = new List<BarangStokRendahDto>();

        public List<PermintaanTerbaruDto> PermintaanTerbaru { get; set; }
            = new List<PermintaanTerbaruDto>();
    }
}