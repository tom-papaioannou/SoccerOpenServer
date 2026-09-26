namespace SoccerOpenServer.Services;

public static class NationalSquadSelector
{
    public static IReadOnlyList<Guid> SelectUnique(IEnumerable<Guid> eligiblePlayerIds, int squadSize, Random random)
    {
        if (squadSize <= 0)
            return [];

        return eligiblePlayerIds
            .Distinct()
            .OrderBy(_ => random.Next())
            .Take(squadSize)
            .ToList();
    }
}
