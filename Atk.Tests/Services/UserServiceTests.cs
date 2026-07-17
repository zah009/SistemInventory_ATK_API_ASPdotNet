using System;
using System.Linq;
using System.Threading.Tasks;
using Atk.Data;
using Atk.DTOs.Users;
using Atk.Models;
using Atk.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Atk.Tests.Services
{
    public class UserServiceTests
    {
        private async Task<ApplicationDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            // Seed data awal jika perlu
            context.Users.Add(new User
            {
                Id = 1,
                Username = "user1",
                Password = BCrypt.Net.BCrypt.HashPassword("password1"),
                Nama = "User Satu",
                NamaDivisi = "IT",
                Role = UserRole.Divisi,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
            await context.SaveChangesAsync();

            return context;
        }

        private UserService GetService(ApplicationDbContext context)
        {
            return new UserService(context);
        }

        [Fact]
        public async Task CreateDivisiUserAsync_CreatesUserSuccessfully()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var dto = new UserCreateDivisiDto
            {
                Username = "user2",
                Password = "secret",
                Nama = "User Dua",
                NamaDivisi = "HR"
            };

            var result = await service.CreateDivisiUserAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("user2", result.Username);
            Assert.Equal("User Dua", result.Nama);
            Assert.Equal("HR", result.NamaDivisi);
            Assert.Equal(UserRole.Divisi, result.Role);
            Assert.True(BCrypt.Net.BCrypt.Verify("secret", result.Password));
            Assert.Equal(2, await context.Users.CountAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var users = await service.GetAllAsync();

            Assert.Single(users);
            Assert.Equal("user1", users.First().Username);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenExists()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var user = await service.GetByIdAsync(1);

            Assert.NotNull(user);
            Assert.Equal("user1", user.Username);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var user = await service.GetByIdAsync(999);

            Assert.Null(user);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesUserSuccessfully()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var dto = new UserUpdateDivisiDto
            {
                Username = "user1-updated",
                Nama = "User Satu Updated",
                NamaDivisi = "Finance",
                Password = "newpass"
            };

            var updated = await service.UpdateAsync(1, dto);

            Assert.True(updated);

            var check = await context.Users.FindAsync(1);
            Assert.Equal("user1-updated", check.Username);
            Assert.Equal("User Satu Updated", check.Nama);
            Assert.Equal("Finance", check.NamaDivisi);
            Assert.True(BCrypt.Net.BCrypt.Verify("newpass", check.Password));
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenUserNotFound()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var dto = new UserUpdateDivisiDto
            {
                Username = "notfound",
                Nama = "Not Found",
                NamaDivisi = "None",
                Password = "pass"
            };

            var updated = await service.UpdateAsync(999, dto);

            Assert.False(updated);
        }

        [Fact]
        public async Task DeleteAsync_RemovesUserSuccessfully()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var deleted = await service.DeleteAsync(1);

            Assert.True(deleted);
            Assert.Empty(context.Users);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenUserNotFound()
        {
            var context = await GetInMemoryDbContext();
            var service = GetService(context);

            var deleted = await service.DeleteAsync(999);

            Assert.False(deleted);
        }
    }
}
