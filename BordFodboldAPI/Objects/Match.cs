using BordFodboldAPI.Exceptions;

namespace BordFodboldAPI.Objects
{
    public class Match
    {
        public int Id { get; set; }
        public int Team1Player1Id { get; set; }
        public int Team1Player2Id { get; set; }
        public int Team2Player1Id { get; set; }
        public int Team2Player2Id { get; set; }

        public Match(int team1Player1Id, int team1Player2Id, int team2Player1Id, int team2Player2Id)
        {
            Team1Player1Id = team1Player1Id;
            Team1Player2Id = team1Player2Id;
            Team2Player1Id = team2Player1Id;
            Team2Player2Id = team2Player2Id;

        }


        public static Match CreateMatch(List<Player> team1, List<Player> team2)
        {
            if (team1.Count != 2 || team2.Count != 2)
            {
                throw new InvalidTeamSizeException($"Each team must have exactly 2 players." +
                    $"\nTeam 1 Player count: {team1.Count}" +
                    $"\nTeam 2 Player count: {team2.Count}");
            }
            if (team1.Intersect(team2).Any())
            {
                throw new PlayerOnBothTeamsException("A player cannot be in both teams.");
            }

            return new Match(
                team1[0].Id,
                team1[1].Id,
                team2[0].Id,
                team2[1].Id
            );

        }
    }
}
