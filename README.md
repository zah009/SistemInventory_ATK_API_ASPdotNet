# Sistem Inventaris ATK — REST API (ASP.NET Core)

REST API untuk sistem pengadaan dan pengelolaan inventaris ATK: manajemen barang & stok, barang masuk, permintaan barang antar-divisi dengan alur persetujuan, pengadaan ke supplier, dan pembayaran. Dibangun dengan **ASP.NET Core Web API**, didesain sebagai backend terpisah (headless) yang dikonsumsi oleh frontend React: [**atk-inventory-react**](https://github.com/zah009/atk-inventory-react).

---

## Daftar Isi

1. [Fitur Utama](#1-fitur-utama)
2. [Teknologi yang Digunakan](#2-teknologi-yang-digunakan)
3. [Arsitektur & Cara Kerja](#3-arsitektur--cara-kerja)
4. [Autentikasi & Otorisasi](#4-autentikasi--otorisasi)
5. [Rate Limiting](#5-rate-limiting)
6. [Struktur Data](#6-struktur-data)
7. [Daftar Endpoint API](#7-daftar-endpoint-api)
8. [Instalasi & Menjalankan](#8-instalasi--menjalankan)
9. [Migrasi Database](#9-migrasi-database)
10. [Swagger / OpenAPI](#10-swagger--openapi)
11. [Unit Testing](#11-unit-testing)
12. [Struktur Folder](#12-struktur-folder)
13. [Catatan Pengembangan](#13-catatan-pengembangan)

---

## 1. Fitur Utama

- **Manajemen Barang** — CRUD data barang, input massal (bulk), stok terlacak otomatis
- **Pengadaan Barang** — admin mengajukan rencana pengadaan per Barang & Supplier, dengan status lifecycle (`Diajukan` → `Disetujui` → `Selesai`/`Dibatalkan`) dan pelacakan `JumlahDiterima` vs `JumlahDiajukan`
- **Barang Masuk** — pencatatan penerimaan barang dari supplier (single & bulk); **bisa dikaitkan ke sebuah Pengadaan** — jika dikaitkan, sistem otomatis memvalidasi kecocokan Barang/Supplier, menambah `JumlahDiterima` pada Pengadaan terkait (dan menolak jika melebihi `JumlahDiajukan`), memperbarui status Pengadaan, serta otomatis membuat/mengupdate record Payment ke supplier. Stok Barang bertambah otomatis di setiap kasus.
- **Permintaan Barang (Divisi)** — user divisi mengajukan permintaan barang; admin approve/reject; permintaan yang disetujui otomatis tercatat sebagai Barang Keluar dan stok berkurang
- **Payment** — pencatatan pembayaran ke supplier, bisa dibuat manual atau otomatis lewat linking Barang Masuk↔Pengadaan (lihat di atas), upload bukti transfer, update status (`Pending` → `Lunas`/`Ditolak`)
- **Manajemen Supplier** — CRUD data supplier
- **Manajemen Divisi & User Divisi** — admin mengelola daftar divisi dan akun user per divisi
- **Admin Dashboard** — endpoint ringkasan data untuk kebutuhan dashboard admin
- **Autentikasi JWT** berbasis role (`Admin` & `Divisi`)
- **Rate limiting** pada endpoint login (anti brute-force) dan endpoint input massal (bulk) untuk mencegah penyalahgunaan/spam request

---

## 2. Teknologi yang Digunakan

| Komponen | Teknologi |
|---|---|
| Framework | **ASP.NET Core Web API — .NET 10** |
| ORM | Entity Framework Core 10 |
| Database | **Microsoft SQL Server** (via `Microsoft.EntityFrameworkCore.SqlServer`) |
| Autentikasi | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), token juga bisa dibaca dari cookie `AuthToken` |
| Hash Password | BCrypt (`BCrypt.Net-Next`) |
| Dokumentasi API | Swagger / Swashbuckle.AspNetCore |
| Rate Limiting | `Microsoft.AspNetCore.RateLimiting` (bawaan .NET, Fixed Window Limiter) |
| Testing | xUnit + Moq + EF Core InMemory |
| Container | Docker & Docker Compose (image SQL Server 2022 + API) |

> **Catatan:** README versi sebelumnya menyebut PostgreSQL, .NET 8, dan AutoMapper — setelah dicek ulang ke `Atk.csproj` dan `docker-compose.yml`, project ini sebenarnya memakai **SQL Server** dan **.NET 10**, serta **tidak** memakai AutoMapper (mapping dilakukan manual di Service layer).

---

## 3. Arsitektur & Cara Kerja

Pola **Controller → Service (Interface + Implementation) → DbContext (EF Core) → SQL Server**, dengan DTO sebagai kontrak request/response terpisah dari Model/Entity.

```
Client (React/Postman/Swagger)
   |
   v
Controller (validasi request, [Authorize])
   |
   v
Service Interface  --(Dependency Injection)-->  Service Implementation
   |
   v
ApplicationDbContext (EF Core)
   |
   v
SQL Server
```

Setiap modul punya interface (`Services/Interfaces/IXxx.cs`) dan implementasi (`Services/Implementations/XxxService.cs`), didaftarkan sebagai *scoped service* di `Program.cs`. Ini memudahkan unit testing lewat mocking (`Moq`) tanpa perlu database asli.

### Alur Bisnis Utama

```
ADMIN -> Pengadaan Barang (bulk, per Barang+Supplier)  status: Diajukan
              |
ADMIN -> Barang Masuk (opsional dikaitkan ke Pengadaan via PengadaanId)
              |
              +-> Stok Barang bertambah
              +-> Jika terkait Pengadaan:
              |      - validasi Barang & Supplier harus cocok dengan Pengadaan
              |      - JumlahDiterima Pengadaan bertambah (ditolak jika > JumlahDiajukan)
              |      - status Pengadaan otomatis -> Disetujui (sebagian) / Selesai (penuh)
              |      - Payment ke Supplier otomatis dibuat/di-update (status Pending)
              |
ADMIN -> (opsional) update status Payment -> Lunas / Ditolak, upload bukti transfer

DIVISI -> Permintaan Barang (status awal: Pending)
              |
ADMIN  -> Approve  -> status "Disetujui" -> Barang Keluar tercatat -> Stok berkurang
       -> Reject   -> status "Ditolak"   -> stok tidak berubah
```

> Barang Masuk **tidak wajib** dikaitkan ke Pengadaan — kalau `PengadaanId` dikosongkan, alurnya tetap seperti sebelumnya (Barang Masuk mandiri, Payment dibuat manual lewat `POST /api/Payment`). Edit/hapus Barang Masuk yang terkait Pengadaan juga otomatis membalikkan efeknya (`JumlahDiterima` dikurangi, status Pengadaan disesuaikan ulang).

---

## 4. Autentikasi & Otorisasi

- Login via `POST /api/Auth/login` dengan `username` & `password` → password diverifikasi dengan BCrypt, response berisi **JWT token**.
- Token dapat dikirim lewat:
  - Cookie `AuthToken` (dibaca otomatis di `Program.cs`), **atau**
  - Header `Authorization: Bearer {token}` (fallback, juga dipakai untuk testing lewat Swagger)
- Role tersimpan dalam token (`Admin` atau `Divisi`), dipakai oleh atribut `[Authorize(Roles = "...")]` di setiap controller untuk membatasi akses per endpoint.
- Endpoint `GET /api/PermintaanBarang` bersifat khusus: role `Divisi` hanya melihat permintaan miliknya sendiri, role `Admin` melihat seluruh permintaan — logika ini difilter di controller berdasarkan klaim (`ClaimTypes.NameIdentifier`, `ClaimTypes.Role`) di dalam token.

---

## 5. Rate Limiting

Dikonfigurasi lewat `AddRateLimiter` di `Program.cs`:

| Policy | Batas | Cakupan |
|---|---|---|
| `login_limit` | 5 percobaan / menit per IP | `POST /api/Auth/login` (anti brute-force) |
| `supplier_bulk_limit` | 3 request / 10 detik | Endpoint bulk Supplier |
| `pengadaan_bulk_limit` | 5 request / menit per IP | Endpoint bulk Pengadaan Barang |
| `barang_bulk_limit` | 5 request / 30 detik per IP | Endpoint bulk Barang |

> **Perlu dicek:** endpoint bulk `BarangMasuk` (`POST /api/BarangMasuk/bulk`) di kode saat ini memakai policy `pengadaan_bulk_limit`, bukan policy tersendiri. Ini bisa jadi memang disengaja (menyamakan batas dengan Pengadaan), tapi juga bisa sisa copy-paste — cek `BarangMasukController.cs` baris ~111 kalau ini bukan yang dimaksud, dan buat policy `barang_masuk_bulk_limit` terpisah kalau perlu.

---

## 6. Struktur Data

| Entity | Fungsi | Relasi Utama |
|---|---|---|
| `User` | Akun login, role `Admin`/`Divisi`, terhubung ke nama divisi | — |
| `Divisi` | Master data divisi | hasMany `User` |
| `Supplier` | Data pemasok | hasMany `Payment`, `PengadaanBarang`, `BarangMasuk` |
| `Barang` | Master barang + stok | hasMany `BarangMasuk`, `BarangKeluar`, `PermintaanBarang` |
| `BarangMasuk` | Transaksi penerimaan barang dari supplier; opsional terkait ke satu Pengadaan | belongsTo `Barang`, `Supplier` (opsional), `PengadaanBarang` (opsional) |
| `PermintaanBarang` | Permintaan barang dari divisi, status `Pending`/`Disetujui`/`Ditolak` (enum) | belongsTo `User`, `Barang`; hasMany `BarangKeluar` |
| `BarangKeluar` | Transaksi keluar hasil approval | belongsTo `PermintaanBarang`, `Barang` |
| `PengadaanBarang` | Pengajuan pengadaan barang tertentu ke supplier tertentu; status `Diajukan`/`Disetujui`/`Selesai`/`Dibatalkan` (enum); melacak `JumlahDiajukan` vs `JumlahDiterima` | belongsTo `Barang`, `Supplier`; hasMany `BarangMasuk`, `Payment` (via `PengadaanId`) |
| `Payment` | Pembayaran ke supplier, status `Pending`/`Lunas`/`Ditolak` (enum); opsional terkait ke satu Pengadaan | belongsTo `Supplier`, `PengadaanBarang` (opsional) |

---

## 7. Daftar Endpoint API

Semua route berawalan `/api/{controller}`. Kolom **Akses** menunjukkan role yang diizinkan (`[Authorize(Roles = ...)]`); tanpa keterangan berarti endpoint publik (tidak butuh token), kecuali disebutkan lain.

### Auth (`/api/Auth`) — publik
| Method | Endpoint | Keterangan |
|---|---|---|
| POST | `/login` | Login, mengembalikan JWT |
| POST | `/logout` | Instruksi ke client untuk menghapus token |

### Barang (`/api/Barang`)
| Method | Endpoint | Akses |
|---|---|---|
| GET | `/` | Semua role terautentikasi |
| GET | `/{id}` | Semua role terautentikasi |
| POST | `/bulk` | Admin |
| PUT | `/{id}` | Admin |
| DELETE | `/{id}` | Admin |

### BarangMasuk (`/api/BarangMasuk`) — Admin
| Method | Endpoint |
|---|---|
| GET | `/` |
| GET | `/{id}` |
| POST | `/` |
| POST | `/bulk` |
| PUT | `/{id}` |
| DELETE | `/{id}` |

### BarangKeluar (`/api/BarangKeluar`) — Admin
| Method | Endpoint |
|---|---|
| GET | `/` |
| GET | `/{id}` |
| GET | `/by-permintaan/{permintaanId}` |
| GET | `/by-barang/{barangId}` |

### PermintaanBarang (`/api/PermintaanBarang`)
| Method | Endpoint | Akses |
|---|---|---|
| POST | `/` | Divisi (membuat permintaan) |
| GET | `/` | Semua role terautentikasi (hasil difilter sesuai role) |
| PUT | `/{id}/status` | Admin (approve/reject) |

### Pengadaan (`/api/Pengadaan`) — Admin
| Method | Endpoint |
|---|---|
| GET | `/` |
| GET | `/{id}` |
| POST | `/bulk` |
| PUT | `/{id}` |
| DELETE | `/{id}` |

### Payment (`/api/Payment`) — Admin
| Method | Endpoint |
|---|---|
| GET | `/` |
| GET | `/{id}` |
| POST | `/` |
| PUT | `/{id}/status` |
| POST | `/{id}/upload-bukti` |
| DELETE | `/{id}` |

### Supplier (`/api/Supplier`) — Admin
| Method | Endpoint |
|---|---|
| GET | `/` |
| GET | `/{id}` |
| POST | `/bulk` |
| PUT | `/{id}` |
| DELETE | `/{id}` |

### Divisi (`/api/Divisi`) — Admin
| Method | Endpoint |
|---|---|
| GET | `/` |
| POST | `/` |
| PUT | `/{id}` |
| DELETE | `/{id}` |

### UserDivisi (`/api/UserDivisi`) — Admin
| Method | Endpoint |
|---|---|
| GET | `/` |
| GET | `/{id}` |
| POST | `/` |
| PUT | `/{id}` |
| DELETE | `/{id}` |

### AdminDashboard (`/api/AdminDashboard`) — Admin
| Method | Endpoint |
|---|---|
| GET | `/` |

---

## 8. Instalasi & Menjalankan

### Prasyarat

- **.NET SDK 10** — https://dotnet.microsoft.com/download
- **Docker & Docker Compose** (opsi termudah, tidak perlu install SQL Server manual), **atau** SQL Server lokal/Express jika menjalankan tanpa Docker

### Opsi A — Menjalankan dengan Docker Compose (direkomendasikan)

Project ini sudah menyediakan `docker-compose.yml` yang otomatis menyiapkan container SQL Server + API sekaligus. `docker-compose.yml` membaca dua env var (`MSSQL_SA_PASSWORD`, `JWT_KEY`) dari file `.env` di root project — **file ini wajib dibuat manual dulu**, `docker compose up --build` akan gagal (atau jalan dengan credential kosong) tanpanya.

```bash
git clone https://github.com/zah009/SistemInventory_ATK_API_ASPdotNet.git
cd SistemInventory_ATK_API_ASPdotNet

# Buat file .env di root project (sejajar dengan docker-compose.yml), isi:
#   MSSQL_SA_PASSWORD=<password kuat: huruf besar+kecil, angka, simbol, min 8 karakter>
#   JWT_KEY=<random string, minimal 32 karakter>
# JANGAN commit file .env ini ke git (sudah di-ignore lewat .gitignore).

docker compose up --build
```

- API berjalan di `http://localhost:8080`
- SQL Server berjalan di `localhost:1433` (user `sa`, password sesuai `MSSQL_SA_PASSWORD` yang kamu isi di `.env`)

> **Catatan:** repo ini belum menyediakan file `.env.example` sebagai referensi format — sebaiknya ditambahkan supaya kontributor lain tidak perlu menebak-nebak isi `.env` yang dibutuhkan.

> Setelah container `db` menyala, jalankan migrasi (lihat [bagian 9](#9-migrasi-database)) agar tabel-tabel terbentuk di database `AtkDb`.

### Opsi B — Menjalankan Manual (tanpa Docker)

```bash
git clone https://github.com/zah009/SistemInventory_ATK_API_ASPdotNet.git
cd SistemInventory_ATK_API_ASPdotNet/Atk
cp appsettings.example.json appsettings.json
```

Edit `appsettings.json`, sesuaikan:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AtkDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "<ganti-dengan-secret-key-minimal-32-karakter>",
    "Issuer": "AtkApi",
    "Audience": "AtkUsers"
  }
}
```

Jalankan migrasi lalu jalankan API:

```bash
dotnet ef database update
dotnet run
```

API berjalan di `http://localhost:5034` (port sesuai `launchSettings.json`, cek `Properties/launchSettings.json` bila berbeda).

---

## 9. Migrasi Database

Riwayat migrasi (`Atk/Migrations/`) menunjukkan skema berkembang bertahap:

```
InitialCreate → AddUserDivisi → AddDivisi → AddKey
→ AddBuktiTransferAndStatusToPayment → NewBmPb → User → SyncModelChanges
→ AddPengadaanLinking → SyncUserDivisiRelation
```

`AddPengadaanLinking` adalah migration paling signifikan sejauh ini: menambahkan `BarangId`, `Status`, `JumlahDiterima` ke `PengadaanBarangs`, serta kolom `PengadaanId` (nullable) ke `Payments` dan `BarangMasuks` untuk mendukung linking otomatis antar ketiga modul (lihat [bagian 3](#3-arsitektur--cara-kerja)).

Menjalankan migrasi ke database aktif:

```bash
dotnet ef database update
```

Membuat migrasi baru setelah mengubah Model:

```bash
dotnet ef migrations add NamaMigrasiBaru
```

---

## 10. Swagger / OpenAPI

Swagger UI aktif otomatis pada environment Development:

```
http://localhost:5034/swagger
```

(atau `http://localhost:8080/swagger` jika menjalankan via Docker Compose dengan `ASPNETCORE_ENVIRONMENT=Development`)

Swagger sudah dikonfigurasi mendukung input JWT lewat tombol **Authorize** (`Bearer {token}`) untuk mencoba endpoint yang butuh autentikasi.

Export dokumentasi ke file JSON:

```bash
dotnet swagger tofile --output openapi.json bin/Debug/net10.0/Atk.dll v1
```

---

## 11. Unit Testing

Project `Atk.Tests` memakai **xUnit + Moq + EF Core InMemory**, dengan cakupan test untuk seluruh controller dan service:

- **Controller Tests:** Auth, Barang, BarangKeluar, BarangMasuk, Divisi, Payment, Pengadaan, PermintaanBarang, Supplier, UserDivisi, AdminDashboard
- **Service Tests:** modul yang sama di sisi business logic

Menjalankan seluruh test:

```bash
dotnet test
```

---

## 12. Struktur Folder

```
SistemInventory_ATK_API_ASPdotNet/
├── Atk/                        # Project utama (Web API)
│   ├── Controllers/            # Endpoint API per modul
│   ├── Data/                   # ApplicationDbContext (EF Core)
│   ├── DTOs/                   # Kontrak request/response per modul
│   ├── Migrations/             # Riwayat migrasi EF Core
│   ├── Models/                 # Entity/domain model
│   ├── Services/
│   │   ├── Interfaces/         # Kontrak service per modul
│   │   └── Implementations/    # Business logic
│   ├── Program.cs              # Konfigurasi startup (DI, JWT, CORS, Rate Limiting, Swagger)
│   ├── Dockerfile
│   └── appsettings.example.json
├── Atk.Tests/                  # Unit test (xUnit)
│   ├── Controllers/
│   └── Services/
├── docker-compose.yml          # SQL Server + API sekaligus
└── Atk.sln
```

---

## 13. Catatan Pengembangan

- API ini **headless** — tidak ada tampilan/view, didesain untuk dikonsumsi frontend terpisah.
- **Frontend resminya sudah dibuat**, React + Vite + Tailwind: [github.com/zah009/atk-inventory-react](https://github.com/zah009/atk-inventory-react)
  - Dev server frontend berjalan di `http://localhost:3000` (`vite.config.js`), sudah sesuai dengan CORS policy `AllowReactApp` di `Program.cs`.
  - Base URL API di frontend diatur lewat env `VITE_API_URL`, dengan default `http://localhost:8080` — cocok dengan port API saat dijalankan via Docker Compose ([lihat bagian 8](#8-instalasi--menjalankan)). Bila menjalankan API secara manual di port `5034`, set `VITE_API_URL=http://localhost:5034` di `.env` frontend.
  - Untuk deployment production, sesuaikan origin di `AllowReactApp` (`Program.cs`) dengan domain frontend yang sebenarnya, bukan lagi `localhost:3000`.
- `Jwt:Key` di `appsettings.example.json` adalah nilai placeholder, dan `Jwt:Key`/`MSSQL_SA_PASSWORD` untuk Docker Compose datang dari `.env` (lihat [bagian 8](#8-instalasi--menjalankan)) — **wajib diisi nilai acak/kuat sendiri**, jangan pakai contoh apa pun sebelum deployment sungguhan.
- File duplikat/typo `appsetting.example.json` (tanpa huruf **s**) yang sempat disebut di versi README sebelumnya **sudah dihapus** dari repo — tidak relevan lagi, tidak perlu dicek.
- Belum ada file `.env.example` di repo padahal `docker-compose.yml` bergantung padanya secara implisit — pertimbangkan menambahkannya (isi placeholder saja, tanpa value asli) supaya kontributor baru tidak stuck di langkah Docker Compose.

---

## Menjalankan Full Stack (Backend + Frontend)

```bash
# 1. Jalankan backend (API + SQL Server) via Docker Compose
git clone https://github.com/zah009/SistemInventory_ATK_API_ASPdotNet.git
cd SistemInventory_ATK_API_ASPdotNet
docker compose up --build
# API tersedia di http://localhost:8080

# 2. Jalankan migrasi (sekali saja, setelah container db aktif)
cd Atk
dotnet ef database update
cd ..

# 3. Clone & jalankan frontend
git clone https://github.com/zah009/atk-inventory-react.git
cd atk-inventory-react
npm install
npm run go
# Frontend tersedia di http://localhost:3000
```

Buka `http://localhost:3000` di browser — frontend akan otomatis memanggil API di `http://localhost:8080`.

---

## Lisensi

Sesuaikan dengan kebutuhan (belum ada file `LICENSE` di repository ini).