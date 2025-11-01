namespace BordFodboldAPI.Objects
{
    // Default scores are set to 0 making them optional when creating a new match and allows to create an inprogress match
    public record CreateMatchDto(List<int> Team1PlayerIds, List<int> Team2PlayerIds, int Team1Score = 0, int Team2Score = 0);
}
