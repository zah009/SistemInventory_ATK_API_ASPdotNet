using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class BarangKeluarController : ControllerBase
    {
        private readonly IBarangKeluar _service;
        
        public BarangKeluarController(IBarangKeluar service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new
            {
                message = "Berhasil mengambil data barang keluar",
                statusCode = 200,
                data = list
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var getId = await _service.GetByIdAsync(id);
            if (getId == null)
                return NotFound(new
                {
                    message = "Barang id tidak ditemukan",
                    statusCode = 404,
                    data = (object)null
                });

            return Ok(new
            {
                message = "Berhasil mengambil data barang keluar",
                statusCode = 200,
                data = getId
            });
        }

        [HttpGet("by-permintaan/{permintaanId}")]
        public async Task<IActionResult> GetByPermintaan(int permintaanId)
        {
            var list = await _service.GetByPermintaanAsync(permintaanId);
            return Ok(new
            {
                message = "Berhasil mengambil data barang keluar berdasarkan permintaan",
                statusCode = 200,
                data = list
            });
        }

        [HttpGet("by-barang/{barangId}")]
        public async Task<IActionResult> GetByBarang(int barangId)
        {
            var list = await _service.GetByBarangAsync(barangId);
            return Ok(new
            {
                message = "Berhasil mengambil data barang keluar berdasarkan barang",
                statusCode = 200,
                data = list
            });
        }
    }
}