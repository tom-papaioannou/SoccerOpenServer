// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.Models.Teams;
using System.ComponentModel.DataAnnotations;

namespace SoccerOpenServer.DTO.Tactics
{
    public class UpdateTacticDTO
    {
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        public bool isMain { get; set; }

        [Required]
        public Formation Formation { get; set; }

        [Required]
        public TacticMentality TacticMentality { get; set; } = TacticMentality.Balanced;

        [Required]
        public PassingMentality PassingMentality { get; set; } = PassingMentality.Mixed;

        [Required]
        public CrossingMentality CrossingMentality { get; set; } = CrossingMentality.Mixed;

        public bool AttackLeft { get; set; } = true;
        public bool AttackMiddle { get; set; } = true;
        public bool AttackRight { get; set; } = true;
        public bool EarlyCrosses { get; set; }
        public bool OffsideTrap { get; set; }

    }
}
