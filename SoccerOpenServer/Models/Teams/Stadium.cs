// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace SoccerOpenServer.Models.Teams
{
    public class Stadium
    {
        public Guid StadiumID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public StadiumState StadiumState { get; set; } = StadiumState.Low;
        public StadiumLayout StadiumLayout { get; set; } = StadiumLayout.Layout01;
        public double Latitude { get; set; } = 0.0;
        public double Longitude { get; set; } = 0.0;
        [JsonIgnore]
        public virtual Team? Team { get; set; }
    }
}
