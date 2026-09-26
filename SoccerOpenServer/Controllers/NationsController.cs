// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.Models.Competitions;
using SoccerOpenServer.Models.World;
using SoccerOpenServer.DTO.Nations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace SoccerOpenServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NationsController : ControllerBase
    {
        private readonly SoccerDbContext _context;

        public NationsController(SoccerDbContext context)
        {
            _context = context;
        }

        [HttpGet("{nationID}/details")]
        public async Task<ActionResult<NationDetailsDTO>> GetDetails(Guid nationID)
        {
            var nation = await _context.Nations
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NationID == nationID);

            if (nation == null)
                return NotFound();

            var competitions = await _context.Competitions
                .AsNoTracking()
                .Where(c => c.NationID == nationID)
                .Select(c => new NationCompetitionDTO
                {
                    CompetitionID = c.CompetitionID,
                    CompetitionName = c.CompetitionName,
                    NationID = c.NationID,
                    Priority = c.Priority,
                    CompetitionType = (int)c.CompetitionType,
                    CompetitionTeamsType = (int)c.CompetitionTeamsType,
                    TeamsCount = c.Teams == null ? 0 : c.Teams.Count
                })
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var nationalTeamID = await _context.Teams
                .Where(t => t.IsNationalTeam && t.NationID == nationID)
                .Select(t => (Guid?)t.TeamID)
                .SingleOrDefaultAsync();

            var squad = new List<NationSquadPlayerDTO>();
            if (nationalTeamID.HasValue)
            {
                var contracts = await _context.Contracts
                    .AsNoTracking()
                    .Where(c => c.TeamID == nationalTeamID.Value && c.Role == Models.Contracts.Role.Player &&
                                (c.EndDate == null || c.EndDate >= today))
                    .Include(c => c.Person)
                        .ThenInclude(p => p.PlayerTrainedPositions)
                    .Include(c => c.Person)
                        .ThenInclude(p => p.PlayerTrainedRoles)
                    .ToListAsync();

                squad = contracts
                    .GroupBy(c => c.PersonID)
                    .Select(g => g.OrderByDescending(c => c.StartDate).First())
                    .Select(c => new NationSquadPlayerDTO
                    {
                        PersonID = c.PersonID,
                        Name = c.Person.Name,
                        Surname = c.Person.Surname,
                        DateOfBirth = c.Person.DateOfBirth,
                        NationID = c.Person.NationID,
                        EndDate = c.EndDate,
                        ShirtNumber = c.ShirtNumber,
                        Wage = c.Wage,
                        PlayerTrainedPositions = c.Person.PlayerTrainedPositions?.ToList() ?? [],
                        PlayerTrainedRoles = c.Person.PlayerTrainedRoles?.ToList() ?? []
                    })
                    .OrderBy(p => p.Surname)
                    .ThenBy(p => p.Name)
                    .ToList();
            }

            return Ok(new NationDetailsDTO
            {
                NationID = nation.NationID,
                Name = nation.Name,
                ISO2 = nation.ISO2,
                ISO3 = nation.ISO3,
                FlagUrl = nation.FlagUrl,
                ContinentID = nation.ContinentID,
                Competitions = competitions,
                Squad = squad
            });
        }

        [HttpGet("getAllContinents")]
        public async Task<IActionResult> GetAllContinents()
        {
            var continents = await _context.Continents.ToListAsync();


            if (continents == null)
                return NotFound();

            return Ok(continents);
        }

        [HttpGet("getAllCompetitionParents")]
        public async Task<ActionResult<Nation>> GetAllCompetitionParents()
        {
            var nations = await _context.Nations.ToListAsync();

            if (nations == null)
                return NotFound();

            return Ok(nations);
        }

        [HttpGet("{competitionParentID}")]
        public async Task<ActionResult<Nation>> GetCompetitionParent(Guid competitionParentID)
        {
            var nation = await _context.Nations
                .FirstOrDefaultAsync(cp => cp.NationID == competitionParentID);

            if (nation == null)
                return NotFound();

            return Ok(nation);
        }

        //[HttpPost]
        //public async Task<ActionResult<Nation>> PostCompetitionParent([FromBody] CreateCompetitionParentRequest request)
        //{
        //    var nation = new Nation
        //    {
        //        NationID = Guid.NewGuid(),
        //        Name = request.Name
        //    };

        //    _context.Nations.Add(nation);
        //    await _context.SaveChangesAsync();
        //    return CreatedAtAction(nameof(GetCompetitionParent), new { nationID = nation.NationID }, nation);
        //}

        //[HttpPut("{competitionParentID}")]
        //public async Task<IActionResult> UpdateCompetitionParent(Guid competitionParentID, [FromBody] CompetitionParent updatedCompetitionParent)
        //{
        //    if (competitionParentID != updatedCompetitionParent.CompetitionParentID)
        //        return BadRequest("The ID in the URL does not match the ID in the request body");

        //    var competitionParent = await _context.CompetitionParents.FindAsync(competitionParentID);
        //    if (competitionParent == null)
        //        return NotFound();

        //    competitionParent.Name = updatedCompetitionParent.Name;
        //    competitionParent.CompetitionParentType = updatedCompetitionParent.CompetitionParentType;
        //    competitionParent.NumberOfLeagues = updatedCompetitionParent.NumberOfLeagues;
        //    competitionParent.NumberOfCups = updatedCompetitionParent.NumberOfCups;
        //    competitionParent.NumberOfNationalLeagues = updatedCompetitionParent.NumberOfNationalLeagues;
        //    competitionParent.NumberOfNationalCups = updatedCompetitionParent.NumberOfNationalCups;
        //    competitionParent.NationalTeamID = updatedCompetitionParent.NationalTeamID;

        //    _context.Entry(competitionParent).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //[HttpDelete("{competitionParentID}")]
        //public async Task<IActionResult> UpdateCompetitionParent(Guid competitionParentID)
        //{
        //    var competitionParent = await _context.CompetitionParents.FindAsync(competitionParentID);
        //    if (competitionParent == null)
        //        return NotFound();

        //    _context.Entry(competitionParent).State = EntityState.Deleted;
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
    }
}
