using BordFodboldAPI.Exceptions;

namespace BordFodboldAPI.Objects
{
    public class Match
    {
        public int Id { get; set; } // Primary key for database
        public List<Player> Team1 { get; set; }
        public List<Player> Team2 { get; set; }
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }

        public Match(List<Player> team1, List<Player> team2, int team1Score = 0, int team2Score = 0)
        {
            if (team1.Count != 2 || team2.Count != 2)
            {
                throw new TooManyPlayersException($"Each team must have exactly 2 players." +
                    $"\nTeam 1 Player count: {team1.Count}" +
                    $"\nTeam 2 Player count: {team2.Count}");
            }
            if (team1.Intersect(team2).Any())
            {
                throw new PlayerOnBothTeamsException("A player cannot be in both teams.");
            }

            Team1 = team1;
            Team2 = team2;
            Team1Score = team1Score;
            Team2Score = team2Score;
        }
    }
}
