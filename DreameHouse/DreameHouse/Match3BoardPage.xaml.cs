using Aplication.Services;
using Domain.Entities;
using DreameHouse.Aplication.Services;
using DreameHouse.Domain.Entities;
using DreameHouse.Infrastructure;
using DreameHouse.Infrastructure.Repositories;

namespace DreameHouse;

public partial class Match3BoardPage : ContentPage, IQueryAttributable
{
    const int TILE_SIZE = 64;
    const int MARGIN = 4;
    const int ROWS = 10;
    const int COLS = 10;

    private Match3BoardService board;
    private (int, int)? selected = null;
    private Image[,] tileImages;
    private string[,] currentSources;

    public string Id { get; set; }
    public string Level { get; set; }
    private Level _currentLevel;
    private readonly LevelService _levelService;

    private readonly PlayerRepository _playerRepository; 
    private readonly PlayerService _playerService;

    public Match3BoardPage()
    {
        InitializeComponent();

        var dbContext = new DatabaseContext();

        _levelService = new LevelService();

        _playerRepository = new PlayerRepository(dbContext.GetDatabase());
        _playerService = new PlayerService(_playerRepository);

        board = new Match3BoardService(null, ROWS, COLS);
        tileImages = new Image[ROWS, COLS];
        currentSources = new string[ROWS, COLS];
        InitGrid();
        _ = DrawBoard();  
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("level"))
        {
            Level = query["level"].ToString();
            if (int.TryParse(Level, out int levelNumber))
            {
                _currentLevel = _levelService.GetLevel(levelNumber);
                board.Level = _currentLevel;

                MovesLabel.Text = $"{_currentLevel.MaxMoves}";

                GoalCountLabel.Text = $"{_currentLevel.TargetCount}";
                var tileImage = _currentLevel.TargetType.ToString().ToLower() + ".png";
                GoalTypeImage.Source = ImageSource.FromFile(tileImage);
            }
            else
            {
                MovesLabel.Text = "Ошибка: Уровень не передан.";
                GoalCountLabel.Text = "";
                GoalTypeImage.Source = null;
            }

            if (query.TryGetValue("id", out var idValue))
            {
                Id = idValue?.ToString();
            }
        }
    }

    private void InitGrid()
    {
        GameGrid.RowDefinitions.Clear();
        GameGrid.ColumnDefinitions.Clear();
        GameGrid.Children.Clear();

        for (int r = 0; r < ROWS; r++)
            GameGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

        for (int c = 0; c < COLS; c++)
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLS; c++)
            {
                var image = new Image
                {
                    Margin = MARGIN,
                    Aspect = Aspect.AspectFit,
                    BackgroundColor = Colors.Transparent
                };

                var tapGesture = new TapGestureRecognizer();
                int row = r, col = c;
                tapGesture.Tapped += async (s, e) => await OnTileTapped(row, col);
                image.GestureRecognizers.Add(tapGesture);

                GameGrid.Children.Add(image);
                Grid.SetRow(image, r);
                Grid.SetColumn(image, c);

                tileImages[r, c] = image;
                currentSources[r, c] = string.Empty;
            }
        }
    }

    private async Task DrawBoard()
    {
        List<Task> animations = new();

        for (int c = 0; c < COLS; c++)
        {
            for (int r = 0; r < ROWS; r++)
            {
                var tile = board.Board[r, c];
                string newSource = tile == null ? "empty.png" : $"{tile.Type.ToString().ToLower()}.png";

                if (currentSources[r, c] != newSource)
                {
                    currentSources[r, c] = newSource;

                    var image = tileImages[r, c];

                    var animation = Device.InvokeOnMainThreadAsync(async () => {
                        await image.FadeTo(0, 100);
                        image.Source = ImageSource.FromFile(newSource);
                        await image.FadeTo(1, 150);
                    });

                    animations.Add(animation);
                }
                
                tileImages[r, c].BackgroundColor = selected.HasValue && selected.Value == (r, c)
                    ? Colors.Red
                    : Colors.Transparent;
            }
        }

        await Task.WhenAll(animations);
    }

    private async Task DrawBoardWithDelay(int delayMs = 200)
    {
        await DrawBoard();
        await Task.Delay(delayMs);
    }

    private async Task AnimateSwapVisual(int r1, int c1, int r2, int c2)
    {
        var img1 = tileImages[r1, c1];
        var img2 = tileImages[r2, c2];

        uint duration = 150;

        var pos1 = new Point(Grid.GetColumn(img1), Grid.GetRow(img1));
        var pos2 = new Point(Grid.GetColumn(img2), Grid.GetRow(img2));

        var anim1 = img1.TranslateTo((c2 - c1) * (TILE_SIZE + 2 * MARGIN), (r2 - r1) * (TILE_SIZE + 2 * MARGIN), duration);
        var anim2 = img2.TranslateTo((c1 - c2) * (TILE_SIZE + 2 * MARGIN), (r1 - r2) * (TILE_SIZE + 2 * MARGIN), duration);

        await Task.WhenAll(anim1, anim2);

        img1.TranslationX = 0;
        img1.TranslationY = 0;
        img2.TranslationX = 0;
        img2.TranslationY = 0;
    }
    
    private async Task AnimateBomb(int r, int c)
    {
        var img = tileImages[r, c];
        await img.ScaleTo(4, 150);
        await img.ScaleTo(1, 150);
    }

    private async Task AnimateHome(int r, int c, TileType targetType)
    {
        var affectedImages = new List<Image>();

        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                var tile = board.Board[row, col];
                if (tile != null && tile.Type == targetType)
                {
                    affectedImages.Add(tileImages[row, col]);
                }
            }
        }

        affectedImages.Add(tileImages[r, c]);

        foreach (var img in affectedImages)
        {
            img.BackgroundColor = Colors.Red;
        }

        var tasks = affectedImages.Select(img =>
            Task.WhenAll(
                img.ScaleTo(1.5, 150),
                img.FadeTo(0.0, 300)
            )
        );

        await Task.WhenAll(tasks);

        foreach (var img in affectedImages)
        {
            img.Scale = 1.0;
            img.Opacity = 1.0;
            img.BackgroundColor = Colors.Transparent;
        }
    }

    private async Task AnimateRocket(int r, int c)
    {
        var affectedImages = new List<Image>();

        for (int col = 0; col < COLS; col++)
        {
            affectedImages.Add(tileImages[r, col]);
        }

        for (int row = 0; row < ROWS; row++)
        {
            if (row != r)
                affectedImages.Add(tileImages[row, c]);
        }

        foreach (var img in affectedImages)
        {
            img.BackgroundColor = Colors.Red;
        }

        var scaleTasks = affectedImages.Select(img =>
            Task.WhenAll(
                img.ScaleTo(1.5, 150),
                img.FadeTo(0.0, 300) 
            )
        );

        await Task.WhenAll(scaleTasks);

        foreach (var img in affectedImages)
        {
            img.Scale = 1.0;
            img.Opacity = 1.0;
            img.BackgroundColor = Colors.Transparent;
        }
    }

    private void UpdateUI()
    {
        if (_currentLevel != null)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                MovesLabel.Text = $"{Math.Max(0, _currentLevel.MaxMoves - board.MovesUsed)}";
                GoalCountLabel.Text = $"{Math.Max(0, _currentLevel.TargetCount - board.TargetProgress)}";
            });

            var tileImage = _currentLevel.TargetType.ToString().ToLower() + ".png";
            GoalTypeImage.Source = ImageSource.FromFile(tileImage);
        }
    }

    private async Task CheckLevelCompletion()
    {
        if (_currentLevel == null) return;

        if (board.TargetProgress >= _currentLevel.TargetCount)
        {
            await DisplayAlert("Поздравляем!", "Уровень пройден!", "OK");

            if(int.TryParse(Id, out int playerId))
            {
                int nextLevel = _currentLevel.Number + 1;
                int bitcoin = _currentLevel.RewardBitcoin;
                await _playerService.UpdatePlayerLevelAsync(playerId, nextLevel);
                await _playerService.UpdatePlayerBitcoinAsync(playerId, bitcoin);
            }

            await Shell.Current.GoToAsync($"/room?id={Id}");
        }
        else if (board.MovesUsed >= _currentLevel.MaxMoves)
        {
            if (int.TryParse(Id, out int playerId))
            {
                await DisplayAlert("Конец игры", "Ходы закончились!", "OK");
                await Shell.Current.GoToAsync($"/room?id={Id}");
            }
        }
    }

    private async Task OnTileTapped(int r, int c)
    {
        if (selected.HasValue)
        {
            var (r1, c1) = selected.Value;

            if (board.Board[r, c]?.Type == TileType.HOME)
            {
                if (board.IsAdjacent(r, c, r1, c1))
                {
                    var targetTile = board.Board[r1, c1];
                    if (targetTile != null)
                    {
                        await AnimateHome(r, c, targetTile.Type);
                        board.ActivateBonusAndClearColor(r, c, r1, c1);
                        board.DropTiles();
                        while (board.GetMatches().Any())
                        {
                            board.ProcessTurn();
                        }
                        selected = null;
                        await DrawBoard();
                        UpdateUI();
                        await CheckLevelCompletion();

                        return;
                    }
                }

                selected = null;
                await DrawBoard();
                return;
            }

            if (board.Board[r1, c1]?.Type == TileType.HOME)
            {
                if (board.IsAdjacent(r, c, r1, c1))
                {
                    var targetTile = board.Board[r, c];
                    if (targetTile != null)
                    {
                        await AnimateHome(r, c, targetTile.Type);
                        board.ActivateBonusAndClearColor(r1, c1, r, c);
                        board.DropTiles();
                        while (board.GetMatches().Any())
                        {
                            board.ProcessTurn();
                        }
                        selected = null;
                        await DrawBoard();
                        UpdateUI();
                        await CheckLevelCompletion();
                        return;
                    }
                }

                selected = null;
                await DrawBoard();
                return;
            }

            if (r == r1 && c == c1)
            {
                if (board.Board[r, c]?.Type == TileType.BOMB)
                {
                    await AnimateBomb(r, c);
                    board.ActivateBombAndClear(r, c);
                    board.DropTiles();
                    while (board.GetMatches().Any())
                    {
                        board.ProcessTurn();
                    }
                    selected = null;
                    await DrawBoard();
                    UpdateUI();
                    await CheckLevelCompletion();

                    return;
                }
                if (board.Board[r, c]?.Type == TileType.ROCKET)
                {
                    await AnimateRocket(r, c);
                    board.ActivateRocketAndClear(r, c);
                    board.DropTiles();
                    while (board.GetMatches().Any())
                    {
                        board.ProcessTurn();
                    }
                    selected = null;
                    await DrawBoard();
                    UpdateUI();
                    await CheckLevelCompletion();

                    return;
                }
                else
                {
                    selected = null;
                    await DrawBoard();
                    return;
                }
            }

            if (!board.IsAdjacent(r1, c1, r, c))
            {
                selected = null;
                await DrawBoard();
                return;
            }

            await AnimateSwapVisual(r1, c1, r, c);

            bool success = board.TrySwapAndProcess(r1, c1, r, c);

            if (!success)
            {
                await AnimateSwapVisual(r1, c1, r, c);
            }
            else
            {
                UpdateUI();
                await CheckLevelCompletion();
            }

            selected = null;
            await DrawBoardWithDelay();
        }
        else
        {
            selected = (r, c);
            await DrawBoard();
        }
    }
}
