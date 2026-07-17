using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.Models;
using Atk.DTOs.Users;

namespace Atk.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateDivisiUserAsync(UserCreateDivisiDto dto);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UserUpdateDivisiDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
