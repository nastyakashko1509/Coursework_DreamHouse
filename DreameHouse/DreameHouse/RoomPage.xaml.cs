using DreameHouse.Aplication.Services;
using DreameHouse.Infrastructure.Repositories;
using DreameHouse.Infrastructure;
using DreameHouse.Domain.Entities;
using System.Collections.ObjectModel;

namespace DreameHouse;

public partial class RoomPage : ContentPage, IQueryAttributable
{
    public string? Id { get; set; }
    public Player? player;
    private readonly PlayerService _playerService;
    private readonly TaskService _taskService;
    private ObservableCollection<string> _tasksList = new ObservableCollection<string>();

    public RoomPage()
    {
        InitializeComponent();

        var dbContext = new DatabaseContext();
        var playerRepository = new PlayerRepository(dbContext.GetDatabase());
        var taskRepository = new TasksRepository(dbContext);

        _playerService = new PlayerService(playerRepository);
        _taskService = new TaskService(taskRepository, _playerService);

        TasksCollection.ItemsSource = _tasksList;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("id"))
        {
            Id = query["id"].ToString();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (int.TryParse(Id, out int playerId))
        {
            player = await _playerService.GetPlayerByIdAsync(playerId);
            if (player != null)
            {
                LevelLabel.Text = $"{player.level}";
                BitcoinLabel.Text = $"{player.bitcoin}";

                await LoadCurrentTask();

                await LoadTasksList();
            }
        }
    }

    private async Task LoadCurrentTask()
    {
        if (player != null)
        {
            var currentTask = await _taskService.GetCurrentTaskAsync(player.task);
            if (currentTask != null)
            {
                var imageSource = currentTask.Reward ?? "kitchen_0.png";
                RoomImageFrame.Content = new Image
                {
                    Source = imageSource,
                    Aspect = Aspect.Fill,
                    HeightRequest = 400,
                    WidthRequest = 600
                };
            }
        }
    }

    private async Task LoadTasksList()
    {
        _tasksList.Clear();

        if (player != null)
        {
            var currentTask = await _taskService.GetCurrentTaskAsync(player.task);
            if (currentTask != null)
            {
                _tasksList.Add($"Текущая: {currentTask.Description} (Цена: {currentTask.PriceBitcoin} биткоин)");
            }
        }
    }

    private async void OnExecuteTaskClicked(object sender, EventArgs e)
    {
        if (player == null) return;

        var currentTask = await _taskService.GetCurrentTaskAsync(player.task);
        if (currentTask == null)
        {
            await DisplayAlert("Ошибка", "Задача не найдена", "OK");
            return;
        }

        bool success = await _taskService.CompleteTaskAsync(player, currentTask);
        if (success)
        {
            BitcoinLabel.Text = $"{player.bitcoin}";
            await LoadCurrentTask();
            await LoadTasksList();

            await DisplayAlert("Успех", "Задание выполнено!", "OK");
        }
        else
        {
            await DisplayAlert("Ошибка", "Недостаточно биткоинов", "OK");
        }
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        if (player != null)
        {
            await Shell.Current.GoToAsync($"/match3board?level={player.level}&id={player.Id}");
        }
    }
}