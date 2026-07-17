using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Supplier;
using Atk.Services.Implementations;
using Atk.Services.Interfaces;
using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierServices _service;
        private static DateTime _lastRequestTime = DateTime.MinValue;

        public SupplierController(ISupplierServices services)
        {
            _service = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new
            {
                message = "Berhasil mengambil data supplier",
                statusCode = 200,
                data
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _service.GetByIdAsync(id);

            if (supplier == null)
            {
                return NotFound(new
                {
                    message = "Supplier tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });
            }

            return Ok(new
            {
                message = "Berhasil mengambil data supplier",
                statusCode = 200,
                data = supplier
            });
        }

        [EnableRateLimiting("supplier_bulk_limit")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<SupplierCreateDto> dtos)
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
                    message = "Data supplier tidak boleh kosong",
                    statusCode = 400,
                    data = (object)null
                });
            }

            var result = new List<SupplierResponseDto>();

            foreach (var dto in dtos)
            {
                // validasi duplikat
                if (await _service.ExistsByName(dto.NamaSupplier))
                {
                    return BadRequest(new
                    {
                        message = $"{dto.NamaSupplier} sudah ada",
                        statusCode = 400,
                        data = (object)null
                    });
                }

                var newSupplier = await _service.CreateAsync(dto);
                result.Add(newSupplier);
            }

            return Ok(new
            {
                message = "Berhasil menambahkan supplier secara bulk",
                statusCode = 200,
                data = result
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupplierUpdateDto dto)
        {
            var update = await _service.UpdateAsync(id, dto);

            if (update == null)
            {
                return NotFound(new
                {
                    message = "Supplier tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });
            }

            return Ok(new
            {
                message = "Berhasil mengupdate supplier",
                statusCode = 200,
                data = update
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var delete = await _service.DeleteAsync(id);

            if (!delete)
            {
                return NotFound(new
                {
                    message = "Supplier tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });
            }

            return Ok(new
            {
                message = "Berhasil menghapus supplier",
                statusCode = 200,
                data = (object)null
            });
        }
    }
}