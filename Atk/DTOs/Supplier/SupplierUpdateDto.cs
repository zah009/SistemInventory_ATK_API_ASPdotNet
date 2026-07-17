using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Atk.DTOs.Supplier
{
    public class SupplierUpdateDto
    {
        public string? NamaSupplier { get; set; }
        public string? Alamat { get; set; }
        public string? Telepon { get; set; }
        public string? Email { get; set; }
    }
}
