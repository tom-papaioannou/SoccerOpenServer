// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

﻿using SoccerOpenServer.Models.Competitions;
using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.Users;
using SoccerOpenServer.Models.World;
using System.Text.Json.Serialization;

namespace SoccerOpenServer.Models.Teams
{
    public class Team
    {
        public Guid TeamID { get; set; }

        public string? Name { get; set; }

        public Guid? AppUserID { get; set; }

        [JsonIgnore]
        public virtual AppUser? AppUser { get; set; }

        [JsonIgnore]
        public ICollection<Competition>? Competitions { get; set; }
        [JsonIgnore]
        public ICollection<Contract>? Contracts { get; set; }

        public string Code { get; set; }

        public Guid StadiumID { get; set; }

        [JsonIgnore]
        public virtual Stadium? Stadium { get; set; }

        public Guid KitID { get; set; }

        [JsonIgnore]
        public virtual Kit Kit { get; set; } = null!;

        public string? BadgePath { get; set; }

        public Guid? NationID { get; set; }

        [JsonIgnore]
        public virtual Nation? Nation { get; set; }

        public bool IsNationalTeam { get; set; }

        public ICollection<TeamTacticPriority> TacticPriorities { get; set; } = new List<TeamTacticPriority>();
    }
}
