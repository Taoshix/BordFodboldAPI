namespace BordFodboldAPI.Objects
{
    // Default handicap is set to 10 making it optional when creating a new player
    public record CreatePlayerDto(string Name, string Initials, int Handicap = 10);
}