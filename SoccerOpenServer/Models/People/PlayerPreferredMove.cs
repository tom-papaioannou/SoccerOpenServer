// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

namespace SoccerOpenServer.Models.People
{
    public enum PreferredMove
    {
        CutsInside = 1,
        RunsWithBall = 2,
        TriesThroughBalls = 3,
        ShootsFromDistance = 4,
        GetsForward = 5,
        LikesToSwitchBall = 6,
        ComesDeepToGetBall = 7,
        StaysBack = 8
    }

    public class PlayerPreferredMove
    {
        public Guid PlayerPreferredMoveID { get; set; }
        public Guid PersonID { get; set; }
        public Person Person { get; set; } = null!;
        public PreferredMove PreferredMove { get; set; }
    }
}
