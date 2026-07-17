using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.Models
{
   public enum StatusPermintaan
    {
        Pending,
        Disetujui,
        Ditolak
    }

    public class PermintaanBarang
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Required]
    public int BarangId { get; set; }

    [Required]
    public Barang Barang { get; set; } = null!;

    [Required]
    public int JumlahDiminta { get; set; }

    [Required]
    public StatusPermintaan Status { get; set; } = StatusPermintaan.Pending;

    [MaxLength(500)]
    public string? Alasan { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ICollection<BarangKeluar>? BarangKeluars { get; set; }
}

}