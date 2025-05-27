namespace Domain.Entities;

public enum TileType
{
    CAT,
    FLOWER,
    LAMP,
    BRICK,
    WREATH,
    MUG,
    PILLOW,
    HOME,
    BOMB,
    ROCKET
}

public class Tile
{
    public TileType Type { get; set; }

    public Tile(TileType type)
    {
        Type = type;
    }

    public override string ToString() => Type.ToString();
}
