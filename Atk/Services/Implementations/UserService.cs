using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.Users;
using Atk.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atk.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateDivisiUserAsync(UserCreateDivisiDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ArgumentException("Username harus diisi.");

            // Cegah duplikat Username. Belum ada unique index di level DB
            // (lihat catatan di ApplicationDbContext), jadi ini masih rawan
            // TOCTOU kalau dua request submit bersamaan persis di waktu yang
            // sama — perbaikan definitif tetap harus HasIndex(...).IsUnique()
            // + migration. Guard di service layer ini menutup celah untuk
            // kasus normal (bukan race condition murni).
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                throw new InvalidOperationException($"Username '{dto.Username}' sudah digunakan.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                Password = hashedPassword,
                Nama = dto.Nama,
                NamaDivisi = dto.NamaDivisi,
                Role = UserRole.Divisi
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(int id, UserUpdateDivisiDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            // Cegah rename ke Username yang sudah dipakai user LAIN (kalau
            // usernya tidak ganti username sama sekali, tidak masalah).
            if (!string.Equals(user.Username, dto.Username, StringComparison.Ordinal))
            {
                var dipakaiUserLain = await _context.Users
                    .AnyAsync(u => u.Id != id && u.Username == dto.Username);
                if (dipakaiUserLain)
                    throw new InvalidOperationException($"Username '{dto.Username}' sudah digunakan user lain.");
            }

            user.Username = dto.Username;
            user.Nama = dto.Nama;
            user.NamaDivisi = dto.NamaDivisi;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
    
}