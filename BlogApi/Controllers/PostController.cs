using BlogApi.DTOs;
using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    public class PostsController : BaseController
    {
        private readonly IUnitOfWork _uow;
        public PostsController(IUnitOfWork uow) => _uow = uow;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = _uow.Posts.Query().AsNoTracking();
            int totalCount = await query.CountAsync();

            var posts = await query.Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(p => new PostSummaryDto { /* map data */ }).ToListAsync();

            var result = new PagedResult<PostSummaryDto>(posts, totalCount, pageNumber, pageSize);
            return SuccessResponse(result, "Get paged posts");
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var post = await _uow.Posts.Query().FirstOrDefaultAsync(p => p.Slug == slug);
            if (post == null) return NotFoundResponse("Get post detail");

            return SuccessResponse(post, "Get post detail");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] PostCreateDto dto)
        {
            string action = "Create post";

            // 1. Lấy UserId từ Token (để biết ai là người đăng)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new ApiResponse<string>(action, "Vui lòng đăng nhập lại"));

            // 2. Tạo Slug và kiểm tra trùng lặp
            string slug = dto.Title.ToSlug();
            var isSlugExist = await _uow.Posts.Query().AnyAsync(p => p.Slug == slug);
            if (isSlugExist) return ErrorResponse(action, "Tiêu đề này đã tồn tại, vui lòng đặt tiêu đề khác");

            // 3. Khởi tạo đối tượng Post
            var post = new Post
            {
                Title = dto.Title,
                Content = dto.Content,
                Summary = dto.Summary,
                Thumbnail = dto.Thumbnail,
                Slug = slug,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                IsPublished = true, // Mặc định cho hiển thị luôn
                Tags = new List<Tag>()
            };

            // 4. Xử lý quan hệ N-N với Tags
            if (dto.TagIds != null && dto.TagIds.Any())
            {
                // Lấy danh sách các Tag từ DB dựa trên mảng ID gửi lên
                var tags = await _uow.Tags.Query()
                    .Where(t => dto.TagIds.Contains(t.Id))
                    .ToListAsync();

                post.Tags = tags;
            }

            // 5. Lưu vào Database
            try
            {
                await _uow.Posts.AddAsync(post);
                await _uow.CompleteAsync();

                return SuccessResponse(post, action, "Đăng bài viết mới thành công!");
            }
            catch (Exception ex)
            {
                return ErrorResponse(action, "Lỗi khi lưu bài viết: " + ex.Message);
            }
        }
    }
}
