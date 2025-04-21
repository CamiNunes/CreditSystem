namespace CreditSystem.Application.Interfaces
{
    // Princípio SOLID: Dependency Inversion - Dependemos da abstração, não da implementação concreta
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
