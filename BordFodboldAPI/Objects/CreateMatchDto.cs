namespace BordFodboldAPI.Objects
{
    public record CreateMatchDto(List<int> Team1PlayerIds, List<int> Team2PlayerIds);
}
