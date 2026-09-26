// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

﻿using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.Servers;
using SoccerOpenServer.Models.Teams;
using SoccerOpenServer.Models.Training;
using SoccerOpenServer.Models.Users;
using SoccerOpenServer.Models.World;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SoccerOpenServer.Models.People
{
    public class Person
    {
        public Guid PersonID { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public AppUser? AppUser { get; set; }
        public Guid? NationID { get; set; }
        [JsonIgnore]
        public Nation? Nation { get; set; }
        public Guid? ServerID { get; set; }
        [JsonIgnore]
        [ForeignKey("ServerID")]
        public virtual Server? Server { get; set; }

        public ICollection<PlayerTrainedPosition>? PlayerTrainedPositions { get; set; }
        public ICollection<PlayerTrainedRole>? PlayerTrainedRoles { get; set; }
        public ICollection<PlayerPreferredMove> PlayerPreferredMoves { get; set; } = new List<PlayerPreferredMove>();
        public virtual PlayerStats? PlayerStats { get; set; }
        public virtual CoachStats? CoachStats { get; set; }
        public virtual MedicStats? MedicStats { get; set; }
        [JsonIgnore]
        public ICollection<PlayerTactic>? PlayerTactics { get; set; }
        public StaffRole? StaffRole { get; set; }
        public int Weight { get; set; } = 80;
        public int Height { get; set; } = 180;

        public PersonHealthAndFitness? HealthAndFitness { get; set; }

        [JsonIgnore]
        public ManagerGameStats? ManagerGameStats { get; set; }

        [JsonIgnore]
        public ICollection<ManagerFormationPicked> ManagerFormationsPicked { get; set; } = new List<ManagerFormationPicked>();

        public Guid? TrainingScheduleID { get; set; }

        public TrainingSchedule? TrainingSchedule { get; set; }
    }
}
