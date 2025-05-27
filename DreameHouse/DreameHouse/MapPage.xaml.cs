using DreameHouse.Aplication.Services;
using DreameHouse.Domain.Entities;
using DreameHouse.Infrastructure.Repositories;
using DreameHouse.Infrastructure;
using DreameHouse.Validators;
namespace DreameHouse;

public partial class MapPage : ContentPage, IQueryAttributable
{
    public string Id { get; set; }
    private readonly PlayerService _playerService;

    public MapPage()
	{
		InitializeComponent();

        var dbContext = new DatabaseContext();
        var playerRepository = new PlayerRepository(dbContext.GetDatabase());
        _playerService = new PlayerService(playerRepository);
    }
 
    private async void OnClickedTransitionToTheFirstHouse(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"/house_one?id={Id}");
    }

    private async void OnClickedTransitionToTheSecondHouse(object sender, EventArgs e)
    {
    }

    private async void OnClickedTransitionToTheThirdHouse(object sender, EventArgs e)
    {
    }

    private async void OnClickedTransitionToTheFourthHouse(object sender, EventArgs e)
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
                if (player.level < 100)
                {
                    HouseTwoButton.IsEnabled = false;
                    HouseTwoButton.Text = "🔒";

                    HouseThreeButton.IsEnabled = false;
                    HouseThreeButton.Text = "🔒";

                    HouseFourButton.IsEnabled = false;
                    HouseFourButton.Text = "🔒";
                }
            }
        }
    }
}