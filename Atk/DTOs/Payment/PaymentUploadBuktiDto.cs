using System.ComponentModel.DataAnnotations;

namespace Atk.DTOs.Payment
{
    public class PaymentUploadBuktiDto
    {
        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; } = string.Empty;
    }
}
