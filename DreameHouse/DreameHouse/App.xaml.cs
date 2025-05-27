using DreameHouse.Infrastructure;

namespace DreameHouse
{
    public partial class App : Application
    {
        public static DatabaseContext DbContext { get; set; }
        public App()
        {
            InitializeComponent();

            DbContext = new DatabaseContext();
            Task.Run(async () => await DbContext.InitializeAsync()).Wait();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}