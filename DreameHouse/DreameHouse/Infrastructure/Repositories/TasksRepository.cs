using DreameHouse.Domain.Abstarctions;
using DreameHouse.Domain.Entities;
using SQLite;
using System.Threading.Tasks;

namespace DreameHouse.Infrastructure.Repositories
{
    public class TasksRepository : ICrudOperations<Tasks>
    {
        private readonly SQLiteAsyncConnection _database;

        public TasksRepository(DatabaseContext dbContext)
        {
            _database = dbContext.GetDatabase();
        }

        public async Task CreateAsync(Tasks item)
        {
            await _database.InsertAsync(item);
        }

        public async Task<Tasks> ReadByIdAsync(int id)
        {
            return await _database.Table<Tasks>()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(Tasks item)
        {
            await _database.UpdateAsync(item);
        }
    }
}