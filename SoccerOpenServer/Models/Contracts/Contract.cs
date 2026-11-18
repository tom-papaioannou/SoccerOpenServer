// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

﻿using SoccerOpenServer.Models.People;
using SoccerOpenServer.Models.Teams;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SoccerOpenServer.Models.Contracts
{
    public class Contract
    {
        public Guid ContractID { get; set; }

        public Guid PersonID { get; set; }

        [JsonIgnore]
        public Person Person { get; set; } = null!;

        public Guid TeamID { get; set; }

        [JsonIgnore]
        public virtual Team Team { get; set; } = null!;

        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Role Role { get; set; }

        [Range(1, 99)]
        public byte? ShirtNumber { get; set; }

        public int Wage { get; set; } = 0;

        public bool isYouth { get; set; } = false;
    }
}
