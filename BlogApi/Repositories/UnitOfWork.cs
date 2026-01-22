using BlogApi.Data;
using BlogApi.Interfaces;
using BlogApi.Models;

namespace BlogApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IGenericRepository<Post> Posts { get; private set; }
        public IGenericRepository<Category> Categories { get; private set; }
        public IGenericRepository<Tag> Tags { get; private set; }
        public IGenericRepository<User> Users { get; private set; }
        public IGenericRepository<Media> Medias { get; private set; }
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Posts = new GenericRepository<Post>(_context);
            Categories = new GenericRepository<Category>(_context);
            Tags = new GenericRepository<Tag>(_context);
            Users = new GenericRepository<User>(_context);
            Medias = new GenericRepository<Media>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
