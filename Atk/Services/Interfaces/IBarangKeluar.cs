using Atk.Models;

public interface IBarangKeluar
{
    Task<List<BarangKeluar>> GetAllAsync();
    Task<BarangKeluar?> GetByIdAsync(int id);
    Task<List<BarangKeluar>> GetByPermintaanAsync(int permintaanId);
    Task<List<BarangKeluar>> GetByBarangAsync(int barangId);
}
