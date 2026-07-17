using System.ComponentModel.DataAnnotations;

namespace Atk.DTOs.Users
{
    public class UserUpdateDivisiDto
    {
        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(100)]
        public string Nama { get; set; }

        [Required, MaxLength(150)]
        public string NamaDivisi { get; set; }

        public string? Password { get; set; } // Optional, hanya diupdate kalau diisi
    }
}
