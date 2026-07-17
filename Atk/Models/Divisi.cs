using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.Models
{
    public class Divisi
    {
        [Key]
        public int Id { get; set; }
        public string? Nama { get; set; }

        // Relasi ke User
        public ICollection<User> Users { get; set; } 
    }
}

