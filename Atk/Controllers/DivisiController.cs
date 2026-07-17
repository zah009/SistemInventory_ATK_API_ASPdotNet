using Atk.DTOs.Divisi;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class DivisiController : ControllerBase
    {
        private readonly IDivisi _service;
        
        public DivisiController(IDivisi service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new
            {
                message = "Berhasil mengambil data divisi",
                statusCode = 200,
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(DivisiCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return Ok(new
            {
                message = "Berhasil menambahkan divisi",
                statusCode = 200,
                data = created
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DivisiCreateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            
            if (result == null)
                return NotFound(new
                {
                    message = "Divisi tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });
            
            return Ok(new
            {
                message = "Berhasil mengupdate divisi",
                statusCode = 200,
                data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            
            if (!success)
                return NotFound(new
                {
                    message = "Divisi tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });
            
            return Ok(new
            {
                message = "Berhasil menghapus divisi",
                statusCode = 200,
                data = (object)null
            });
        }
    }
}