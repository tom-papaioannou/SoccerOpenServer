using Microsoft.EntityFrameworkCore;
using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.Teams;
using SoccerOpenServer.Models.World;

namespace SoccerOpenServer.Services;

public sealed class NationalTeamGenerationService
{
    private readonly SoccerDbContext _context;
    private readonly ILogger<NationalTeamGenerationService> _logger;

    public NationalTeamGenerationService(SoccerDbContext context, ILogger<NationalTeamGenerationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnsureNationalSquadsAsync(CancellationToken cancellationToken = default)
    {
        var nations = await _context.Nations.AsNoTracking().ToListAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var nation in nations)
        {
            var nationalTeam = await EnsureNationalTeamAsync(nation, cancellationToken);
            var selectedPlayerIds = await _context.Contracts
                .Where(c => c.TeamID == nationalTeam.TeamID && c.Role == Role.Player &&
                            (c.EndDate == null || c.EndDate >= today))
                .Select(c => c.PersonID)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (selectedPlayerIds.Count >= 30)
            {
                continue;
            }

            var eligiblePlayers = await _context.People
                .Where(p => p.NationID == nation.NationID &&
                            p.PlayerStats != null &&
                            !selectedPlayerIds.Contains(p.PersonID))
                .Select(p => p.PersonID)
                .ToListAsync(cancellationToken);

            var playersToAdd = NationalSquadSelector
                .SelectUnique(eligiblePlayers, 30 - selectedPlayerIds.Count, Random.Shared)
                .ToList();

            foreach (var personId in playersToAdd)
            {
                _context.Contracts.Add(new Contract
                {
                    ContractID = Guid.NewGuid(),
                    PersonID = personId,
                    TeamID = nationalTeam.TeamID,
                    StartDate = today,
                    EndDate = today.AddYears(1),
                    Role = Role.Player,
                    Wage = 0
                });
            }

            if (selectedPlayerIds.Count + playersToAdd.Count < 30)
            {
                _logger.LogWarning(
                    "Nation {Nation} has only {Count} eligible players; generated national squad contains all eligible players.",
                    nation.Name,
                    selectedPlayerIds.Count + playersToAdd.Count);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Team> EnsureNationalTeamAsync(Nation nation, CancellationToken cancellationToken)
    {
        var existing = await _context.Teams
            .SingleOrDefaultAsync(t => t.IsNationalTeam && t.NationID == nation.NationID, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var team = new Team
        {
            TeamID = Guid.NewGuid(),
            Name = nation.Name,
            Code = nation.ISO2,
            NationID = nation.NationID,
            IsNationalTeam = true,
            Stadium = new Stadium
            {
                StadiumID = Guid.NewGuid(),
                Name = $"{nation.Name} National Stadium",
                City = nation.Name,
                Capacity = 50000
            },
            Kit = new Kit
            {
                KitID = Guid.NewGuid(),
                HomeShirtColor = "#1F4E79",
                HomeShortsColor = "#FFFFFF",
                AwayShirtColor = "#FFFFFF",
                AwayShortsColor = "#1F4E79",
                KitShape = KitShapeEnum.Empty
            },
            Contracts = new List<Contract>(),
            Competitions = new List<SoccerOpenServer.Models.Competitions.Competition>()
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);
        return team;
    }
}
