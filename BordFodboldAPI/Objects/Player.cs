namespace BordFodboldAPI.Objects
{
    public class Player
    {
        public int Id { get; set; } // Primary key for database
        public string Name { get; set; }
        public string Initials { get; set; }
        public int Handicap { get; set; }

        public Player(string name, string initials, int handicap)
        {
            Name = name;
            Initials = initials;
            Handicap = handicap;
        }
    }
}
