using DreameHouse.Infrastructure.Repositories;
using DreameHouse.Infrastructure;
using DreameHouse.Aplication.Services;

namespace DreameHouse;

public partial class HouseOnePage : ContentPage, IQueryAttributable
{
    public string Id { get; set; }
    private readonly PlayerService _playerService;
    public HouseOnePage()
	{
		InitializeComponent();

        var dbContext = new DatabaseContext();

        var playerRepository = new PlayerRepository(dbContext.GetDatabase());
        _playerService = new PlayerService(playerRepository);
    }

    private async void OnClickedTransitionToTheKitchen(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"/room?id={Id}"); 
    }

    private async void OnClickedTransitionToTheBedroom(object sender, EventArgs e)
    {
    }

    private async void OnClickedTransitionToTheBath(object sender, EventArgs e)
    {
    }

    private async void OnClickedTransitionToTheHall(object sender, EventArgs e)
    {
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
            var player = await _playerService.GetPlayerByIdAsync(playerId);
            if (player != null)
            {
                if (player.level < 16)
                {
                    Bath.IsEnabled = false;
                    Bath.Text = "🔒";

                    Hall.IsEnabled = false;
                    Hall.Text = "🔒";

                    Bedroom.IsEnabled = false;
                    Bedroom.Text = "🔒";
                }
            }
        }
    }
}