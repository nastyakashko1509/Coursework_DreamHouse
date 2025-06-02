using Domain.Entities;
using DreameHouse.Domain.Entities;

namespace Aplication.Services;

public class Match3BoardService
{
    private readonly int rows;
    private readonly int cols;

    public int MovesUsed { get; set; }
    public int TargetProgress { get; set; }

    public Tile?[,] Board { get; set; }
    public Level Level { get; set; }

    private static readonly Random random = new();

    public Match3BoardService(Level level, int rows = 10, int cols = 10)
    {
        Level = level;
        this.rows = rows;
        this.cols = cols;
        Board = new Tile[rows, cols];

        var tileTypes = Enum.GetValues(typeof(TileType))
            .Cast<TileType>()
            .Where(t => t != TileType.HOME && t != TileType.BOMB && t != TileType.ROCKET)
            .ToArray();

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                Board[r, c] = new Tile(tileTypes[random.Next(tileTypes.Length)]);

        RemoveInitialMatches();
    }

    private void RemoveInitialMatches()
    {
        while (GetMatches().Any())
        {
            ProcessTurn();
        }
    }

    public void PrintBoard()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Console.Write(Board[r, c]?.ToString().PadRight(6));
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public bool IsAdjacent(int r1, int c1, int r2, int c2) =>
        Math.Abs(r1 - r2) + Math.Abs(c1 - c2) == 1;

    public void Swap(int r1, int c1, int r2, int c2)
    {
        (Board[r1, c1], Board[r2, c2]) = (Board[r2, c2], Board[r1, c1]);
    }

    public Dictionary<HashSet<(int, int)>, int> GetMatches()
    {
        var matches = new Dictionary<HashSet<(int, int)>, int>();

        // Горизонтальные совпадения
        for (int r = 0; r < rows; r++)
        {
            int c = 0;
            while (c < cols - 2)
            {
                int matchLen = 1;
                while (c + matchLen < cols &&
                       Board[r, c] != null &&
                       Board[r, c + matchLen] != null &&
                       Board[r, c]?.Type == Board[r, c + matchLen]?.Type)
                {
                    matchLen++;
                }

                if (matchLen >= 3)
                {
                    var coords = new HashSet<(int, int)>();
                    for (int i = 0; i < matchLen; i++) coords.Add((r, c + i));
                    matches[coords] = matchLen;
                    c += matchLen;
                }
                else
                {
                    c++;
                }
            }
        }

        // Вертикальные совпадения
        for (int c = 0; c < cols; c++)
        {
            int r = 0;
            while (r < rows - 2)
            {
                int matchLen = 1;
                while (r + matchLen < rows &&
                       Board[r, c] != null &&
                       Board[r + matchLen, c] != null &&
                       Board[r, c]?.Type == Board[r + matchLen, c]?.Type)
                {
                    matchLen++;
                }

                if (matchLen >= 3)
                {
                    var coords = new HashSet<(int, int)>();
                    for (int i = 0; i < matchLen; i++) coords.Add((r + i, c));
                    matches[coords] = matchLen;
                    r += matchLen;
                }
                else
                {
                    r++;
                }
            }
        }

        return matches;
    }

    public void RemoveMatches(HashSet<(int, int)> matches)
    {
        foreach (var (r, c) in matches)
        {
            Board[r, c] = null;
        }
    }

    public void DropTiles()
    {
        var normalTypes = Enum.GetValues(typeof(TileType))
            .Cast<TileType>()
            .Where(t => t != TileType.HOME && t != TileType.BOMB && t != TileType.ROCKET)
            .ToArray();

        for (int c = 0; c < cols; c++)
        {
            var column = new List<Tile>();
            for (int r = rows - 1; r >= 0; r--)
            {
                if (Board[r, c] != null)
                    column.Add(Board[r, c]);
            }

            int missing = rows - column.Count;
            for (int i = 0; i < missing; i++)
                column.Add(new Tile(normalTypes[random.Next(normalTypes.Length)]));

            for (int r = rows - 1, i = 0; r >= 0; r--, i++)
                Board[r, c] = column[i];
        }
    }

    public void ProcessTurn()
    {
        while (true)
        {
            var matches = GetMatches();
            if (!matches.Any()) break;

            if (Level != null)
            {
                foreach (var match in matches)
                {
                    var firstTile = Board[match.Key.First().Item1, match.Key.First().Item2];
                    if (firstTile != null && firstTile.Type == Level.TargetType)
                    {
                        TargetProgress += match.Value; 
                    }
                }
            }

            var mergedGroups = MergeIntersectingMatches(matches.Keys.ToList());

            foreach (var group in mergedGroups)
            {
                var coordsList = group.OrderBy(p => p).ToList();
                var length = group.Count;

                if (IsLShape(group))
                {
                    var (centerR, centerC) = coordsList[coordsList.Count / 2];
                    Board[centerR, centerC] = new Tile(TileType.ROCKET);
                    foreach (var (r, c) in group)
                        if ((r, c) != (centerR, centerC)) Board[r, c] = null;
                }
                else if (length == 5)
                {
                    var (centerR, centerC) = coordsList[coordsList.Count / 2];
                    Board[centerR, centerC] = new Tile(TileType.HOME);
                    foreach (var (r, c) in group)
                        if ((r, c) != (centerR, centerC)) Board[r, c] = null;
                }
                else if (length == 4)
                {
                    var (centerR, centerC) = coordsList[coordsList.Count / 2];
                    Board[centerR, centerC] = new Tile(TileType.BOMB);
                    foreach (var (r, c) in group)
                        if ((r, c) != (centerR, centerC)) Board[r, c] = null;
                }
                else
                {
                    foreach (var (r, c) in group)
                        Board[r, c] = null;
                }
            }

            DropTiles();
        }
    }

    private List<HashSet<(int, int)>> MergeIntersectingMatches(List<HashSet<(int, int)>> matches)
    {
        var merged = new List<HashSet<(int, int)>>();

        foreach (var match in matches)
        {
            bool added = false;
            for (int i = 0; i < merged.Count; i++)
            {
                if (merged[i].Overlaps(match))
                {
                    merged[i].UnionWith(match);
                    added = true;
                    break;
                }
            }

            if (!added)
                merged.Add(new HashSet<(int, int)>(match));
        }

        bool merging;
        do
        {
            merging = false;
            for (int i = 0; i < merged.Count; i++)
            {
                for (int j = i + 1; j < merged.Count; j++)
                {
                    if (merged[i].Overlaps(merged[j]))
                    {
                        merged[i].UnionWith(merged[j]);
                        merged.RemoveAt(j);
                        merging = true;
                        break;
                    }
                }
                if (merging) break;
            }
        } while (merging);

        return merged;
    }
    private bool IsLShape(HashSet<(int, int)> group)
    {
        if (group.Count < 5) return false;

        var coords = group.ToList();
        var rCounts = coords.GroupBy(p => p.Item1).ToDictionary(g => g.Key, g => g.Count());
        var cCounts = coords.GroupBy(p => p.Item2).ToDictionary(g => g.Key, g => g.Count());

        var rowsWith3 = rCounts.Where(kv => kv.Value >= 3).Select(kv => kv.Key).ToList();
        var colsWith3 = cCounts.Where(kv => kv.Value >= 3).Select(kv => kv.Key).ToList();

        foreach (var r in rowsWith3)
        {
            foreach (var c in colsWith3)
            {
                if (group.Contains((r, c)))
                    return true;
            }
        }

        return false;
    }

    public bool TrySwapAndProcess(int r1, int c1, int r2, int c2)
    {
        if (Board[r1, c1]?.Type == TileType.BOMB && Board[r2, c2]?.Type == TileType.BOMB)
        {
            if (ActivateBombAndClear(r1, c1))
            {
                DropTiles();
                return true;
            }
        }

        if (Board[r1, c1]?.Type == TileType.ROCKET && Board[r2, c2]?.Type == TileType.ROCKET)
        {
            if (ActivateRocketAndClear(r1, c1))
            {
                DropTiles();
                return true;
            }
        }

        if (!IsAdjacent(r1, c1, r2, c2))
            return false;

        Swap(r1, c1, r2, c2);

        if (Board[r1, c1]?.Type == TileType.HOME)
        {
            if (ActivateBonusAndClearColor(r1, c1, r2, c2))
            {
                DropTiles();
                return true;
            }
        }

        if (Board[r2, c2]?.Type == TileType.HOME)
        {
            if (ActivateBonusAndClearColor(r2, c2, r1, c1))
            {
                DropTiles();
                return true;
            }
        }

        if (GetMatches().Any())
        {
            MovesUsed++;
            while (GetMatches().Any())
            {
                ProcessTurn();
            }
            return true;
        }

        Swap(r1, c1, r2, c2);
        return false;
    }

    public bool ActivateBombAndClear(int r, int c)
    {
        MovesUsed++;
        int targetCleared = 0;
        TileType? targetType = Level?.TargetType;

        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                int nr = r + dr;
                int nc = c + dc;
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && Board[nr, nc] != null)
                {
                    if (targetType.HasValue && Board[nr, nc].Type == targetType.Value)
                    {
                        targetCleared++;
                    }
                    Board[nr, nc] = null;
                }
            }
        }

        if (targetCleared > 0)
        {
            TargetProgress += targetCleared;
        }
        return true;
    }

    public bool ActivateRocketAndClear(int r, int c)
    {
        MovesUsed++;
        int targetCleared = 0;
        TileType? targetType = Level?.TargetType;

        for (int row = 0; row < rows; row++)
        {
            if (Board[row, c] != null)
            {
                if (targetType.HasValue && Board[row, c].Type == targetType.Value)
                {
                    targetCleared++;
                }
                Board[row, c] = null;
            }
        }

        for (int column = 0; column < cols; column++)
        {
            if (Board[r, column] != null && column != c) 
            {
                if (targetType.HasValue && Board[r, column].Type == targetType.Value)
                {
                    targetCleared++;
                }
                Board[r, column] = null;
            }
        }

        if (targetCleared > 0)
        {
            TargetProgress += targetCleared;
        }
        return true;
    }

    public bool ActivateBonusAndClearColor(int r1, int c1, int r2, int c2)
    {
        MovesUsed++;
        var bonusTile = Board[r1, c1];
        if (bonusTile == null || bonusTile.Type != TileType.HOME)
            return false;

        var targetTile = Board[r2, c2];
        if (targetTile == null) return false;

        var targetType = targetTile.Type;
        int targetCleared = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (Board[r, c]?.Type == targetType)
                {
                    if (Level != null && targetType == Level.TargetType)
                    {
                        targetCleared++;
                    }
                    Board[r, c] = null;
                }
            }
        }

        if (Level != null && Level.TargetType == TileType.HOME)
        {
            TargetProgress++; 
        }

        else if (Level != null && targetType == Level.TargetType)
        {
            TargetProgress += targetCleared;
        }

        Board[r1, c1] = null;
        return true;
    }
}
