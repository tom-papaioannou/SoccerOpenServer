// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using System.ComponentModel.DataAnnotations;

namespace SoccerOpenServer.Models.People
{
    public class PersonHealthAndFitness
    {
        public Guid PersonHealthAndFitnessID { get; set; }
        public Guid PersonID { get; set; }

        public Person Person { get; set; } = null!;

        [Range(1, 100)]
        public byte PhysicalCondition { get; set; } = 100;

        [Range(1, 100)]
        public byte MentalCondition { get; set; } = 100;

        [Range(1, 100)]
        public byte FitnessCondition { get; set; } = 100;

        public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;
    }
}
