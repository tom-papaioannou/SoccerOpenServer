// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SoccerOpenServer.Models.People
{
    public class PlayerStats
    {
        public Guid PlayerStatsID { get; set; }
        public Guid PersonID { get; set; }
        [JsonIgnore]
        public virtual Person Person { get; set; } = null!;
        [Range(1, 100)]
        public byte Shooting { get; set; }
        [Range(1, 100)]
        public byte Passing { get; set; }
        [Range(1, 100)]
        public byte Crossing { get; set; }
        [Range(1, 100)]
        public byte Tackling { get; set; }
        [Range(1, 100)]
        public byte Dribbling { get; set; }
        [Range(1, 100)]
        public byte Control { get; set; }
        [Range(1, 100)]
        public byte Kicking { get; set; }
        [Range(1, 100)]
        public byte Goalkeeping { get; set; }
        [Range(1, 100)]
        public byte Teamwork { get; set; }
        [Range(1, 100)]
        public byte Creativity { get; set; }
        [Range(1, 100)]
        public byte Decisions { get; set; }
        [Range(1, 100)]
        public byte Positioning { get; set; }
        [Range(1, 100)]
        public byte Speed { get; set; }
        [Range(1, 100)]
        public byte Acceleration { get; set; }
        [Range(1, 100)]
        public byte Strength { get; set; }
        [Range(1, 100)]
        public byte Jumping { get; set; }
        [Range(1, 100)]
        public byte Stamina { get; set; }
        [Range(1, 100)]
        public byte Rating { get; set; } = 1;
        [Range(1, 100)]
        public byte RightLegRating { get; set; }
        [Range(1, 100)]
        public byte LeftLegRating { get; set; }
    }
}
