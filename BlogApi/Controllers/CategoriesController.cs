using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    public class CategoriesController : BaseController
    {
        private readonly IUnitOfWork _uow;
        public CategoriesController(IUnitOfWork uow) => _uow = uow;

        // 1. Lấy danh sách (không kèm Posts để nhẹ data)
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _uow.Categories.GetAllAsync();
            // Trả về list category, mỗi category chỉ cần Id, Name, Slug
            var result = categories.Select(c => new {
                c.Id,
                c.Name,
                c.Slug,
                PostCount = c.Posts?.Count ?? 0 // Trả về số lượng bài viết để hiển thị ở Admin
            });
            return SuccessResponse(result, "Lấy danh sách thành công");
        }

        // 2. Lấy chi tiết kèm danh sách bài viết thuộc danh mục này
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Giả sử Repository của bạn có hỗ trợ Include hoặc bạn viết thêm query
            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null) return ErrorResponse("GetCategory", "Không tìm thấy danh mục");

            return SuccessResponse(category, "Lấy chi tiết thành công");
        }

        // 3. Tạo mới
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ErrorResponse("Create", "Tên danh mục không được để trống");

            var category = new Category
            {
                Name = request.Name,
                Slug = request.Name.ToSlug()
            };

            await _uow.Categories.AddAsync(category);
            await _uow.CompleteAsync();

            return SuccessResponse(category, "Đã thêm danh mục mới");
        }

        // 4. Cập nhật
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
        {
            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null) return ErrorResponse("Update", "Danh mục không tồn tại");

            if (string.IsNullOrWhiteSpace(request.Name))
                return ErrorResponse("Update", "Tên không được để trống");

            category.Name = request.Name;
            category.Slug = request.Name.ToSlug();

            _uow.Categories.Update(category);
            await _uow.CompleteAsync();

            return SuccessResponse(category, "Cập nhật thành công");
        }

        // 5. Xóa (Quan trọng: Xử lý quan hệ với Post)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null) return ErrorResponse("Delete", "Danh mục không tồn tại");

            try
            {
                _uow.Categories.Delete(category);
                await _uow.CompleteAsync();
                return SuccessResponse("Xóa danh mục thành công");
            }
            catch (Exception)
            {
                // Lỗi này thường xảy ra khi danh mục có bài viết (Foreign Key Constraint)
                return ErrorResponse("Delete", "Không thể xóa danh mục này vì đang có bài viết thuộc về nó.");
            }
        }
    }

    // DTO để nhận dữ liệu từ Frontend
    public class CategoryRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}