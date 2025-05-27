using DreameHouse.Domain.Entities;
using DreameHouse.Infrastructure.Serialization;

namespace DreameHouse.Aplication.Services;

public class LevelService
{
    private readonly List<Level> _levels;

    public LevelService()
    {
        var path = Path.Combine(@"D:\4 семестр\ООП (объектно ориентированное программирование)\Coursework_DreamHouse\DreameHouse\DreameHouse\Infrastructure", "levels.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("levels.json not found", path);
        }

        var json = File.ReadAllText(path);
        _levels = JsonDeserializer.Deserialize<Level>(json);
    }

    public Level GetLevel(int levelNumber)
    {
        return _levels.FirstOrDefault(l => l.Number == levelNumber)
            ?? throw new InvalidOperationException($"Level {levelNumber} not found");
    }

    public int TotalLevels => _levels.Count;
}
