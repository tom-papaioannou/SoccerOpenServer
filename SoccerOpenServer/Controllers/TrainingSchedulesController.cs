// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerOpenServer.DTO.Training;
using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.People;
using SoccerOpenServer.Models.Training;
using SoccerOpenServer.Services;

namespace SoccerOpenServer.Controllers
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/training-schedules")]
    public class TrainingSchedulesController : ControllerBase
    {
        private const int MaximumSliderPoints = 20;
        private readonly SoccerDbContext _db;
        private readonly ITeamAccessService _teamAccessService;

        public TrainingSchedulesController(SoccerDbContext db, ITeamAccessService teamAccessService)
        {
            _db = db;
            _teamAccessService = teamAccessService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingScheduleDTO>>> GetTeamTrainingSchedules()
        {
            var team = await _teamAccessService.GetOwnedTeamAsync(User);
            if (team == null)
            {
                return NotFound("Team not found.");
            }

            var schedules = await _db.TrainingSchedules
                .AsNoTracking()
                .Where(schedule => schedule.TeamID == team.TeamID)
                .OrderBy(schedule => schedule.ScheduleName)
                .ThenBy(schedule => schedule.TrainingScheduleID)
                .Select(schedule => ToDto(schedule))
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpGet("coaches")]
        public async Task<ActionResult<IEnumerable<TrainingScheduleCoachDTO>>> GetTeamCoaches()
        {
            var team = await _teamAccessService.GetOwnedTeamAsync(User);
            if (team == null)
            {
                return NotFound("Team not found.");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var coaches = await _db.Contracts
                .AsNoTracking()
                .Where(contract =>
                    contract.TeamID == team.TeamID &&
                    contract.Role == Role.Staff &&
                    (contract.EndDate == null || contract.EndDate > today) &&
                    contract.Person.StaffRole == StaffRole.Coach)
                .OrderBy(contract => contract.Person.Surname)
                .ThenBy(contract => contract.Person.Name)
                .Select(contract => new TrainingScheduleCoachDTO
                {
                    PersonID = contract.PersonID,
                    Name = ((contract.Person.Name ?? "") + " " + (contract.Person.Surname ?? "")).Trim()
                })
                .ToListAsync();

            return Ok(coaches);
        }

        [HttpPost]
        public async Task<ActionResult<TrainingScheduleDTO>> CreateTrainingSchedule(
            [FromBody] CreateTrainingScheduleDTO request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var scheduleName = request.ScheduleName.Trim();
            if (string.IsNullOrWhiteSpace(scheduleName))
            {
                return BadRequest("Schedule name is required.");
            }

            if (!Enum.IsDefined(request.TrainingScheduleLevel))
            {
                return BadRequest("Invalid training schedule level.");
            }

            var totalSliderPoints = request.AttackPoints
                + request.DefendPoints
                + request.ControlPoints
                + request.GoalkeeperPoints
                + request.TacticPoints
                + request.FitnessPoints;
            if (totalSliderPoints > MaximumSliderPoints)
            {
                return BadRequest($"Training schedules can assign at most {MaximumSliderPoints} points in total.");
            }

            var team = await _teamAccessService.GetOwnedTeamAsync(User);
            if (team == null)
            {
                return NotFound("Team not found.");
            }

            if (!await IsTeamCoachAsync(team.TeamID, request.CoachID))
            {
                return BadRequest("Choose an active coach from the team.");
            }

            var schedule = new TrainingSchedule
            {
                TrainingScheduleID = Guid.NewGuid(),
                TeamID = team.TeamID,
                ScheduleName = scheduleName,
                TrainingScheduleLevel = request.TrainingScheduleLevel,
                AttackPoints = ToTrainingPoints(request.AttackPoints),
                DefendPoints = ToTrainingPoints(request.DefendPoints),
                ControlPoints = ToTrainingPoints(request.ControlPoints),
                GoalkeeperPoints = ToTrainingPoints(request.GoalkeeperPoints),
                TacticPoints = ToTrainingPoints(request.TacticPoints),
                FitnessPoints = ToTrainingPoints(request.FitnessPoints),
                CoachID = request.CoachID
            };

            _db.TrainingSchedules.Add(schedule);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeamTrainingSchedules), ToDto(schedule));
        }

        [HttpPut("{trainingScheduleID:guid}/coach")]
        public async Task<ActionResult<TrainingScheduleDTO>> UpdateTrainingScheduleCoach(
            Guid trainingScheduleID,
            [FromBody] UpdateTrainingScheduleCoachDTO request)
        {
            var team = await _teamAccessService.GetOwnedTeamAsync(User);
            if (team == null)
            {
                return NotFound("Team not found.");
            }

            var schedule = await _db.TrainingSchedules
                .FirstOrDefaultAsync(item =>
                    item.TrainingScheduleID == trainingScheduleID &&
                    item.TeamID == team.TeamID);
            if (schedule == null)
            {
                return NotFound("Training schedule not found.");
            }

            if (!await IsTeamCoachAsync(team.TeamID, request.CoachID))
            {
                return BadRequest("Choose an active coach from the team.");
            }

            schedule.CoachID = request.CoachID;
            await _db.SaveChangesAsync();

            return Ok(ToDto(schedule));
        }

        [HttpPut("{trainingScheduleID:guid}")]
        public async Task<ActionResult<TrainingScheduleDTO>> UpdateTrainingSchedule(
            Guid trainingScheduleID,
            [FromBody] CreateTrainingScheduleDTO request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var scheduleName = request.ScheduleName.Trim();
            if (string.IsNullOrWhiteSpace(scheduleName))
            {
                return BadRequest("Schedule name is required.");
            }

            if (!Enum.IsDefined(request.TrainingScheduleLevel))
            {
                return BadRequest("Invalid training schedule level.");
            }

            var totalSliderPoints = request.AttackPoints
                + request.DefendPoints
                + request.ControlPoints
                + request.GoalkeeperPoints
                + request.TacticPoints
                + request.FitnessPoints;
            if (totalSliderPoints > MaximumSliderPoints)
            {
                return BadRequest($"Training schedules can assign at most {MaximumSliderPoints} points in total.");
            }

            var team = await _teamAccessService.GetOwnedTeamAsync(User);
            if (team == null)
            {
                return NotFound("Team not found.");
            }

            var schedule = await _db.TrainingSchedules
                .FirstOrDefaultAsync(item =>
                    item.TrainingScheduleID == trainingScheduleID &&
                    item.TeamID == team.TeamID);
            if (schedule == null)
            {
                return NotFound("Training schedule not found.");
            }

            if (!await IsTeamCoachAsync(team.TeamID, request.CoachID))
            {
                return BadRequest("Choose an active coach from the team.");
            }

            schedule.ScheduleName = scheduleName;
            schedule.TrainingScheduleLevel = request.TrainingScheduleLevel;
            schedule.CoachID = request.CoachID;
            schedule.AttackPoints = ToTrainingPoints(request.AttackPoints);
            schedule.DefendPoints = ToTrainingPoints(request.DefendPoints);
            schedule.ControlPoints = ToTrainingPoints(request.ControlPoints);
            schedule.GoalkeeperPoints = ToTrainingPoints(request.GoalkeeperPoints);
            schedule.TacticPoints = ToTrainingPoints(request.TacticPoints);
            schedule.FitnessPoints = ToTrainingPoints(request.FitnessPoints);
            await _db.SaveChangesAsync();

            return Ok(ToDto(schedule));
        }

        private static byte ToTrainingPoints(byte sliderPoints) => (byte)(sliderPoints * 20);

        private async Task<bool> IsTeamCoachAsync(Guid teamID, Guid coachID)
        {
            if (coachID == Guid.Empty)
            {
                return false;
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return await _db.Contracts.AnyAsync(contract =>
                contract.PersonID == coachID &&
                contract.TeamID == teamID &&
                contract.Role == Role.Staff &&
                (contract.EndDate == null || contract.EndDate > today) &&
                contract.Person.StaffRole == StaffRole.Coach);
        }

        private static TrainingScheduleDTO ToDto(TrainingSchedule schedule) => new()
        {
            TrainingScheduleID = schedule.TrainingScheduleID,
            ScheduleName = schedule.ScheduleName,
            TrainingScheduleLevel = schedule.TrainingScheduleLevel,
            AttackPoints = schedule.AttackPoints,
            DefendPoints = schedule.DefendPoints,
            ControlPoints = schedule.ControlPoints,
            GoalkeeperPoints = schedule.GoalkeeperPoints,
            TacticPoints = schedule.TacticPoints,
            FitnessPoints = schedule.FitnessPoints,
            CoachID = schedule.CoachID
        };
    }
}
