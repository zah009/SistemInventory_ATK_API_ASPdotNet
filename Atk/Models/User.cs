using System.ComponentModel.DataAnnotations;

public enum UserRole
{
    Admin,
    Divisi
}

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; }

    [Required, MaxLength(255)]
    public string Password { get; set; } // hashed nanti

    [Required, MaxLength(100)]
    public string Nama { get; set; }

    // Tambahan ini untuk nama divisi user divisi
    [MaxLength(150)]
    public string? NamaDivisi { get; set; }  // Admin = null, Divisi = isi

    [Required]
    public UserRole Role { get; set; } = UserRole.Divisi;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
