using BlogApi.Models;

namespace BlogApi.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Post> Posts { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Tag> Tags { get; }
        IGenericRepository<User> Users { get; } 
        Task<int> CompleteAsync(); // SaveChanges
    }
}
