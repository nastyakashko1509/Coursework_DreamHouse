using DreameHouse.Aplication.Services;
using DreameHouse.Domain.Entities;
using DreameHouse.Infrastructure;
using DreameHouse.Infrastructure.Repositories;
using Microsoft.UI.Xaml.Documents;

namespace DreameHouse;

public partial class MazePage : ContentPage, IQueryAttributable
{
    private MazeService _mazeService = new MazeService();
    private const int MazeSize = 10; 
    private const int CellSize = 45;

    public string Id { get; set; }
    public string Level { get; set; }

    private readonly PlayerRepository _playerRepository;
    private readonly PlayerService _playerService;
    private readonly LevelService _levelService;
    private Level _currentLevel;

    public MazePage()
    {
        InitializeComponent();

        var dbContext = new DatabaseContext();

        _levelService = new LevelService();

        _playerRepository = new PlayerRepository(dbContext.GetDatabase());
        _playerService = new PlayerService(_playerRepository);

        BuildMaze();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("level"))
        {
            Level = query["level"].ToString();
            if (int.TryParse(Level, out int levelNumber))
            {
                _currentLevel = _levelService.GetLevel(levelNumber);
            }

            if (query.TryGetValue("id", out var idValue))
            {
                Id = idValue?.ToString();
            }
        }
    }

    private void BuildMaze()
    {
        MazeGrid.Children.Clear();
        MazeGrid.ColumnDefinitions.Clear();
        MazeGrid.RowDefinitions.Clear();

        for (int i = 0; i < MazeSize; i++)
        {
            MazeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = CellSize });
            MazeGrid.RowDefinitions.Add(new RowDefinition { Height = CellSize });
        }

        var walls = _mazeService.GetWalls();

        for (int x = 0; x < MazeSize; x++)
        {
            for (int y = 0; y < MazeSize; y++)
            {
                var box = new BoxView
                {
                    BackgroundColor = walls[x, y] ?
                        (IsBorder(x, y) ?
                         Color.FromArgb("#795548") : 
                         Color.FromArgb("#FF9800")) : 
                        Color.FromArgb("#FFF5E6")    
                };
                MazeGrid.Add(box, x, y);
            }
        }

        DrawPlayer();
        DrawExit();
    }

    private bool IsBorder(int x, int y)
    {
        return x == 0 || y == 0 || x == MazeSize - 1 || y == MazeSize - 1;
    }

    private void DrawPlayer()
    {
        var player = _mazeService.GetPlayer();
        var playerImage = new Image
        {
            Source = "cat.png",
            WidthRequest = CellSize - 8,
            HeightRequest = CellSize - 8,
            Aspect = Aspect.AspectFill
        };
        MazeGrid.Add(playerImage, player.X, player.Y);
    }

    private void DrawExit()
    {
        var exit = _mazeService.GetExit();
        var exitView = new BoxView
        {
            BackgroundColor = Color.FromArgb("#4CAF50"),
            WidthRequest = CellSize - 10,
            HeightRequest = CellSize - 10,
            CornerRadius = 5
        };
        MazeGrid.Add(exitView, exit.X, exit.Y);
    }

    private void MovePlayerAndCheck(int deltaX, int deltaY)
    {
        if (_mazeService.IsGameOver)
        {
            DisplayAlert("Игра окончена", "Вы проиграли! Начнём заново?", "OK");
            ResetGame();
            return;
        }

        if (_mazeService.MovePlayer(deltaX, deltaY))
        {
            BuildMaze();
            CheckGameStatus();
        }
    }

    private async void CheckGameStatus()
    {
        if (_mazeService.CheckWin())
        {
            WinLabel.Text = "Победа! 🎉";
            DisplayAlert("Поздравляем!", "Вы прошли лабиринт!", "OK");
            ResetGame();

            if (int.TryParse(Id, out int playerId))
            {
                int nextLevel = _currentLevel.Number + 1;
                await _playerService.UpdatePlayerBitcoinAsync(playerId, 1);
                await _playerService.UpdatePlayerLevelAsync(playerId, nextLevel);
            }

            await Shell.Current.GoToAsync($"/room?id={Id}");
        }
        else if (_mazeService.IsGameOver)
        {
            WinLabel.Text = "Проигрыш! 💀";
            DisplayAlert("Увы!", "Вы попали в ловушку!", "OK");
            ResetGame();
        }
    }

    private void ResetGame()
    {
        _mazeService.Regenerate();
        WinLabel.Text = "";
        BuildMaze();
    }

    private void OnUpClicked(object sender, EventArgs e) => MovePlayerAndCheck(0, -1);
    private void OnDownClicked(object sender, EventArgs e) => MovePlayerAndCheck(0, 1);
    private void OnLeftClicked(object sender, EventArgs e) => MovePlayerAndCheck(-1, 0);
    private void OnRightClicked(object sender, EventArgs e) => MovePlayerAndCheck(1, 0);
}
