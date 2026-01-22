using BlogApi.DTOs;

namespace BlogApi.Interfaces
{
    public interface IPostService
    {
        Task<bool> CreateBlogPost(PostCreateDto dto);
        Task<IEnumerable<PostSummaryDto>> GetRecentPostsAsync();
    }
}