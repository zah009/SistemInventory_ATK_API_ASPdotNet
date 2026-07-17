using System;
using System.Threading.Tasks;
using Atk.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboard _service;

        public AdminDashboardController(IAdminDashboard service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _service.GetDashboardAsync();

            var response = new
            {
                message = "Berhasil mengambil data dashboard",
                statusCode = StatusCodes.Status200OK,
                data = data
            };

            return Ok(response);
        }
    }
}
