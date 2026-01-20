using BlogApi.Data;
using Microsoft.EntityFrameworkCore;

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
                "http://localhost:3000"       // React cũ Local
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Cho phép gửi kèm Cookie/Auth nếu cần
    });
});


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Kích hoạt CORS - Quan trọng: Phải đặt trước MapControllers
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
