using System.Threading.Tasks;
using Atk.DTOs.AdminDashboard;

namespace Atk.Services.Interfaces
{
    public interface IAdminDashboard
    {
        Task<AdminDashboardDto> GetDashboardAsync();
    }
}
