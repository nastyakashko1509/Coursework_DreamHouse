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
        await Shell.Current.GoToAsync($"/room?id={Id}");
    }

    private async void OnClickedTransitionToTheBath(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"/room?id={Id}");
    }

    private async void OnClickedTransitionToTheHall(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"/room?id={Id}");
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
                if (player.task < 17)
                {
                    Bath.IsEnabled = false;
                    Bath.Text = "🔒";

                    Hall.IsEnabled = false;
                    Hall.Text = "🔒";

                    Bedroom.IsEnabled = false;
                    Bedroom.Text = "🔒";
                }
                else if (player.task > 16 && player.task < 28)
                {
                    if (Kitchen.Parent is AbsoluteLayout parent)
                    {
                        parent.Children.Remove(Kitchen); 
                    }
                    this.BackgroundImageSource = "house_one_1.png";

                    Hall.IsEnabled = false;
                    Hall.Text = "🔒";

                    Bedroom.IsEnabled = false;
                    Bedroom.Text = "🔒";
                }
                else if (player.task > 27 && player.task < 36)
                {
                    if (Kitchen.Parent is AbsoluteLayout parent_1 && Bath.Parent is AbsoluteLayout parent_2)
                    {
                        parent_1.Children.Remove(Kitchen);
                        parent_2.Children.Remove(Bath);
                    }
                    this.BackgroundImageSource = "house_one_2.png";

                    Hall.IsEnabled = false;
                    Hall.Text = "🔒";
                }
                else
                {
                    if (Kitchen.Parent is AbsoluteLayout parent_1 && Bath.Parent is AbsoluteLayout parent_2 && Bedroom.Parent is AbsoluteLayout parent_3)
                    {
                        parent_1.Children.Remove(Kitchen);
                        parent_2.Children.Remove(Bath);
                        parent_3.Children.Remove(Bedroom);
                    }
                    this.BackgroundImageSource = "house_one_3.png";
                }
            }
        }
    }
}