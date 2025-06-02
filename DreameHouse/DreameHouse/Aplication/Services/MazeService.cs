using DreameHouse.Domain.Entities;

namespace DreameHouse.Aplication.Services
{
    public class MazeService
    {
        private Maze _maze = new Maze();

        public bool[,] GetWalls() => _maze.Walls;
        public (int X, int Y) GetPlayer() => _maze.Player;
        public (int X, int Y) GetExit() => _maze.Exit;
        public bool IsGameOver => _maze.IsGameOver;

        public bool MovePlayer(int deltaX, int deltaY)
        {
            int newX = _maze.Player.X + deltaX;
            int newY = _maze.Player.Y + deltaY;

            if (_maze.CanMove(newX, newY))
            {
                _maze.Player = (newX, newY);
                return true;
            }
            return false;
        }

        public bool CheckWin() => _maze.IsPlayerWin();
        public void Regenerate() => _maze.Generate();
    }
}
