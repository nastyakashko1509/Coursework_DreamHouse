using DreameHouse.Aplication.Services;
using DreameHouse.Infrastructure.Repositories;
using DreameHouse.Infrastructure;
using DreameHouse.Validators;
using DreameHouse.Domain.Entities;

namespace DreameHouse
{
    public partial class MainPage : ContentPage
    {
        private readonly PlayerService _playerService;

        private readonly EmailValidator _emailValidator;
        private readonly PasswordValidator _passwordValidator;

        public MainPage()
        {
            InitializeComponent();

            var dbContext = new DatabaseContext();

            var playerRepository = new PlayerRepository(dbContext.GetDatabase());
            _playerService = new PlayerService(playerRepository);

            _emailValidator = new EmailValidator();
            _passwordValidator = new PasswordValidator();
        }

        private async void OnCreatePlayerClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Ошибка", "Введите email и пароль", "OK");
                return;
            }

            if (!_emailValidator.IsValidEmail(email) || !_passwordValidator.IsValidPassword(password))
            {
                await DisplayAlert("Ошибка", "Введите корректный email или пароль", "OK");
                return;
            }

            try
            {
                var player = await _playerService.CreatePlayerAsync(email, password);
                await DisplayAlert("Успех", "Игрок создан!", "OK");
                EmailEntry.Text = "";
                PasswordEntry.Text = "";

                await Shell.Current.GoToAsync($"/map?id={player.Id}");
            }
            catch (InvalidOperationException ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", "Не удалось создать игрока", "OK");
            }
        }

        private async void OnGetPlayerClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Ошибка", "Введите email и пароль", "OK");
                return;
            }

            if (!_emailValidator.IsValidEmail(email) || !_passwordValidator.IsValidPassword(password))
            {
                await DisplayAlert("Ошибка", "Введите корректный email или password", "OK");
                return;
            }

            var player = await _playerService.GetPlayerByEmailAndPasswordAsync(email, password);
            if (player == null)
            {
                await DisplayAlert("Ошибка", "Игрок с таким email и паролем не найден", "OK");
                return;
            }

            EmailEntry.Text = "";
            PasswordEntry.Text = "";

            await Shell.Current.GoToAsync($"/map?id={player.Id}");
        }
    }
}
