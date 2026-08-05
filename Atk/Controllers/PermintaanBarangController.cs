using System;
using System.Collections.Generic;
using Atk.DTOs;
using Atk.DTOs.PermintaanBarang;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermintaanBarangController : ControllerBase
    {
        private readonly IPermintaanBarang _service;

        public PermintaanBarangController(IPermintaanBarang service)
        {
            _service = service;
        }

        // ====================================================
        // 1. DIVISI membuat permintaan
        // ====================================================
        [Authorize(Roles = "Divisi")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PermintaanBarangCreateDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var permintaan = await _service.CreateAsync(dto, userId);

            return Ok(new
            {
                message = "Berhasil membuat permintaan barang",
                statusCode = 200,
                data = permintaan
            });
        }

        // ====================================================
        // 2. List permintaan
        //    - Admin = semua permintaan
        //    - Divisi = permintaan miliknya sendiri
        // ====================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] StatusPermintaan? status = null)
        {
            string role = User.FindFirstValue(ClaimTypes.Role);
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<PermintaanBarang> list;

            if (role == "Divisi")
            {
                list = (await _service.GetAllAsync(status))
                        .FindAll(p => p.UserId == userId);
            }
            else // Admin
            {
                list = await _service.GetAllAsync(status);
            }

            return Ok(new
            {
                message = "Berhasil mengambil data permintaan barang",
                statusCode = 200,
                data = list
            });
        }

        // ====================================================
        // 3. Admin Approve / Reject permintaan
        // ====================================================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id,
            [FromBody] PermintaanBarangUpdateStatusDto dto)
        {
            try
            {
                var success = await _service.UpdateStatusAsync(id, dto);

                if (!success)
                    return NotFound(new
                    {
                        message = "Permintaan barang tidak ditemukan",
                        statusCode = 404,
                        data = (object)null
                    });

                var msg = dto.Status == StatusPermintaan.Disetujui
                    ? "disetujui"
                    : "ditolak";

                return Ok(new
                {
                    message = $"Permintaan barang berhasil {msg}",
                    statusCode = 200,
                    data = (object)null
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, statusCode = 404, data = (object)null });
            }
            catch (InvalidOperationException ex)
            {
                // Termasuk "Stok barang tidak mencukupi" — bisa juga hasil dari
                // dua approval bersamaan yang kalah rebutan lock stok.
                return BadRequest(new { message = ex.Message, statusCode = 400, data = (object)null });
            }
        }
    }
}