using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Atk.Data;
using Microsoft.EntityFrameworkCore;
using Atk.Services.Interfaces;
using Atk.Services.Implementations;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Atk.Services;
using Microsoft.OpenApi.Models; // WAJIB untuk Swagger

var builder = WebApplication.CreateBuilder(args);

// ===============================
// Controllers & JSON
// ===============================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

// =================
// Add React cuy
// =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Port Vite default
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===============================
// Swagger (Swashbuckle)
// ===============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ATK API",
        Version = "v1",
        Description = "API Sistem Informasi Pengadaan & Pengolahan ATK"
    });

    // JWT Support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Masukkan token seperti ini: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===============================
// Database
// ===============================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ===============================
// Rate Limiting
// ===============================
builder.Services.AddRateLimiter(options =>
{
    // ⚠️ FIX: policy ini sebelumnya TIDAK ADA di sini, padahal dipakai lewat
    // [EnableRateLimiting("login_limit")] di AuthController. Tanpa policy
    // terdaftar, ASP.NET Core melempar InvalidOperationException begitu
    // endpoint /api/auth/login diakses -> login selalu gagal (500).
    // Window 1 menit, maksimal 5 percobaan per IP, supaya tidak mengganggu
    // user normal tapi tetap membatasi brute-force password.
    options.AddPolicy("login_limit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }
        )
    );

    options.AddFixedWindowLimiter("supplier_bulk_limit", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 3;
        opt.QueueLimit = 0;
    });

    options.AddPolicy("pengadaan_bulk_limit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }
        )
    );

    options.AddPolicy("barang_bulk_limit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(30),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }
        )
    );
});

// ===============================
// Services
// ===============================
builder.Services.AddScoped<ISupplierServices, SupplierService>();
builder.Services.AddScoped<IBarang, BarangService>();
builder.Services.AddScoped<IPengadaan, PengadaanService>();
builder.Services.AddScoped<IBarangMasuk, BarangMasukService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDivisi, DivisiService>();
builder.Services.AddScoped<IPayment, PaymentService>();
builder.Services.AddScoped<IBarangKeluar, BarangKeluarService>();
builder.Services.AddScoped<IPermintaanBarang, PermintaanBarangService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminDashboard, AdminDashboardService>();

// ===============================
// JWT Authentication
// ===============================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException(
        "Konfigurasi 'Jwt:Key' wajib diisi (appsettings.json / environment variable / user-secrets). " +
        "Aplikasi tidak akan start dengan secret key default demi keamanan.");

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // ⚠️ CATATAN (belum di-fix di sini, sengaja dibiarkan apa adanya):
    // Kode ini membaca token dari cookie "AuthToken" kalau ada, tapi TIDAK
    // ADA satupun endpoint (termasuk AuthController.Login) yang men-set
    // cookie tersebut. Ini dead code selama login hanya mengembalikan token
    // lewat JSON body. Tidak berbahaya (tidak ada cookie = tidak dibaca),
    // tapi membingungkan. Beri tahu saya kalau memang mau diaktifkan
    // (server set httponly cookie saat login) atau dihapus saja.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Cek cookie dulu
            if (context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }
            
            // Fallback ke Authorization header (untuk Swagger testing)
            if (string.IsNullOrEmpty(context.Token))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
// Auto-apply pending migration saat startup (workaround Smart App Control
// yang memblokir dotnet-ef tool reflection-load).
//
// ⚠️ FIX untuk Docker: healthcheck di docker-compose.yml sudah memastikan
// SQL Server siap SEBELUM container api dijalankan, tapi tetap ditambahkan
// retry manual di sini sebagai lapisan kedua — kondisi jaringan Docker atau
// container yang baru pertama kali init (membuat volume baru) kadang masih
// butuh beberapa detik tambahan meski healthcheck sudah "healthy". Tanpa
// retry ini, app akan langsung crash sekali gagal konek dan tidak restart
// otomatis kecuali `restart: on-failure` di-set.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Atk.Data.ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxRetries = 5;
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            var delaySeconds = attempt * 3; // 3s, 6s, 9s, 12s
            logger.LogWarning(ex,
                "Migrasi database gagal (percobaan {Attempt}/{MaxRetries}), retry dalam {Delay}s...",
                attempt, maxRetries, delaySeconds);
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}

// ===============================
// Swagger UI
// ===============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ATK API V1");
        c.RoutePrefix = "swagger"; // URL = /swagger/index.html
    });
}

// ===============================
// Middleware
// ===============================
// disini juga add react nya jangan lupa
app.UseCors("AllowReactApp");
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();