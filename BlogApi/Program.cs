using BlogApi.Data;
using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Middlewares;
using BlogApi.Repositories;
using BlogApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
// 1. Thêm dịch vụ CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.WithOrigins(
                "https://admin.nhatdev.top",  // Admin trên Server
                "https://nhatdev.top",        // Web trên Server
                "http://localhost:5173",      // React/Vite Local
                "http://localhost:3000"       // React cũ Local ok
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Cho phép gửi kèm Cookie/Auth nếu cần
    });
});

//register services for UnitOfWork and repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Đăng ký MediaService (Nhớ tạo file Service ở bước dưới trước)
builder.Services.AddScoped<IMediaService, MediaService>();
// Add services to the container.

//Register DI
// Đăng ký cấu hình Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập Token theo cú pháp: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});


// 1. Cấu hình JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    // Lỗi này sẽ hiện rõ trong Docker logs nếu bạn quên cấu hình Key
    throw new Exception("LỖI: JWT Key chưa được cấu hình trong appsettings hoặc Environment Variables!");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// 2. Thêm Authorization
builder.Services.AddAuthorization();
var app = builder.Build();
// Đặt ở ngay đầu app
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Kích hoạt CORS - Quan trọng: Phải đặt trước MapControllers
app.UseCors("AllowAll");

// 3. Sử dụng (Thứ tự rất quan trọng: Authentication trước Authorization)
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Tự động chạy Migration nếu chưa chạy
        await context.Database.MigrateAsync();
        Console.WriteLine("------> Đang bắt đầu Seeding...");
        // Chạy hàm Seeding
        await SeedData.Initialize(context);
        Console.WriteLine("------> Seeding hoàn tất!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Một lỗi đã xảy ra trong quá trình Seeding dữ liệu.");
    }
}

app.Run();


