namespace BordFodboldAPI.Objects
{
    // Default scores are set to 0 making them optional when creating a new match
    public record CreateMatchDto(List<int> Team1PlayerIds, List<int> Team2PlayerIds);
}
