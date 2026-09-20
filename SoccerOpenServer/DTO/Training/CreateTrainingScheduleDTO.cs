// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.Models.Training;
using System.ComponentModel.DataAnnotations;

namespace SoccerOpenServer.DTO.Training
{
    public class CreateTrainingScheduleDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string ScheduleName { get; set; } = string.Empty;

        [EnumDataType(typeof(TrainingScheduleLevel))]
        public TrainingScheduleLevel TrainingScheduleLevel { get; set; }

        public Guid CoachID { get; set; }

        [Range(0, 5)]
        public byte AttackPoints { get; set; }

        [Range(0, 5)]
        public byte DefendPoints { get; set; }

        [Range(0, 5)]
        public byte ControlPoints { get; set; }

        [Range(0, 5)]
        public byte GoalkeeperPoints { get; set; }

        [Range(0, 5)]
        public byte TacticPoints { get; set; }

        [Range(0, 5)]
        public byte FitnessPoints { get; set; }
    }
}
