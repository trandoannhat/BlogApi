namespace BlogApi.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        IQueryable<T> Query(); // Dùng để linh hoạt Include bảng khác
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
