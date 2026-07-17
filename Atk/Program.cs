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
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey123!";

var key = Encoding.ASCII.GetBytes(jwtKey);

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

    // ✅ BACA TOKEN DARI COOKIE
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
