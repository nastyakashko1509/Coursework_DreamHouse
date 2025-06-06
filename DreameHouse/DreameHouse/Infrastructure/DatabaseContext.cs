using SQLite;
using DreameHouse.Domain.Entities;
using DreameHouse.Infrastructure.Serialization;
using System.Reflection;

namespace DreameHouse.Infrastructure
{
    public class DatabaseContext
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseContext()
        {
            var dbPath = Path.Combine(@"D:\4 семестр\ООП (объектно ориентированное программирование)\Coursework_DreamHouse", "dreame_house.db");
            _database = new SQLiteAsyncConnection(dbPath);
        }

        public SQLiteAsyncConnection GetDatabase() => _database;

        public async Task InitializeAsync()
        {
            await _database.CreateTableAsync<Player>();
            await _database.CreateTableAsync<Tasks>();

            await SyncTasksFromJsonAsync();
        }

        private async Task SyncTasksFromJsonAsync()
        {
            var path = Path.Combine(@"D:\4 семестр\ООП (объектно ориентированное программирование)\Coursework_DreamHouse\DreameHouse\DreameHouse\Infrastructure", "tasks.json");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("tasks.json not found", path);
            }

            var json = await File.ReadAllTextAsync(path);
            var jsonTasks = JsonDeserializer.Deserialize<Tasks>(json);

            // Получаем все задачи из БД (для оптимизации можно взять только Id)
            var dbTaskIds = (await _database.Table<Tasks>().ToListAsync())
                .Select(t => t.Id)
                .ToHashSet(); // Используем HashSet для быстрого поиска

            foreach (var jsonTask in jsonTasks)
            {
                // Если задачи с таким Id нет в БД — добавляем
                if (!dbTaskIds.Contains(jsonTask.Id))
                {
                    await _database.InsertAsync(jsonTask);
                }
                // Если есть — проверяем, изменились ли данные (опционально)
                else if (!AreTasksEqual(
                    await _database.Table<Tasks>().FirstOrDefaultAsync(t => t.Id == jsonTask.Id),
                    jsonTask))
                {
                    await _database.UpdateAsync(jsonTask);
                }
            }
        }

        private bool AreTasksEqual(Tasks a, Tasks b)
        {
            return a.Description == b.Description
                && a.Reward == b.Reward
                && a.PriceBitcoin == b.PriceBitcoin;
        }
    }
}
