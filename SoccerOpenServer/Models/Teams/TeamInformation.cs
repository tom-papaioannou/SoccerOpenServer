// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SoccerOpenServer.Models.Teams
{
    public class TeamInformation
    {
        public Guid TeamID { get; set; }
        [Range(1, 100)]
        public byte Reputation { get; set; }
        public decimal Balance { get; set; }
        public decimal TransferBudget { get; set; }
        public decimal WageBudget { get; set; }
        public int YearFounded { get; set; }
        public TrainingFacilitiesLevel TrainingFacilitiesLevel { get; set; }
        [JsonIgnore]
        public virtual Team Team { get; set; } = null!;
    }
}
