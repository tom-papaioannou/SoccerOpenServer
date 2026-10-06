// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using System.Buffers.Binary;
using SoccerOpenServer.Models.Teams;

namespace SoccerOpenServer.Services;

public static class TeamInformationGenerator
{
    public const int EarliestFoundingYear = 1850;

    public static TeamInformation Generate(Team team, int currentGameYear)
    {
        ArgumentNullException.ThrowIfNull(team);
        ArgumentOutOfRangeException.ThrowIfLessThan(currentGameYear, EarliestFoundingYear);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(currentGameYear, 9999);
        var random = new Random(BinaryPrimitives.ReadInt32LittleEndian(team.TeamID.ToByteArray()));
        var reputation = (byte)random.Next(1, 101);
        var balance = team.IsNationalTeam ? 0m : 1_000_000m + reputation * 500_000m;

        return new TeamInformation
        {
            TeamID = team.TeamID,
            Team = team,
            Reputation = reputation,
            Balance = balance,
            TransferBudget = balance * 0.25m,
            WageBudget = balance * 0.5m,
            YearFounded = random.Next(EarliestFoundingYear, currentGameYear + 1),
            TrainingFacilitiesLevel = (TrainingFacilitiesLevel)(1 + (reputation - 1) * 6 / 100)
        };
    }
}
