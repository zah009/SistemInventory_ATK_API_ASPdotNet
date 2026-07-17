using System.ComponentModel.DataAnnotations;

namespace Atk.DTOs.Users
{
    public class UserCreateDivisiDto
    {
        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }  // nanti akan di-hash

        [Required, MaxLength(100)]
        public string Nama { get; set; }

        [Required, MaxLength(150)]
        public string NamaDivisi { get; set; }
    }
}
