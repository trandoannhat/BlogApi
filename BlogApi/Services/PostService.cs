using BlogApi.DTOs;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services
{
    //lạ thât đâu có trùng namspace
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _uow;
        public PostService(IUnitOfWork uow) => _uow = uow;

        public async Task<IEnumerable<PostSummaryDto>> GetRecentPostsAsync()
        {
            return await _uow.Posts.Query()
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    CategoryName = p.Category.Name,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
        }

        public async Task<bool> CreateBlogPost(PostCreateDto dto)
        {
            var post = new Post
            {
                Title = dto.Title,
                Content = dto.Content,
                CategoryId = dto.CategoryId,
                Slug = dto.Title.ToLower().Replace(" ", "-") // Logic tạo slug tạm thời
            };

            // Gắn Tags
            var tags = await _uow.Tags.Query().Where(t => dto.TagIds.Contains(t.Id)).ToListAsync();
            post.Tags = tags;

            await _uow.Posts.AddAsync(post);
            return await _uow.CompleteAsync() > 0;
        }
    }
}
