using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Users;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class UserDivisiController : ControllerBase
    {
        private readonly IUserService _service;
        
        public UserDivisiController(IUserService service)
        {
            _service = service;
        } 

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new
            {
                message = "Berhasil mengambil data user divisi",
                statusCode = 200,
                data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new
                {
                    message = "User tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });
            }

            return Ok(new
            {
                message = "Berhasil mengambil data user divisi",
                statusCode = 200,
                data = user
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDivisiDto dto)
        {
            var newUser = await _service.CreateDivisiUserAsync(dto);
            return Ok(new
            {
                message = "Berhasil menambahkan user divisi",
                statusCode = 200,
                data = newUser
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDivisiDto dto)
        {
            var update = await _service.UpdateAsync(id, dto);
            if (!update)
                return NotFound(new
                {
                    message = "User tidak ditemukan atau gagal diupdate",
                    statusCode = 404,
                    data = (object)null
                });
            
            return Ok(new
            {
                message = "Berhasil mengupdate user divisi",
                statusCode = 200,
                data = (object)null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var del = await _service.DeleteAsync(id);
            if (!del)
                return NotFound(new
                {
                    message = "User tidak ditemukan atau gagal dihapus",
                    statusCode = 404,
                    data = (object)null
                });

            return Ok(new
            {
                message = "Berhasil menghapus user divisi",
                statusCode = 200,
                data = (object)null
            });
        }
    }
}