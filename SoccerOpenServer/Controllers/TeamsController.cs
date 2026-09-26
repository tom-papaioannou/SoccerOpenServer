// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.DTO.Teams;
using SoccerOpenServer.Models.Competitions;
using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.People;
using SoccerOpenServer.Models.Teams;
using SoccerOpenServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SoccerOpenServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly SoccerDbContext _db;
        private readonly ITeamAccessService _teamAccessService;

        public TeamsController(SoccerDbContext db, ITeamAccessService teamAccessService)
        {
            _db = db;
            _teamAccessService = teamAccessService;
        }

        [HttpGet("getCurrentTeam")]
        public async Task<ActionResult<TeamInformationDTO>> GetCurrentTeam()
        {
            var team = await _teamAccessService.GetOwnedTeamAsync(User);

            if (team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }

        [HttpGet("{teamID}")]
        public async Task<ActionResult<Team>> GetTeam(Guid teamID)
        {
            var team = await _db.Teams
                .Include(t => t.Competitions)
                .FirstOrDefaultAsync(t => t.TeamID == teamID);

            if (team == null)
                return NotFound();

            return Ok(team);
        }

        [HttpGet("getTeamInformation/{teamID}")]
        public async Task<ActionResult<TeamInformationDTO>> GetTeamInformation(Guid teamID)
        {
            var currentUserID = _teamAccessService.GetCurrentUserID(User);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var team = await _db.Teams
                .Where(t => t.TeamID == teamID)
                .AsNoTracking()
                .Include(t => t.Stadium)
                .Select(t => new TeamInformationDTO
                {
                    TeamID = t.TeamID,
                    Name = t.Name,
                    IsOwned = currentUserID != null && t.AppUserID == currentUserID.Value,
                    LeagueID = t.Competitions!
                        .Where(c => c.CompetitionType == CompetitionType.League)
                        .OrderBy(c => c.Priority)
                        .Select(c => (Guid?)c.CompetitionID)
                        .FirstOrDefault(),
                    LeagueName = t.Competitions!
                        .Where(c => c.CompetitionType == CompetitionType.League)
                        .OrderBy(c => c.Priority)
                        .Select(c => c.CompetitionName)
                        .FirstOrDefault(),
                    ManagerName = _db.Contracts
                        .Where(c =>
                            c.TeamID == t.TeamID &&
                            c.Role == Role.Staff &&
                            (c.EndDate == null || c.EndDate > today) &&
                            c.Person.StaffRole == StaffRole.Manager)
                        .Select(c => ((c.Person.Name ?? "") + " " + (c.Person.Surname ?? "")).Trim())
                        .FirstOrDefault(),
                    ManagerID = _db.Contracts
                        .Where(c =>
                            c.TeamID == t.TeamID &&
                            c.Role == Role.Staff &&
                            (c.EndDate == null || c.EndDate > today) &&
                            c.Person.StaffRole == StaffRole.Manager)
                        .Select(c => (Guid?)c.PersonID)
                        .FirstOrDefault(),
                    Stadium = t.Stadium == null ? null : new StadiumDTO
                    {
                        StadiumID = t.Stadium.StadiumID,
                        Name = t.Stadium.Name,
                        Capacity = t.Stadium.Capacity,
                        City = t.Stadium.City,
                        Latitude = t.Stadium.Latitude,
                        Longitude = t.Stadium.Longitude
                    },
                    Kit = t.Kit
                })
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }

        [HttpPost]
        public async Task<ActionResult<Team>> PostTeam([FromBody] Team team)
        {
            _db.Teams.Add(team);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTeam), new { teamID = team.TeamID }, team);
        }

        [HttpPut("{teamID}")]
        public async Task<IActionResult> UpdateTeam(Guid teamID, [FromBody] Team updatedTeam)
        {
            if (teamID != updatedTeam.TeamID)
                return BadRequest();

            var team = await _db.Teams.FindAsync(teamID);
            if (team == null)
                return NotFound();

            team.Name = updatedTeam.Name;

            _db.Entry(team).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("getTeamSquad/{teamID}")]
        public async Task<IActionResult> GetTeamSquad(Guid teamID)
        {
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.TeamID == teamID);
            if (team == null)
            {
                return NotFound();
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var squad = await _db.Contracts
                .Where(c => c.TeamID == teamID && (c.EndDate == null || c.EndDate > today) && c.Role == Role.Player)
                .Include(c => c.Person)
                    .ThenInclude(p => p.PlayerTrainedPositions)
                .Include(c => c.Person)
                    .ThenInclude(p => p.PlayerTrainedRoles)
                .Include(c => c.Person)
                    .ThenInclude(p => p.PlayerPreferredMoves)
                .Select(c => new
                {
                    c.Person.PersonID,
                    c.Person.Name,
                    c.Person.Surname,
                    c.Person.DateOfBirth,
                    c.Person.NationID,
                    c.ShirtNumber,
                    c.Wage,
                    c.EndDate,
                    PlayerTrainedPositions = c.Person.PlayerTrainedPositions!.Select(ptp => new
                    {
                        ptp.PlayerPosition,
                        ptp.PlayerTrainedPositionAdaptation
                    }),
                    PlayerTrainedRoles = c.Person.PlayerTrainedRoles!.Select(ptr => new
                    {
                        ptr.PlayerPosition,
                        ptr.PlayerRole,
                        ptr.PlayerTrainedRoleAdaptation
                    }),
                    PreferredMoves = c.Person.PlayerPreferredMoves.Select(pm => pm.PreferredMove)
                })
                .ToListAsync();

            return Ok(squad);
        }

        [HttpGet("getTeamStaff/{teamID}")]
        public async Task<IActionResult> GetTeamStaff(Guid teamID)
        {
            var teamExists = await _db.Teams.AnyAsync(t => t.TeamID == teamID);
            if (!teamExists)
            {
                return NotFound();
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var staff = await _db.Contracts
                .AsNoTracking()
                .Where(c =>
                    c.TeamID == teamID &&
                    c.Role == Role.Staff &&
                    (c.EndDate == null || c.EndDate > today) &&
                    (c.Person.StaffRole == StaffRole.Manager ||
                     c.Person.StaffRole == StaffRole.Coach ||
                     c.Person.StaffRole == StaffRole.Medic))
                .OrderBy(c => c.Person.StaffRole == StaffRole.Manager
                    ? 0
                    : c.Person.StaffRole == StaffRole.Coach ? 1 : 2)
                .ThenBy(c => c.Person.Surname)
                .ThenBy(c => c.Person.Name)
                .Select(c => new
                {
                    c.PersonID,
                    c.Person.Name,
                    c.Person.Surname,
                    c.Person.NationID,
                    Role = c.Person.StaffRole == StaffRole.Manager
                        ? "Manager"
                        : c.Person.StaffRole == StaffRole.Coach ? "Coach" : "Medic",
                    c.Wage,
                    c.EndDate
                })
                .ToListAsync();

            return Ok(staff);
        }

        [HttpPut("updatePlayerShirtNumber")]
        public async Task<IActionResult> UpdatePlayerShirtNumber([FromBody] UpdatePlayerShirtNumberDTO model)
        {
            if (!await _teamAccessService.OwnsTeamAsync(User, model.TeamID))
            {
                return Forbid();
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var playerContract = await _db.Contracts
                .FirstOrDefaultAsync(c =>
                    c.TeamID == model.TeamID &&
                    c.PersonID == model.PersonID &&
                    (c.EndDate == null || c.EndDate > today) &&
                    c.Role == Role.Player);

            if (playerContract == null)
            {
                return NotFound();
            }

            var previousAssignedContract = await _db.Contracts
                .FirstOrDefaultAsync(c =>
                    c.TeamID == model.TeamID &&
                    c.PersonID != model.PersonID &&
                    c.ShirtNumber == model.ShirtNumber &&
                    (c.EndDate == null || c.EndDate > today) &&
                    c.Role == Role.Player);

            var playerPreviousShirtNumber = playerContract.ShirtNumber;
            playerContract.ShirtNumber = model.ShirtNumber;

            if (previousAssignedContract != null)
            {
                previousAssignedContract.ShirtNumber = playerPreviousShirtNumber;
            }

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{teamID}/tactic-priorities")]
        public async Task<ActionResult<List<TeamTacticPriorityDto>>> GetTeamTacticPriorities(Guid teamID)
        {
            if (!await _teamAccessService.OwnsTeamAsync(User, teamID))
            {
                return Forbid();
            }

            var priorities = await _db.TeamTacticPriorities
                .AsNoTracking()
                .Where(x => x.TeamID == teamID)
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Priority)
                .Select(x => new TeamTacticPriorityDto
                {
                    TeamTacticPriorityID = x.TeamTacticPriorityID,
                    PersonID = x.PersonID,
                    Type = x.Type,
                    Priority = x.Priority
                })
                .ToListAsync();

            return Ok(priorities);
        }

        [HttpPut("{teamID}/tactic-priorities")]
        public async Task<IActionResult> UpdateTeamTacticPriorities(
            Guid teamID,
            [FromBody] UpdateTeamTacticPrioritiesRequest? request)
        {
            if (request == null)
            {
                return BadRequest("Priority update payload is required.");
            }

            if (!Enum.IsDefined(typeof(TeamTacticPriorityType), request.Type))
            {
                return BadRequest("Invalid tactic priority type.");
            }

            if (!await _teamAccessService.OwnsTeamAsync(User, teamID))
            {
                return Forbid();
            }

            var personIDs = request.PersonIDs;
            if (personIDs.Count != personIDs.Distinct().Count())
            {
                return BadRequest("A player can only appear once in the same priority list.");
            }

            if (!await AllPlayersBelongToTeam(teamID, personIDs))
            {
                return BadRequest("One or more selected players do not belong to this team.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var existingEntries = await _db.TeamTacticPriorities
                    .Where(x => x.TeamID == teamID && x.Type == request.Type)
                    .OrderBy(x => x.Priority)
                    .ToListAsync();

                var requestedPeople = personIDs.ToHashSet();
                var obsoleteEntries = existingEntries
                    .Where(x => !requestedPeople.Contains(x.PersonID))
                    .ToList();

                _db.TeamTacticPriorities.RemoveRange(obsoleteEntries);

                var retainedEntries = existingEntries
                    .Where(x => requestedPeople.Contains(x.PersonID))
                    .ToList();

                MovePrioritiesOutOfTheWay(retainedEntries);
                await _db.SaveChangesAsync();

                foreach (var personID in personIDs)
                {
                    if (retainedEntries.All(x => x.PersonID != personID))
                    {
                        var entry = new TeamTacticPriority
                        {
                            TeamTacticPriorityID = Guid.NewGuid(),
                            TeamID = teamID,
                            Type = request.Type,
                            PersonID = personID,
                            Priority = personIDs.Count + retainedEntries.Count + 1
                        };

                        retainedEntries.Add(entry);
                        _db.TeamTacticPriorities.Add(entry);
                    }
                }

                for (int i = 0; i < personIDs.Count; i++)
                {
                    retainedEntries.First(x => x.PersonID == personIDs[i]).Priority = i + 1;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPatch("{teamID}/tactic-priorities/{type}/primary")]
        public async Task<IActionResult> UpdatePrimaryTeamTacticPriority(
            Guid teamID,
            TeamTacticPriorityType type,
            [FromBody] UpdatePrimaryTeamTacticPriorityRequest? request)
        {
            if (request == null)
            {
                return BadRequest("Primary priority update payload is required.");
            }

            if (!Enum.IsDefined(typeof(TeamTacticPriorityType), type))
            {
                return BadRequest("Invalid tactic priority type.");
            }

            if (!await _teamAccessService.OwnsTeamAsync(User, teamID))
            {
                return Forbid();
            }

            if (!await AllPlayersBelongToTeam(teamID, [request.PersonID]))
            {
                return BadRequest("Selected player does not belong to this team.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var entries = await _db.TeamTacticPriorities
                    .Where(x => x.TeamID == teamID && x.Type == type)
                    .OrderBy(x => x.Priority)
                    .ToListAsync();

                var duplicatePersonEntries = entries
                    .Where(x => x.Priority != 1 && x.PersonID == request.PersonID)
                    .ToList();

                _db.TeamTacticPriorities.RemoveRange(duplicatePersonEntries);
                entries.RemoveAll(x => duplicatePersonEntries.Contains(x));

                var primary = entries.FirstOrDefault(x => x.Priority == 1);
                MovePrioritiesOutOfTheWay(entries);
                await _db.SaveChangesAsync();

                if (primary == null)
                {
                    primary = new TeamTacticPriority
                    {
                        TeamTacticPriorityID = Guid.NewGuid(),
                        TeamID = teamID,
                        Type = type,
                        PersonID = request.PersonID,
                        Priority = 1
                    };

                    entries.Insert(0, primary);
                    _db.TeamTacticPriorities.Add(primary);
                }
                else
                {
                    primary.PersonID = request.PersonID;
                    primary.Priority = 1;
                }

                var nextPriority = 2;
                foreach (var entry in entries.Where(x => x.TeamTacticPriorityID != primary.TeamTacticPriorityID).OrderBy(x => x.Priority))
                {
                    entry.Priority = nextPriority++;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet("getPlayerDetails/{personID}")]
        public async Task<IActionResult> GetPlayerDetails(Guid personID)
        {
            var player = await _db.People
                .Where(p => p.PersonID == personID)
                .Select(p => new {
                    p.Name,
                    p.Surname,
                    p.DateOfBirth,
                    p.PlaceOfBirth,
                    p.NationID,
                    p.Weight,
                    p.Height,
                    Contracts = p.Contracts
                            .OrderByDescending(c => c.EndDate)
                            .Select(c => new {
                                c.StartDate,
                                c.EndDate,
                                c.Wage,
                                Team = new { c.Team.Name }
                    }),
                    p.PlayerStats,
                    p.HealthAndFitness,
                    p.PlayerTrainedPositions,
                    p.PlayerTrainedRoles,
                    PreferredMoves = p.PlayerPreferredMoves.Select(pm => pm.PreferredMove)
                })
                .FirstOrDefaultAsync();

            return Ok(player);
        }

        [HttpGet("getManagerDetails/{personID}")]
        public async Task<IActionResult> GetManagerDetails(Guid personID)
        {
            var manager = await _db.People
                .Where(p => p.PersonID == personID && p.StaffRole == StaffRole.Manager)
                .Select(p => new {
                    p.PersonID,
                    p.Name,
                    p.Surname,
                    p.DateOfBirth,
                    p.PlaceOfBirth,
                    p.NationID,
                    p.Weight,
                    p.Height,
                    p.StaffRole,
                    Contracts = p.Contracts
                        .Where(c => c.Role == Role.Staff)
                        .OrderByDescending(c => c.EndDate)
                        .Select(c => new {
                            c.StartDate,
                            c.EndDate,
                            c.Wage,
                            Team = new { c.Team.Name }
                        })
                })
                .FirstOrDefaultAsync();

            if (manager == null)
            {
                return NotFound();
            }

            return Ok(manager);
        }

        [HttpGet("getCoachDetails/{personID}")]
        public async Task<IActionResult> GetCoachDetails(Guid personID)
        {
            var coach = await _db.People
                .AsNoTracking()
                .Where(p => p.PersonID == personID && p.StaffRole == StaffRole.Coach)
                .Select(p => new
                {
                    p.PersonID,
                    p.Name,
                    p.Surname,
                    p.DateOfBirth,
                    p.PlaceOfBirth,
                    p.NationID,
                    p.Weight,
                    p.Height,
                    CoachStats = p.CoachStats == null ? null : new
                    {
                        p.CoachStats.Attack,
                        p.CoachStats.Defend,
                        p.CoachStats.Control,
                        p.CoachStats.Goalkeeper,
                        p.CoachStats.Tactic,
                        p.CoachStats.Fitness
                    },
                    Contracts = p.Contracts
                        .Where(c => c.Role == Role.Staff)
                        .OrderByDescending(c => c.EndDate)
                        .Select(c => new
                        {
                            c.StartDate,
                            c.EndDate,
                            c.Wage,
                            Team = new { c.Team.Name }
                        })
                })
                .FirstOrDefaultAsync();

            if (coach == null)
            {
                return NotFound();
            }

            return Ok(coach);
        }

        [HttpGet("getStaffDetails/{personID}")]
        public async Task<IActionResult> GetStaffDetails(Guid personID)
        {
            var staffMember = await _db.People
                .AsNoTracking()
                .Where(p =>
                    p.PersonID == personID &&
                    (p.StaffRole == StaffRole.Coach || p.StaffRole == StaffRole.Medic))
                .Select(p => new
                {
                    p.PersonID,
                    p.Name,
                    p.Surname,
                    p.DateOfBirth,
                    p.PlaceOfBirth,
                    p.NationID,
                    p.Weight,
                    p.Height,
                    p.StaffRole,
                    CoachStats = p.StaffRole != StaffRole.Coach || p.CoachStats == null ? null : new
                    {
                        p.CoachStats.Attack,
                        p.CoachStats.Defend,
                        p.CoachStats.Control,
                        p.CoachStats.Goalkeeper,
                        p.CoachStats.Tactic,
                        p.CoachStats.Fitness
                    },
                    MedicStats = p.StaffRole != StaffRole.Medic || p.MedicStats == null ? null : new
                    {
                        p.MedicStats.Diagnosis,
                        p.MedicStats.Treatment,
                        p.MedicStats.Rehabilitation,
                        p.MedicStats.Prevention
                    },
                    Contracts = p.Contracts
                        .Where(c => c.Role == Role.Staff)
                        .OrderByDescending(c => c.EndDate)
                        .Select(c => new
                        {
                            c.StartDate,
                            c.EndDate,
                            c.Wage,
                            Team = new { c.Team.Name }
                        })
                })
                .FirstOrDefaultAsync();

            if (staffMember == null)
            {
                return NotFound();
            }

            return Ok(staffMember);
        }

        [HttpGet("getManagerProfileSummary/{personID}")]
        public async Task<IActionResult> GetManagerProfileSummary(Guid personID)
        {
            var managerExists = await _db.People
                .AnyAsync(p => p.PersonID == personID && p.StaffRole == StaffRole.Manager);

            if (!managerExists)
            {
                return NotFound();
            }

            var stats = await _db.ManagerGameStatsTable
                .AsNoTracking()
                .Where(s => s.PersonID == personID)
                .Select(s => new
                {
                    s.Wins,
                    s.Draws,
                    s.Losses,
                    s.GamesPlayed,
                    s.LeaguesWon,
                    s.CupsWon
                })
                .FirstOrDefaultAsync();

            var favoriteFormation = await _db.ManagerFormationPickedTable
                .AsNoTracking()
                .Where(f => f.PersonID == personID)
                .OrderByDescending(f => f.TimesPicked)
                .ThenBy(f => f.Formation)
                .Select(f => new
                {
                    f.Formation,
                    f.TimesPicked
                })
                .FirstOrDefaultAsync();

            var hasFavoriteFormation = favoriteFormation != null && favoriteFormation.TimesPicked > 0;

            return Ok(new
            {
                GameStats = new
                {
                    Wins = stats?.Wins ?? 0,
                    Draws = stats?.Draws ?? 0,
                    Losses = stats?.Losses ?? 0,
                    GamesPlayed = stats?.GamesPlayed ?? 0,
                    LeaguesWon = stats?.LeaguesWon ?? 0,
                    CupsWon = stats?.CupsWon ?? 0
                },
                FavoriteFormation = hasFavoriteFormation
                    ? new
                    {
                        Formation = favoriteFormation!.Formation,
                        FormationName = favoriteFormation.Formation.ToString(),
                        favoriteFormation.TimesPicked
                    }
                    : null
            });
        }

        [HttpGet("getCurrentTeamDashboard")]
        public async Task<ActionResult<Team>> GetCurrentTeamDashboard()
        {
            var team = await _teamAccessService.GetOwnedTeamAsync(User);
            if (team == null)
            {
                return NotFound();
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            Competition competition = await _db.Competitions
                .Where(c => c.Teams.Any(t => t.TeamID == team.TeamID) && c.CompetitionType == CompetitionType.League)
                .FirstOrDefaultAsync();

            Person[] players = await _db.People
                .Where(p => _db.Contracts.Any(c => c.PersonID == p.PersonID && c.TeamID == team.TeamID && c.EndDate > today && c.Role == Role.Player))
                .Include(p => p.PlayerTrainedPositions)
                .Take(10)
                .ToArrayAsync();

            Formation formation = await _db.Tactics
                .Where(t => t.TeamID == team.TeamID && t.isMain)
                .Select(t => t.Formation ?? Formation.None)
                .FirstOrDefaultAsync();

            var dashboardInformation = new
            {
                TeamName = team.Name,
                CompetitionID = competition?.CompetitionID,
                CompetitionName = competition?.CompetitionName ?? "No active league",
                ManagerID = team.ManagerID,
                ManagerName = team.ManagerName,
                Players = players,
                Formation = formation,
                Kit = team.Kit
            };

            return Ok(dashboardInformation);
        }

        private async Task<bool> AllPlayersBelongToTeam(Guid teamID, IReadOnlyCollection<Guid> personIDs)
        {
            if (personIDs.Count == 0)
            {
                return true;
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var validPlayersCount = await _db.Contracts
                .Where(c =>
                    c.TeamID == teamID &&
                    personIDs.Contains(c.PersonID) &&
                    (c.EndDate == null || c.EndDate > today) &&
                    c.Role == Role.Player)
                .Select(c => c.PersonID)
                .Distinct()
                .CountAsync();

            return validPlayersCount == personIDs.Count;
        }

        private static void MovePrioritiesOutOfTheWay(IReadOnlyList<TeamTacticPriority> entries)
        {
            var offset = entries.Count + 1;

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Priority = offset + i + 1;
            }
        }
    }
}
