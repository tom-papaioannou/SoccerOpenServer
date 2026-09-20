// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.Models.Training;

namespace SoccerOpenServer.DTO.Training
{
    public class TrainingScheduleDTO
    {
        public Guid TrainingScheduleID { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public TrainingScheduleLevel TrainingScheduleLevel { get; set; }
        public byte AttackPoints { get; set; }
        public byte DefendPoints { get; set; }
        public byte ControlPoints { get; set; }
        public byte GoalkeeperPoints { get; set; }
        public byte TacticPoints { get; set; }
        public byte FitnessPoints { get; set; }
        public Guid? CoachID { get; set; }
    }
}
