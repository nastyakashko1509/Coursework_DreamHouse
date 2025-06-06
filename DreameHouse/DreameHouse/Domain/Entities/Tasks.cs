using LiteDB;
using SQLite;

namespace DreameHouse.Domain.Entities
{
    public class Tasks
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? Reward { get; set; }
        public int PriceBitcoin { get; set; }
    }
}
