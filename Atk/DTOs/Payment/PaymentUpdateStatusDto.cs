using System.ComponentModel.DataAnnotations;
using Atk.Models;

namespace Atk.DTOs.Payment
{
    public class PaymentUpdateStatusDto
    {
        [Required]
        public PaymentStatus Status { get; set; } // harus diisi
    }
}
