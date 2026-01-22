using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data
{
    public static class SeedData
    {
        public static async Task Initialize(AppDbContext context)
        {
            // 1. Kiểm tra nếu đã có dữ liệu thì không seed nữa
            if (await context.Categories.AnyAsync() || await context.Users.AnyAsync())
            {
                return;
            }

            // 2. Seed Categories (Danh mục)
            var categories = new List<Category>
            {
                new Category { Name = "Lập trình .NET", Slug = "lap-trinh-net" },
                new Category { Name = "Góc Dev", Slug = "goc-dev" },
                new Category { Name = "Thủ thuật Công nghệ", Slug = "thu-thuat-cong-nghe" }
            };
            await context.Categories.AddRangeAsync(categories);

            // 3. Seed Tags (Thẻ tag)
            var tags = new List<Tag>
            {
                new Tag { Name = "C#", Slug = "c-sharp" },
                new Tag { Name = "Backend", Slug = "backend" },
                new Tag { Name = "Entity Framework", Slug = "entity-framework" }
            };
            await context.Tags.AddRangeAsync(tags);

            // 4. Seed Admin User mặc định
            // Lưu ý: Dùng BCrypt để mã hóa mật khẩu trước khi lưu
            var adminUser = new User
            {
                Username = "nhatdev",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Nhat123@A") // Mật khẩu mặc định
            };
            await context.Users.AddAsync(adminUser);

            // 5. Lưu tất cả thay đổi
            await context.SaveChangesAsync();
        }
    }
}
