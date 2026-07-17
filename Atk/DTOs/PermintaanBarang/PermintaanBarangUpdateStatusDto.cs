using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Models;

namespace Atk.DTOs.PermintaanBarang
{
    public class PermintaanBarangUpdateStatusDto
    {
        public StatusPermintaan Status { get; set; } // Disetujui / Ditolak
    public string? Keterangan { get; set; } // Optional
    }
}