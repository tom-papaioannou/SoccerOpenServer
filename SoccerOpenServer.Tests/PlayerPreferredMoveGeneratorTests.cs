// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using Microsoft.EntityFrameworkCore;
using SoccerOpenServer.Models.People;
using SoccerOpenServer.Services;
using System;
using System.Linq;
using Xunit;

namespace SoccerOpenServer.Tests;

public class PlayerPreferredMoveGeneratorTests
{
    [Fact]
    public void Generate_ProducesOnlyDefinedMovesAndNeverDuplicates()
    {
        var random = new Random(12345);

        for (var i = 0; i < 1_000; i++)
        {
            var moves = PlayerPreferredMoveGenerator.Generate(
                random,
                PlayerPosition.CentralCenterMidfielder,
                [PlayerRole.AdvancedPlaymaker]);

            Assert.InRange(moves.Count, 0, 2);
            Assert.Equal(moves.Count, moves.Distinct().Count());
            Assert.All(moves, move => Assert.True(Enum.IsDefined(move)));
        }
    }

    [Fact]
    public void Generate_CanAssignMultipleMoves()
    {
        var random = new Random(67890);

        var generatedMultipleMoves = Enumerable.Range(0, 1_000)
            .Select(_ => PlayerPreferredMoveGenerator.Generate(
                random,
                PlayerPosition.RightWinger,
                [PlayerRole.Winger]))
            .Any(moves => moves.Count > 1);

        Assert.True(generatedMultipleMoves);
    }

    [Fact]
    public void Generate_RespectsPositionAndRoleConstraints()
    {
        var random = new Random(24680);

        for (var i = 0; i < 1_000; i++)
        {
            var defenderMoves = PlayerPreferredMoveGenerator.Generate(
                random,
                PlayerPosition.CentralCenterBack,
                [PlayerRole.CenterBack]);

            Assert.DoesNotContain(PreferredMove.CutsInside, defenderMoves);
            Assert.DoesNotContain(PreferredMove.RunsWithBall, defenderMoves);
            Assert.DoesNotContain(PreferredMove.TriesThroughBalls, defenderMoves);
            Assert.DoesNotContain(PreferredMove.ShootsFromDistance, defenderMoves);
            Assert.DoesNotContain(PreferredMove.GetsForward, defenderMoves);
            Assert.DoesNotContain(PreferredMove.ComesDeepToGetBall, defenderMoves);
        }
    }

    [Fact]
    public void PlayerPreferredMove_HasUniquePersonAndMoveConstraint()
    {
        using var context = new SoccerDbContext(
            new DbContextOptionsBuilder<SoccerDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SoccerOpenServerTests")
                .Options);

        var index = context.Model.FindEntityType(typeof(PlayerPreferredMove))!
            .GetIndexes()
            .Single(index => index.IsUnique && index.Properties.Count == 2);

        Assert.Equal(
            [nameof(PlayerPreferredMove.PersonID), nameof(PlayerPreferredMove.PreferredMove)],
            index.Properties.Select(property => property.Name).ToArray());
    }
}
