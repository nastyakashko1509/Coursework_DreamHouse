using SQLite;

namespace DreameHouse.Domain.Abstarctions
{
    public interface ICrudOperations<T>
    {
        Task CreateAsync(T item);
        Task<T> ReadByIdAsync(int id);
        Task UpdateAsync(T item);
    }
}
