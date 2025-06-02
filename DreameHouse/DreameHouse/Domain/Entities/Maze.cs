namespace DreameHouse.Domain.Entities
{
    public class Maze
    {
        public int Width { get; } = 10;
        public int Height { get; } = 10;
        public bool[,] Walls { get; private set; }
        public (int X, int Y) Player { get; set; }
        public (int X, int Y) Exit { get; private set; }
        public bool IsGameOver { get; private set; }

        [ThreadStatic]
        private Random _random = new Random();

        public Maze()
        {
            if (_random == null)
            {
                _random = new Random(Guid.NewGuid().GetHashCode());
            }
            Generate();
        }

        public void Generate()
        {
            Walls = new bool[Width, Height];
            IsGameOver = false;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Walls[x, y] = true;

            for (int x = 0; x < Width; x++)
            {
                Walls[x, 0] = true;
                Walls[x, Height - 1] = true;
            }
            for (int y = 0; y < Height; y++)
            {
                Walls[0, y] = true;
                Walls[Width - 1, y] = true;
            }

            GenerateMazeDFS();
            AddRandomPaths(10);

            Player = (1, 1);
            Exit = (Width - 2, Height - 2);
            Walls[Player.X, Player.Y] = false;
            Walls[Exit.X, Exit.Y] = false;

            EnsurePathToExit();
        }

        private void GenerateMazeDFS()
        {
            var stack = new Stack<(int x, int y)>();
            var start = (x: _random.Next(1, Width - 1) | 1, y: _random.Next(1, Height - 1) | 1);
            var visited = new bool[Width, Height];

            Walls[start.x, start.y] = false;
            visited[start.x, start.y] = true;
            stack.Push(start);

            var directions = new (int dx, int dy)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var neighbors = new List<(int x, int y)>();

                foreach (var (dx, dy) in directions.OrderBy(_ => _random.NextDouble()))
                {
                    int nx = current.x + dx * 2;
                    int ny = current.y + dy * 2;

                    if (nx > 0 && nx < Width - 1 && ny > 0 && ny < Height - 1 && !visited[nx, ny])
                    {
                        neighbors.Add((nx, ny));
                    }
                }

                if (neighbors.Count > 0)
                {
                    stack.Push(current); 
                    var next = neighbors[_random.Next(neighbors.Count)];
                    var wallX = (current.x + next.x) / 2;
                    var wallY = (current.y + next.y) / 2;

                    Walls[next.x, next.y] = false;
                    Walls[wallX, wallY] = false;
                    visited[next.x, next.y] = true;
                    stack.Push(next);
                }
            }
        }

        private void AddRandomPaths(int extraPaths = 5)
        {
            for (int i = 0; i < extraPaths; i++)
            {
                int x = _random.Next(1, Width - 1);
                int y = _random.Next(1, Height - 1);

                if (Walls[x, y])
                {
                    int freeNeighbors = CountFreeNeighbors(x, y);
                    if (freeNeighbors >= 2)
                    {
                        Walls[x, y] = false;
                    }
                }
            }
        }

        private int CountFreeNeighbors(int x, int y)
        {
            int count = 0;
            if (x > 0 && !Walls[x - 1, y]) count++;
            if (x < Width - 1 && !Walls[x + 1, y]) count++;
            if (y > 0 && !Walls[x, y - 1]) count++;
            if (y < Height - 1 && !Walls[x, y + 1]) count++;
            return count;
        }

        private void EnsurePathToExit()
        {
            if (!PathExists(Player, Exit))
            {
                ConnectPoints(Player, Exit);
            }
        }

        private void ConnectPoints((int X, int Y) start, (int X, int Y) end)
        {
            int x = start.X;
            int y = start.Y;

            while (x != end.X || y != end.Y)
            {
                if (x < end.X) x++;
                else if (x > end.X) x--;

                if (y < end.Y) y++;
                else if (y > end.Y) y--;

                Walls[x, y] = false;

                if (x < end.X && y < end.Y)
                {
                    Walls[x + 1, y] = false;
                    Walls[x, y + 1] = false;
                }
            }
        }

        private bool PathExists((int X, int Y) start, (int X, int Y) end)
        {
            var visited = new bool[Width, Height];
            var queue = new Queue<(int X, int Y)>();
            queue.Enqueue(start);
            visited[start.X, start.Y] = true;

            var directions = new (int dx, int dy)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.X == end.X && current.Y == end.Y)
                    return true;

                foreach (var dir in directions)
                {
                    int nx = current.X + dir.dx;
                    int ny = current.Y + dir.dy;

                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height &&
                        !Walls[nx, ny] && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
            return false;
        }

        public bool CanMove(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                IsGameOver = true;
                return false;
            }
            return !Walls[x, y];
        }

        public bool IsPlayerWin()
        {
            return Player.X == Exit.X && Player.Y == Exit.Y;
        }
    }
}
