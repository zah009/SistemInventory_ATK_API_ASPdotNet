using Microsoft.EntityFrameworkCore;
using Atk.Models;                    // Supplier, Barang, BarangMasuk, BarangKeluar, PermintaanBarang
using SistemInventoriAtk.Models;     // PengadaanBarang, Payment, User

namespace Atk.Data
{
public class ApplicationDbContext : DbContext
{
public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
: base(options)
{
}

    // DbSet / tabel  
    public DbSet<Supplier> Suppliers { get; set; }  
    public DbSet<PengadaanBarang> PengadaanBarangs { get; set; }  
    public DbSet<Payment> Payments { get; set; }  
    public DbSet<Barang> Barangs { get; set; }  
    public DbSet<BarangMasuk> BarangMasuks { get; set; }  
    public DbSet<BarangKeluar> BarangKeluars { get; set; }  
    public DbSet<PermintaanBarang> PermintaanBarangs { get; set; }  
    public DbSet<User> Users { get; set; }  
    public DbSet<Divisi> Divisis { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        base.OnModelCreating(modelBuilder);  

        // =============================  
        // BarangKeluar → PermintaanBarang  
        modelBuilder.Entity<BarangKeluar>()  
            .HasOne(bk => bk.PermintaanBarang)  
            .WithMany(p => p.BarangKeluars)  
            .HasForeignKey(bk => bk.PermintaanId)  
            .OnDelete(DeleteBehavior.Restrict); // NO CASCADE  

        // BarangKeluar → Barang  
        modelBuilder.Entity<BarangKeluar>()  
            .HasOne(bk => bk.Barang)  
            .WithMany(b => b.BarangKeluars)  
            .HasForeignKey(bk => bk.BarangId)  
            .OnDelete(DeleteBehavior.Restrict); // NO CASCADE  

        // PermintaanBarang → User  
        modelBuilder.Entity<PermintaanBarang>()  
            .HasOne(p => p.User)  
            .WithMany() // User bisa punya banyak permintaan, optional  
            .HasForeignKey(p => p.UserId)  
            .OnDelete(DeleteBehavior.Restrict);  

        // PermintaanBarang → Barang  
        modelBuilder.Entity<PermintaanBarang>()  
            .HasOne(p => p.Barang)  
            .WithMany(b => b.PermintaanBarangs)  
            .HasForeignKey(p => p.BarangId)  
            .OnDelete(DeleteBehavior.Restrict);  

        // BarangMasuk → Barang (Restrict, BUKAN Cascade!)
        // BarangMasuk adalah riwayat/audit trail pembelian (harga, tanggal,
        // supplier). Kalau di-cascade, admin bisa menghapus master Barang
        // dan diam-diam menghapus seluruh riwayat pembelian barang itu juga.
        // Dengan Restrict, Barang yang masih punya riwayat BarangMasuk tidak
        // bisa dihapus sampai riwayatnya ditangani secara eksplisit dulu.
        modelBuilder.Entity<BarangMasuk>()  
            .HasOne(bm => bm.Barang)  
            .WithMany(b => b.BarangMasuks)  
            .HasForeignKey(bm => bm.BarangId)  
            .OnDelete(DeleteBehavior.Restrict);  

        // BarangMasuk → Supplier (optional, no cascade)  
        modelBuilder.Entity<BarangMasuk>()  
            .HasOne(bm => bm.Supplier)  
            .WithMany(s => s.BarangMasuks)  
            .HasForeignKey(bm => bm.SupplierId)  
            .OnDelete(DeleteBehavior.Restrict);  

        // =============================
        // PengadaanBarang → Barang (wajib, no cascade supaya Barang
        // master tidak ikut hilang kalau riwayat pengadaan dihapus)
        modelBuilder.Entity<PengadaanBarang>()
            .HasOne(pg => pg.Barang)
            .WithMany()
            .HasForeignKey(pg => pg.BarangId)
            .OnDelete(DeleteBehavior.Restrict);

        // PengadaanBarang → Supplier
        modelBuilder.Entity<PengadaanBarang>()
            .HasOne(pg => pg.Supplier)
            .WithMany(s => s.PengadaanBarang)
            .HasForeignKey(pg => pg.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // BarangMasuk → PengadaanBarang (opsional, no cascade — hapus
        // pengadaan tidak boleh otomatis menghapus riwayat barang masuk)
        modelBuilder.Entity<BarangMasuk>()
            .HasOne(bm => bm.PengadaanBarang)
            .WithMany()
            .HasForeignKey(bm => bm.PengadaanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment → PengadaanBarang (opsional, no cascade)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.PengadaanBarang)
            .WithMany()
            .HasForeignKey(p => p.PengadaanId)
            .OnDelete(DeleteBehavior.Restrict);
    }  
}  

}