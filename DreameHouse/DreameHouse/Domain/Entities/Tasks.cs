using SQLite;

namespace DreameHouse.Domain.Entities
{
    public class Tasks
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? Reward { get; set; }
        public int PriceBitcoin { get; set; }
    }
}
