using System;

namespace Atk.DTOs.Users
{
    public class UserDivisiResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Nama { get; set; }
        public string? NamaDivisi { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}