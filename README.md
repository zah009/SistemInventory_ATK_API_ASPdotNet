# 🏢 Sistem Informasi Pengadaan & Pengolahan ATK

Aplikasi berbasis **ASP.NET Core Web API** untuk mengelola pengadaan ATK, barang masuk, barang keluar, permintaan divisi, dan pembayaran.

---

## 📌 Fitur Utama

### 1. **Manajemen Barang**

* Tambah, edit, hapus barang
* Satuan barang
* Stok otomatis bertambah/berkurang

### 2. **Pengadaan Barang**

* Admin membuat pengadaan berdasarkan supplier
* Input jumlah, harga satuan, dan total
* Upload invoice
* Status pengadaan (Dibuat → Menunggu Barang → Selesai)

### 3. **Barang Masuk**

* Konfirmasi barang yang sudah diterima
* Upload bukti penerimaan
* **Stok barang bertambah otomatis**
* Pengadaan otomatis berubah menjadi *Selesai*

### 4. **Permintaan Barang (User)**

* User membuat permintaan barang
* Admin menyetujui / menolak
* Barang keluar dicatat otomatis
* **Stok berkurang otomatis**

### 5. **Payment (Opsional)**

* Buat dan kelola payment
* Upload bukti transfer
* Update status pembayaran (Pending → Lunas)

### 6. **Autentikasi**

* JWT Authentication
* Role-based Authorization (Admin & User)

---

## 📘 Alur Sistem (Ringkas)

```
ADMIN → Pengadaan Barang
            ↓
 (Opsional) Payment
            ↓
ADMIN → Barang Masuk → Stok Bertambah
            ↓
USER  → Permintaan Barang
            ↓
ADMIN → Approve → Barang Keluar → Stok Berkurang
```

---

## 🛠️ Teknologi yang Digunakan

* **ASP.NET Core 8 Web API**
* **Entity Framework Core**
* **PostgreSQL**
* **JWT Authentication**
* **Automapper**
* **Swagger / Swashbuckle** (OpenAPI)
* **xUnit** (Unit Testing)

---

## 🚀 Cara Menjalankan Project

### 1. Clone repository

```bash
git clone <repository-url>
cd <nama-folder>
```

### 2. Konfigurasi `appsettings.json`

Sesuaikan:

* Connection string PostgreSQL
* JWT key & issuer

### 3. Jalankan Migrasi

```bash
dotnet ef database update
```

### 4. Jalankan Aplikasi

```bash
dotnet run
```

API akan berjalan di:

```
http://localhost:5034
```

Swagger UI:

```
http://localhost:5034/swagger
```

---

## 📄 OpenAPI / Swagger

Aplikasi ini menggunakan **Swashbuckle.AspNetCore** untuk generate dokumentasi API.

Untuk export file JSON:

```bash
dotnet swagger tofile --output openapi.json bin/Debug/net8.0/Atk.dll v1
```

---

## 🧪 Unit Test

Project sudah mendukung **xUnit**.
Beberapa pengujian yang tersedia:

* AuthController Tests
* BarangService Tests
* PaymentService Tests
* AdminDashboard Tests

Menjalankan test:

```bash
dotnet test
```

---

## 📁 Struktur Folder

```
├── Controllers/
├── Data/
├── Models/
├── Services/
│   ├── Implementations/
│   └── Interfaces/
├── DTOs/
├── Middleware/
├── Tests/
└── README.md
```

---

## 👥 Role & Permissions

| Role      | Akses                                                                              |
| --------- | ---------------------------------------------------------------------------------- |
| **Admin** | Pengadaan barang, barang masuk, barang keluar, daftar supplier, verifikasi payment |
| **User**  | Membuat permintaan barang, melihat status permintaan                               |

---

## 🤝 Kontribusi

Pull Request dipersilakan. Untuk perubahan besar, harap buka issue terlebih dahulu.

---

## 📜 Lisensi

Proyek ini menggunakan lisensi bebas sesuai kebutuhan.

---

## 📧 Kontak

Jika ada pertanyaan atau ingin diskusi mengenai pengembangan lebih lanjut:
**Said Hamzah** – Developer Backend
