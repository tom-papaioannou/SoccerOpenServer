using SoccerOpenServer.Models.People;

namespace SoccerOpenServer.DTO.Nations;

public sealed class NationDetailsDTO
{
    public Guid NationID { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ISO2 { get; init; } = string.Empty;
    public string? ISO3 { get; init; }
    public string? FlagUrl { get; init; }
    public Guid ContinentID { get; init; }
    public IReadOnlyList<NationCompetitionDTO> Competitions { get; init; } = [];
    public IReadOnlyList<NationSquadPlayerDTO> Squad { get; init; } = [];
}

public sealed class NationCompetitionDTO
{
    public Guid CompetitionID { get; init; }
    public string? CompetitionName { get; init; }
    public Guid? NationID { get; init; }
    public int Priority { get; init; }
    public int CompetitionType { get; init; }
    public int CompetitionTeamsType { get; init; }
    public int TeamsCount { get; init; }
}

public sealed class NationSquadPlayerDTO
{
    public Guid PersonID { get; init; }
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public Guid? NationID { get; init; }
    public DateOnly? EndDate { get; init; }
    public byte? ShirtNumber { get; init; }
    public int Wage { get; init; }
    public IReadOnlyList<PlayerTrainedPosition> PlayerTrainedPositions { get; init; } = [];
    public IReadOnlyList<PlayerTrainedRole> PlayerTrainedRoles { get; init; } = [];
}
