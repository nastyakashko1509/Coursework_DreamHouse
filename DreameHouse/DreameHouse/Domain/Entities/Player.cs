using SQLite;

namespace DreameHouse.Domain.Entities
{
    [Table("Players")]
    public class Player
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public int level { get; set; }
        public int bitcoin { get; set; }
        public int task { get; set; }
    }
}
