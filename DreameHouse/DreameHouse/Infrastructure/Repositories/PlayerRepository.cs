using DreameHouse.Domain.Entities;
using DreameHouse.Domain.Abstarctions;
using SQLite;

namespace DreameHouse.Infrastructure.Repositories
{
    public class PlayerRepository : ICrudOperations<Player>
    {
        private readonly SQLiteAsyncConnection _database;

        public PlayerRepository(SQLiteAsyncConnection database)
        {
            _database = database;
        }

        public async Task CreateAsync(Player item)
        {
            await _database.InsertAsync(item);
        }

        public async Task<Player> ReadByIdAsync(int id)
        {
            return await _database.FindAsync<Player>(id);
        }

        public async Task UpdateAsync(Player item) 
        {
            await _database.UpdateAsync(item);
        }

        public async Task<Player?> FindByEmailAndPasswordAsync(string email, string password)
        {
            return await _database.Table<Player>()
                .Where(p => p.email == email && p.password == password)
                .FirstOrDefaultAsync();
        }
    }
}
