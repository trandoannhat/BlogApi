using BlogApi.DTOs;
using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BlogApi.Controllers;

[Route("api/[controller]")]
public class PostsController : BaseController
{
    private readonly IUnitOfWork _uow;
    public PostsController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var action = "Get paged posts";

        // 1. Tạo query cơ bản
        var query = _uow.Posts.Query().AsNoTracking();

        // 2. Lấy tổng số lượng (để phân trang)
        int totalCount = await query.CountAsync();

        // 3. Thực thi query và Map trực tiếp sang DTO để tránh lấy dư dữ liệu (Performance)
        var posts = await query
            .Include(p => p.Category)
            .Include(p => p.Tags) // Đừng quên Include Tags nếu muốn hiển thị ở danh sách
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                Summary = p.Summary,
                Slug = p.Slug,
                Thumbnail = p.Thumbnail,
                CreatedAt = p.CreatedAt,
                CategoryName = p.Category != null ? p.Category.Name : "Không xác định",
                // Cách map List Tag sang List String đơn giản nhất
                Tags = p.Tags.Select(t => t.Name).ToList()
            })
            .ToListAsync();

        // 4. Trả về kết quả phân trang
        var result = new PagedResult<PostSummaryDto>(posts, totalCount, pageNumber, pageSize);
        return SuccessResponse(result, action);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var action = "Get post detail";

        // 1. Lấy dữ liệu từ DB kèm theo Category và Tags
        var post = await _uow.Posts.Query()
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        // 2. Kiểm tra nếu không tìm thấy
        if (post == null) return NotFoundResponse("Không tìm thấy bài viết");

        // 3. Map thủ công sang PostDetailDto để khớp với class bạn đã tạo
        var postDetail = new PostDetailDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Slug = post.Slug,
            Thumbnail = post.Thumbnail,
            CategoryName = post.Category?.Name ?? "Không xác định",
            Tags = post.Tags.Select(t => t.Name).ToList(),
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };

        // 4. Trả về DTO thay vì Entity
        return SuccessResponse(postDetail, action);
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

            // CHỈ TRẢ VỀ DỮ LIỆU CẦN THIẾT (Không trả về nguyên Entity post)
            var result = new
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                CreatedAt = post.CreatedAt
            };

            return SuccessResponse(result, action, "Đăng bài viết mới thành công!");
        }
        catch (Exception ex)
        {
            // Log lỗi thật ra Console để xem trên Docker logs
            Console.WriteLine($"Lỗi tại PostsController: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

            return ErrorResponse(action, "Lỗi Database: " + (ex.InnerException?.Message ?? ex.Message));
        }
    }
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] PostUpdateDto dto)
    {
        string action = "Update post";

        var post = await _uow.Posts.Query()
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFoundResponse("Không tìm thấy bài viết");

        // Nếu đổi title → kiểm tra slug
        var newSlug = dto.Title.ToSlug();
        if (post.Slug != newSlug)
        {
            var slugExist = await _uow.Posts.Query()
                .AnyAsync(p => p.Slug == newSlug && p.Id != id);

            if (slugExist)
                return ErrorResponse(action, "Tiêu đề này đã tồn tại");
        }

        // Update field
        post.Title = dto.Title;
        post.Content = dto.Content;
        post.Summary = dto.Summary;
        post.Thumbnail = dto.Thumbnail;
        post.CategoryId = dto.CategoryId;
        post.Slug = newSlug;
        post.IsPublished = dto.IsPublished;
        post.UpdatedAt = DateTime.UtcNow;

        // Update Tags (N-N)
        post.Tags.Clear();
        if (dto.TagIds.Any())
        {
            var tags = await _uow.Tags.Query()
                .Where(t => dto.TagIds.Contains(t.Id))
                .ToListAsync();

            post.Tags = tags;
        }

        await _uow.CompleteAsync();
        return SuccessResponse(post.Id, action, "Cập nhật bài viết thành công");
    }
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        string action = "Delete post";

        var post = await _uow.Posts.GetByIdAsync(id);
        if (post == null)
            return NotFoundResponse("Không tìm thấy bài viết");

        _uow.Posts.Delete(post);
        await _uow.CompleteAsync();

        return SuccessResponse<object>(null, action, "Đã xóa bài viết");
    }
    [HttpPatch("{id}/publish")]
    [Authorize]
    public async Task<IActionResult> TogglePublish(int id)
    {
        var post = await _uow.Posts.GetByIdAsync(id);
        if (post == null)
            return NotFoundResponse("Không tìm thấy bài viết");

        post.IsPublished = !post.IsPublished;
        post.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync();

        return SuccessResponse(
            post.IsPublished,
            "Toggle publish",
            post.IsPublished ? "Đã xuất bản" : "Đã gỡ bài"
        );
    }

    //dùng để xem cả file rác, tạm
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAdmin()
    {
        var posts = await _uow.Posts.Query()
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.IsPublished,
                p.CreatedAt,
                Category = p.Category!.Name
            })
            .ToListAsync();

        return SuccessResponse(posts, "Admin post list");
    }
    [HttpGet("admin/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var post = await _uow.Posts.Query()
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFoundResponse("Không tìm thấy bài viết");

        var result = new
        {
            post.Id,
            post.Title,
            post.Summary,
            post.Content,
            post.CategoryId,
            post.Thumbnail,
            post.IsPublished,
            TagIds = post.Tags.Select(t => t.Id).ToList()
        };

        return SuccessResponse(result, "Get post by id");
    }

}
