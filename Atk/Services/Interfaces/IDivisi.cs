using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Divisi;
using Atk.Models;

namespace Atk.Services.Interfaces
{
    public interface IDivisi
    {
    Task<List<Divisi>> GetAllAsync();
    Task<Divisi> CreateAsync(DivisiCreateDto dto);
    Task<Divisi> UpdateAsync(int id, DivisiCreateDto dto);
    Task<bool> DeleteAsync(int id);
    }
}