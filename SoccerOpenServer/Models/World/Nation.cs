// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using System.Text.Json.Serialization;
using SoccerOpenServer.Models.Teams;

namespace SoccerOpenServer.Models.World
{
    public class Nation
    {
        public Guid NationID { get; set; }
        public string Name { get; set; } = null!;
        public string ISO2 { get; set; } = null!;
        public string? ISO3 { get; set; } 
        public string? FlagUrl { get; set; }
        public Guid ContinentID { get; set; }
        [JsonIgnore]
        public Continent Continent { get; set; } = null!;

        [JsonIgnore]
        public Team? NationalTeam { get; set; }
    }
}
