using Domain.Entities;
using SQLite;

namespace DreameHouse.Domain.Entities
{
    public class Level
    {
        public int Number { get; set; } 
        public int RewardBitcoin { get; set; } 
        public int MaxMoves { get; set; }
        public TileType? TargetType { get; set; } 
        public int TargetCount { get; set; } 
    }
}
