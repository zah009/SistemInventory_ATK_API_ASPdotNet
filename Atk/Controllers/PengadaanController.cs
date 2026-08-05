using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Pengadaan;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.RateLimiting;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class PengadaanController : ControllerBase
    {
        private readonly IPengadaan _service;
        private static DateTime _lastRequestTime = DateTime.MinValue;

        public PengadaanController(IPengadaan service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new
            {
                message = "Berhasil mengambil data pengadaan",
                statusCode = 200,
                data
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pengadaan = await _service.GetByIdAsync(id);
            if (pengadaan == null)
            {
                return NotFound(new
                {
                    message = "Id tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });
            }

            return Ok(new
            {
                message = "Berhasil mengambil data pengadaan",
                statusCode = 200,
                data = pengadaan
            });
        }

        [EnableRateLimiting("pengadaan_bulk_limit")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<PengadaanCreateDto> dtos)
        {
            var now = DateTime.UtcNow;

            if ((now - _lastRequestTime).TotalMilliseconds < 500) // <0.5 detik?
            {
                return StatusCode(429, new
                {
                    message = "Terlalu cepat, coba lagi",
                    statusCode = 429,
                    data = (object)null
                });
            }

            _lastRequestTime = now;

            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Data Pengadaan tidak boleh kosong",
                    statusCode = 400,
                    data = (object)null
                });
            }

            var result = new List<PengadaanResponseDto>();

            try
            {
                foreach (var dto in dtos)
                {
                    if (await _service.HasOpenPengadaanAsync(dto.BarangId, dto.SupplierId))
                    {
                        return BadRequest(new
                        {
                            message = $"Barang id {dto.BarangId} sudah punya pengadaan aktif (belum Selesai/Dibatalkan) ke supplier id {dto.SupplierId}",
                            statusCode = 400,
                            data = (object)null
                        });
                    }

                    var newPengadaan = await _service.CreateAsync(dto);
                    result.Add(newPengadaan);
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, statusCode = 404, data = (object)null });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, statusCode = 400, data = (object)null });
            }

            return Ok(new
            {
                message = "Berhasil menambahkan pengadaan secara bulk",
                statusCode = 200,
                data = result
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PengadaanUpdateDto dto)
        {
            try
            {
                var upt = await _service.UpdateAsync(id, dto);
                return Ok(new
                {
                    message = "Berhasil mengupdate pengadaan",
                    statusCode = 200,
                    data = upt
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, statusCode = 404, data = (object)null });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, statusCode = 400, data = (object)null });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var del = await _service.DeleteAsync(id);
                if (!del)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan atau gagal dihapus",
                        statusCode = 404,
                        data = (object)null
                    });
                }

                return Ok(new
                {
                    message = "Berhasil menghapus pengadaan",
                    statusCode = 200,
                    data = (object)null
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, statusCode = 400, data = (object)null });
            }
        }
    }
}