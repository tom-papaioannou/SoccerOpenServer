// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.DTO.Tactics;
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
    public class TacticsController : ControllerBase
    {
        private SoccerDbContext _db;
        private readonly ITeamAccessService _teamAccessService;
        private readonly ITeamGenerationService _teamGenerationService;

        public TacticsController(SoccerDbContext db, ITeamAccessService teamAccessService, ITeamGenerationService teamGenerationService)
        {
            _db = db;
            _teamAccessService = teamAccessService;
            _teamGenerationService = teamGenerationService;
        }

        [HttpGet("getTeamTactic/{tacticID}")]
        public async Task<IActionResult> GetTeamTactic(Guid tacticID)
        {
            Tactic? teamTactic = await _db.Tactics.FirstOrDefaultAsync(tactic => tactic.TacticID == tacticID);

            if(teamTactic == null)
            {
                return NotFound("Tactic not found.");
            }

            var team = await _teamAccessService.GetOwnedTeamAsync(User);

            if (team == null || team.TeamID != teamTactic.TeamID)
            {
                return NotFound("Team not found or user does not have access to this tactic.");
            }

            return Ok(teamTactic);
        }

        [HttpGet("getTeamTactics")]
        public async Task<IActionResult> GetTeamTactics()
        {
            var team = await _teamAccessService.GetOwnedTeamAsync(User);

            if (team == null)
            {
                return NotFound("Team not found.");
            }

            List<Tactic> teamTactics = await _db.Tactics
                .Where(tactic => tactic.TeamID == team.TeamID)
                .ToListAsync();

            return Ok(teamTactics);
        }

        [HttpPost("createTeamTactic")]
        public async Task<IActionResult> CreateTeamTactic([FromBody] Tactic newTactic)
        {
            if (newTactic.Formation == null ||
                !Enum.IsDefined(typeof(Formation), newTactic.Formation.Value) ||
                newTactic.Formation == Formation.None)
            {
                return BadRequest("Invalid formation.");
            }

            if (!Enum.IsDefined(typeof(TacticMentality), newTactic.TacticMentality) || newTactic.TacticMentality == TacticMentality.None)
            {
                return BadRequest("Invalid tactic mentality.");
            }

            if (!Enum.IsDefined(typeof(PassingMentality), newTactic.PassingMentality) || newTactic.PassingMentality == PassingMentality.None)
            {
                return BadRequest("Invalid passing mentality.");
            }

            if (!Enum.IsDefined(typeof(CrossingMentality), newTactic.CrossingMentality) || newTactic.CrossingMentality == CrossingMentality.None)
            {
                return BadRequest("Invalid crossing mentality.");
            }

            var team = await _teamAccessService.GetOwnedTeamAsync(User);

            if (team == null || team.TeamID != newTactic.TeamID)
            {
                return NotFound("Team not found or user does not have access to this team.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                newTactic.TacticID = Guid.NewGuid();

                // Get all existing tactics for this team
                var existingTactics = await _db.Tactics
                    .Where(t => t.TeamID == newTactic.TeamID)
                    .ToListAsync();

                // If this is the first tactic for the team, always set it as main
                if (!existingTactics.Any())
                {
                    newTactic.isMain = true;
                }
                // If the new tactic is marked as main, set all other tactics to not main
                else if (newTactic.isMain)
                {
                    foreach (var tactic in existingTactics)
                    {
                        tactic.isMain = false;
                    }
                }

                _db.Tactics.Add(newTactic);
                await _db.SaveChangesAsync();
                await _teamGenerationService.AssignPlayersToTactic(newTactic.TacticID, newTactic.TeamID, newTactic.Formation);
                await transaction.CommitAsync();

                return Ok(newTactic);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpDelete("deleteTeamTactic/{tacticID}")]
        public async Task<IActionResult> DeleteTeamTactic(Guid tacticID)
        {
            Tactic? tactic = await _db.Tactics.FindAsync(tacticID);

            if (tactic == null)
            {
                return NotFound("Tactic not found.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Check if there are other tactics for this team
                var otherTacticsCount = await _db.Tactics
                    .CountAsync(t => t.TeamID == tactic.TeamID && t.TacticID != tacticID);

                if (otherTacticsCount == 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Cannot delete the last tactic of a team. A team must have at least one tactic.");
                }

                // If the deleted tactic is main, promote another tactic to main
                if (tactic.isMain)
                {
                    var newMainTactic = await _db.Tactics
                        .Where(t => t.TeamID == tactic.TeamID && t.TacticID != tacticID)
                        .OrderBy(t => t.TacticID)
                        .FirstOrDefaultAsync();

                    if (newMainTactic != null)
                    {
                        newMainTactic.isMain = true;
                    }
                }

                _db.Tactics.Remove(tactic);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok();
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("updateTeamTactic/{tacticID}")]
        public async Task<IActionResult> UpdateTeamTactic(Guid tacticID, [FromBody] UpdateTacticDTO updateTacticModel)
        {
            if (updateTacticModel == null)
            {
                return BadRequest("Tactic update payload is required.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!Enum.IsDefined(typeof(Formation), updateTacticModel.Formation) || updateTacticModel.Formation == Formation.None)
            {
                return BadRequest("Invalid formation.");
            }

            if (!Enum.IsDefined(typeof(TacticMentality), updateTacticModel.TacticMentality) || updateTacticModel.TacticMentality == TacticMentality.None)
            {
                return BadRequest("Invalid tactic mentality.");
            }

            if (!Enum.IsDefined(typeof(PassingMentality), updateTacticModel.PassingMentality) || updateTacticModel.PassingMentality == PassingMentality.None)
            {
                return BadRequest("Invalid passing mentality.");
            }

            if (!updateTacticModel.AttackLeft &&
                !updateTacticModel.AttackMiddle &&
                !updateTacticModel.AttackRight)
            {
                return BadRequest("At least one attack direction must be selected.");
            }

            Tactic? tactic = await _db.Tactics.FirstOrDefaultAsync(t => t.TacticID == tacticID);
            if (tactic == null)
            {
                return NotFound("Tactic not found.");
            }

            var team = await _teamAccessService.GetOwnedTeamAsync(User);
            if (team == null || team.TeamID != tactic.TeamID)
            {
                return NotFound("Team not found or user does not have access to this tactic.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                Formation? previousFormation = tactic.Formation;

                tactic.Name = updateTacticModel.Name.Trim();
                tactic.Formation = updateTacticModel.Formation;
                tactic.TacticMentality = updateTacticModel.TacticMentality;
                tactic.PassingMentality = updateTacticModel.PassingMentality;
                tactic.CrossingMentality = updateTacticModel.CrossingMentality; 
                tactic.AttackLeft = updateTacticModel.AttackLeft;
                tactic.AttackMiddle = updateTacticModel.AttackMiddle;
                tactic.AttackRight = updateTacticModel.AttackRight;
                tactic.EarlyCrosses = updateTacticModel.EarlyCrosses;
                tactic.OffsideTrap = updateTacticModel.OffsideTrap;

                bool formationChanged = previousFormation != tactic.Formation;

                if (updateTacticModel.isMain)
                {
                    List<Tactic> teamTactics = await _db.Tactics
                        .Where(t => t.TeamID == team.TeamID)
                        .ToListAsync();

                    foreach (var teamTactic in teamTactics)
                    {
                        teamTactic.isMain = teamTactic.TacticID == tacticID;
                    }
                }
                else
                {
                    tactic.isMain = false;

                    bool hasOtherMainTactic = await _db.Tactics
                        .AnyAsync(t => t.TeamID == team.TeamID && t.TacticID != tacticID && t.isMain);

                    if (!hasOtherMainTactic)
                    {
                        tactic.isMain = true;
                    }
                }

                await _db.SaveChangesAsync();

                if (formationChanged)
                {
                    await _teamGenerationService.AssignPlayersToTactic(tactic.TacticID, tactic.TeamID, tactic.Formation);
                }

                await transaction.CommitAsync();

                return Ok(tactic);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet("getPlayerTactics/{tacticID}")]
        public async Task<IActionResult> GetPlayerTactics(Guid tacticID)
        {
            List<PlayerTactic> playerTactics = await _db.PlayerTactics
                .Where(pt => pt.TacticID == tacticID)
                .Include(p => p.Person)
                    .ThenInclude(p => p!.PlayerTrainedPositions)
                .Include(p => p.Person)
                    .ThenInclude(p => p!.PlayerTrainedRoles)
                .ToListAsync();

            return Ok(playerTactics);
        }

        [HttpPatch("teams/{teamID}/starting-player-tactics/{playerTacticID}/role")]
        public async Task<IActionResult> UpdateStartingPlayerRole(
            Guid teamID,
            Guid playerTacticID,
            [FromBody] UpdatePlayerTacticRoleDTO? updatePlayerRoleModel)
        {
            if (updatePlayerRoleModel == null)
            {
                return BadRequest("Player role update payload is required.");
            }

            if (!Enum.IsDefined(typeof(PlayerRole), updatePlayerRoleModel.PlayerRole))
            {
                return BadRequest("Invalid player role.");
            }

            bool ownsTeam = await _teamAccessService.OwnsTeamAsync(User, teamID);
            if (!ownsTeam)
            {
                return NotFound("Team not found or user does not have access to this team.");
            }

            PlayerTactic? playerTactic = await _db.PlayerTactics
                .Include(pt => pt.Person)
                    .ThenInclude(p => p!.PlayerTrainedPositions)
                .Include(pt => pt.Person)
                    .ThenInclude(p => p!.PlayerTrainedRoles)
                .FirstOrDefaultAsync(pt => pt.PlayerTacticID == playerTacticID);

            if (playerTactic == null)
            {
                return NotFound("Player tactic not found.");
            }

            bool tacticBelongsToTeam = await _db.Tactics
                .AnyAsync(t => t.TacticID == playerTactic.TacticID && t.TeamID == teamID);

            if (!tacticBelongsToTeam)
            {
                return BadRequest("Player tactic does not belong to the given team.");
            }

            if (playerTactic.SquadUnit != SquadUnit.Starting)
            {
                return BadRequest("Only starting squad player roles can be updated.");
            }

            if (!IsRoleAvailableForPosition(playerTactic.PlayerPosition, updatePlayerRoleModel.PlayerRole))
            {
                return BadRequest("Player role is not available for the player's current position.");
            }

            DateOnly now = DateOnly.FromDateTime(DateTime.Now);
            bool playerBelongsToTeam = await _db.Contracts.AnyAsync(c =>
                c.PersonID == playerTactic.PersonID &&
                c.TeamID == teamID &&
                (c.EndDate == null || c.EndDate > now) &&
                c.Role == Role.Player);

            if (!playerBelongsToTeam)
            {
                return BadRequest("Player does not belong to the given team.");
            }

            playerTactic.PlayerRole = updatePlayerRoleModel.PlayerRole;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }

            return Ok(playerTactic);
        }

        [HttpGet("getPlayerTacticsByTeamID/{teamID}")]
        public async Task<IActionResult> GetPlayerTacticsByTeamID(Guid teamID)
        {
            // Use a join to get all player tactics for the team's tactics in a single query
            List<PlayerTactic> playerTactics = await _db.PlayerTactics
                .Where(pt => _db.Tactics
                    .Where(t => t.TeamID == teamID)
                    .Select(t => t.TacticID)
                    .Contains(pt.TacticID))
                .ToListAsync();

            return Ok(playerTactics);
        }

        [HttpPost("addPlayerTactic")]
        public async Task<IActionResult> AddPlayerTactic([FromBody] PlayerTactic newPlayerTactic)
        {
            PlayerTactic? alreadySamePlayerTactic = await _db.PlayerTactics
                .Where(pt => pt.TacticID == newPlayerTactic.TacticID && pt.PlayerPosition == newPlayerTactic.PlayerPosition)
                .FirstOrDefaultAsync();

            if (alreadySamePlayerTactic != null)
            {
                _db.PlayerTactics.Remove(alreadySamePlayerTactic);
            }

            _db.PlayerTactics.Add(newPlayerTactic);
            await _db.SaveChangesAsync();

            return Ok(newPlayerTactic);
        }

        [HttpPost("swapPlayersTactics")]
        public async Task<IActionResult> SwapPlayersTactic([FromBody] SwapPlayerTacticsDTO swapPlayerTacticsModel)
        {
            List<PlayerTactic>? playerTactics = await _db.PlayerTactics
                .Where(pt => pt.PlayerTacticID == swapPlayerTacticsModel.FirstPersonTacticID || pt.PlayerTacticID == swapPlayerTacticsModel.SecondPersonTacticID)
                .ToListAsync();

            if (playerTactics == null || playerTactics.Count != 2)
            {
                return NotFound("Could not find tactics for players.");
            }

            PlayerTactic firstPlayerTactic = playerTactics.FirstOrDefault(pt => pt.PlayerTacticID == swapPlayerTacticsModel.FirstPersonTacticID);
            PlayerTactic secondPlayerTactic = playerTactics.FirstOrDefault(pt => pt.PlayerTacticID == swapPlayerTacticsModel.SecondPersonTacticID);

            Guid temp = firstPlayerTactic.PersonID;
            firstPlayerTactic.PersonID = secondPlayerTactic.PersonID;
            secondPlayerTactic.PersonID = temp;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }

            return Ok();
        }

        private static bool IsRoleAvailableForPosition(PlayerPosition position, PlayerRole role)
        {
            return position switch
            {
                PlayerPosition.Goalkeeper => role is
                    PlayerRole.Goalkeeper or
                    PlayerRole.SweeperKeeper,

                PlayerPosition.RightCenterBack or
                PlayerPosition.CentralCenterBack or
                PlayerPosition.LeftCenterBack => role is
                    PlayerRole.CenterBack or
                    PlayerRole.BallPlayingDefender or
                    PlayerRole.NoNonsenseCenterBack or
                    PlayerRole.Libero or
                    PlayerRole.Stopper or
                    PlayerRole.Cover,

                PlayerPosition.RightBack or
                PlayerPosition.LeftBack or
                PlayerPosition.RightWingBack or
                PlayerPosition.LeftWingBack => role is
                    PlayerRole.FullBack or
                    PlayerRole.WingBack or
                    PlayerRole.CompleteWingBack or
                    PlayerRole.InvertedWingBack or
                    PlayerRole.WideCenterBack,

                PlayerPosition.RightDefensiveMidfielder or
                PlayerPosition.CentralDefensiveMidfielder or
                PlayerPosition.LeftDefensiveMidfielder => role is
                    PlayerRole.DefensiveMidfielder or
                    PlayerRole.Anchorman or
                    PlayerRole.HalfBack or
                    PlayerRole.DeepLyingPlaymaker or
                    PlayerRole.Regista or
                    PlayerRole.Volante or
                    PlayerRole.SegundoVolante or
                    PlayerRole.BallWinningMidfielder,

                PlayerPosition.RightCenterMidfielder or
                PlayerPosition.CentralCenterMidfielder or
                PlayerPosition.LeftCenterMidfielder => role is
                    PlayerRole.CentralMidfielder or
                    PlayerRole.BoxToBoxMidfielder or
                    PlayerRole.Mezzala or
                    PlayerRole.Carrilero or
                    PlayerRole.AdvancedPlaymaker or
                    PlayerRole.RoamingPlaymaker,

                PlayerPosition.RightMidfielder or
                PlayerPosition.LeftMidfielder or
                PlayerPosition.RightWinger or
                PlayerPosition.LeftWinger => role is
                    PlayerRole.WideMidfielder or
                    PlayerRole.WidePlaymaker or
                    PlayerRole.Winger or
                    PlayerRole.InvertedWinger or
                    PlayerRole.InsideForward or
                    PlayerRole.InvertedForward or
                    PlayerRole.Raumdeuter or
                    PlayerRole.WideTargetMan or
                    PlayerRole.DefensiveWinger,

                PlayerPosition.RightAttackingMidfielder or
                PlayerPosition.CentralAttackingMidfielder or
                PlayerPosition.LeftAttackingMidfielder => role is
                    PlayerRole.AttackingMidfielder or
                    PlayerRole.ShadowStriker or
                    PlayerRole.Enganche or
                    PlayerRole.Trequartista or
                    PlayerRole.SecondStriker or
                    PlayerRole.FalseTen or
                    PlayerRole.CentralWinger,

                PlayerPosition.RightStriker or
                PlayerPosition.CentralStriker or
                PlayerPosition.LeftStriker => role is
                    PlayerRole.AdvancedForward or
                    PlayerRole.CompleteForward or
                    PlayerRole.Poacher or
                    PlayerRole.TargetMan or
                    PlayerRole.DeepLyingForward or
                    PlayerRole.PressingForward or
                    PlayerRole.DefensiveForward or
                    PlayerRole.FalseNine or
                    PlayerRole.TrequartistaForward,

                _ => false
            };
        }
    }
}
