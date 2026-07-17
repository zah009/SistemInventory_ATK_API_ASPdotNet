using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Models;

namespace Atk.DTOs.PermintaanBarang
{
    public class PermintaanBarangResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserNama { get; set; } = string.Empty;
        public int BarangId { get; set; }
        public string BarangNama { get; set; } = string.Empty;
        public int JumlahDiminta { get; set; }
        public StatusPermintaan Status { get; set; }
        public string? Alasan { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}