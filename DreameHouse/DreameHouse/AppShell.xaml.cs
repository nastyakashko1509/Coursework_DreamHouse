using DreameHouse.Infrastructure;

namespace DreameHouse
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("house_one", typeof(HouseOnePage));
            Routing.RegisterRoute("map", typeof(MapPage));
            Routing.RegisterRoute("room", typeof(RoomPage));
            Routing.RegisterRoute("match3board", typeof(Match3BoardPage));
        }
    }
}
