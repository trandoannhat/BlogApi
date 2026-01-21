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
        public async Task<IActionResult> Create(PostCreateDto dto)
        {
            // ... logic tạo bài viết ...
            await _uow.CompleteAsync();
            return SuccessResponse(post, "Create post", "Đăng bài viết mới thành công");
        }
    }
}
