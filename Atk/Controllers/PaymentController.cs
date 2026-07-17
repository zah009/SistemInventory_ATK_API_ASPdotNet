using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atk.DTOs.Payment;
using Atk.Models;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class PaymentController : ControllerBase
    {
        private readonly IPayment _service;
        
        public PaymentController(IPayment service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new
            {
                message = "Berhasil mengambil data payment",
                statusCode = 200,
                data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var getId = await _service.GetByIdAsync(id);
            if (getId == null)
            {
                return NotFound(new
                {
                    message = "Data Payment Tidak Ditemukan",
                    statusCode = 404,
                    data = (object)null
                });
            }

            return Ok(new
            {
                message = "Berhasil mengambil data payment",
                statusCode = 200,
                data = getId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto dto)
        {
            var payment = new Payment
            {
                SupplierId = dto.SupplierId, 
                TotalHarga = dto.TotalHarga, 
                TanggalBayar = dto.TanggalBayar, 
                Keterangan = dto.Keterangan, 
                Status = PaymentStatus.Pending
            };

            var result = await _service.CreateAsync(payment);
            return Ok(new
            {
                message = "Berhasil menambahkan payment",
                statusCode = 200,
                data = result
            });
        }

        [HttpPut("{id}/status")] 
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] PaymentUpdateStatusDto dto) 
        { 
            var success = await _service.UpdateStatusAsync(id, dto.Status); 
            if (!success) 
                return NotFound(new
                {
                    message = "Data Payment Tidak Ditemukan",
                    statusCode = 404,
                    data = (object)null
                }); 

            return Ok(new
            {
                message = "Status Payment Berhasil Diupdate",
                statusCode = 200,
                data = (object)null
            }); 
        }

        [HttpPost("{id}/upload-bukti")] 
        public async Task<IActionResult> UploadBuktiTransfer(int id, [FromBody] PaymentUploadBuktiDto dto) 
        { 
            var success = await _service.UploadBuktiTransferAsync(id, dto.FilePath); 
            if (!success)
                return NotFound(new
                {
                    message = "Data Payment Tidak Ditemukan",
                    statusCode = 404,
                    data = (object)null
                }); 
            
            return Ok(new 
            { 
                message = "Bukti Transfer Berhasil Diunggah",
                statusCode = 200,
                data = (object)null
            }); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        { 
            var success = await _service.DeleteAsync(id); 
            if (!success)
                return NotFound(new
                {
                    message = "Data Payment Tidak Ditemukan",
                    statusCode = 404,
                    data = (object)null
                }); 
            
            return Ok(new
            {
                message = "Payment Berhasil Dihapus",
                statusCode = 200,
                data = (object)null
            }); 
        }
    }
}