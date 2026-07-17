using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atk.DTOs.BarangMasuk;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class BarangMasukController : ControllerBase
    {
        private readonly IBarangMasuk _service;

        public BarangMasukController(IBarangMasuk service)
        {
            _service = service;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new
            {
                message = "Berhasil mengambil data barang masuk",
                statusCode = 200,
                data
            });
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);

            if (data == null)
                return NotFound(new
                {
                    message = "Data Barang Masuk tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });

            return Ok(new
            {
                message = "Berhasil mengambil data barang masuk",
                statusCode = 200,
                data
            });
        }

        // CREATE (single insert)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BarangMasukCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    message = "Data tidak valid",
                    statusCode = 400,
                    data = ModelState
                });

            try
            {
                var created = await _service.CreateAsync(dto);
                return Ok(new
                {
                    message = "Berhasil menambahkan barang masuk",
                    statusCode = 200,
                    data = created
                });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    statusCode = 400,
                    data = (object)null
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    statusCode = 400,
                    data = (object)null
                });
            }
        }

        // BULK CREATE
        [EnableRateLimiting("pengadaan_bulk_limit")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<BarangMasukCreateDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest(new
                {
                    message = "Data tidak boleh kosong",
                    statusCode = 400,
                    data = (object)null
                });

            try
            {
                var created = await _service.CreateBulkAsync(dtos);
                return Ok(new
                {
                    message = "Berhasil menambahkan barang masuk secara bulk",
                    statusCode = 200,
                    data = created
                });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    statusCode = 400,
                    data = (object)null
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    statusCode = 400,
                    data = (object)null
                });
            }
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BarangMasukUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    message = "Data tidak valid",
                    statusCode = 400,
                    data = ModelState
                });

            var result = await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound(new
                {
                    message = "Data tidak ditemukan atau gagal diperbarui",
                    statusCode = 404,
                    data = (object)null
                });

            return Ok(new
            {
                message = "Update berhasil",
                statusCode = 200,
                data = (object)null
            });
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new
                {
                    message = "Data tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });

            return Ok(new
            {
                message = "Data berhasil dihapus",
                statusCode = 200,
                data = (object)null
            });
        }
    }
}