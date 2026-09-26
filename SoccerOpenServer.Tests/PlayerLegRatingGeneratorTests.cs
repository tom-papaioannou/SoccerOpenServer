// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using System;
using SoccerOpenServer.Services;
using Xunit;

namespace SoccerOpenServer.Tests;

public class PlayerLegRatingGeneratorTests
{
    [Fact]
    public void Generate_ProducesValidRatings()
    {
        var random = new Random(12345);

        for (var i = 0; i < 1_000; i++)
        {
            var (rightLegRating, leftLegRating) = PlayerLegRatingGenerator.Generate(random);
            var strongRating = Math.Max(rightLegRating, leftLegRating);
            var weakRating = Math.Min(rightLegRating, leftLegRating);

            Assert.InRange(rightLegRating, (byte)1, (byte)100);
            Assert.InRange(leftLegRating, (byte)1, (byte)100);
            Assert.True(strongRating >= 85);
            Assert.True(weakRating >= 25);
            Assert.True(weakRating < strongRating);
        }
    }

    [Fact]
    public void Generate_CanProduceEitherDominantLeg()
    {
        var random = new Random(67890);
        var generatedRightFootDominant = false;
        var generatedLeftFootDominant = false;

        for (var i = 0; i < 1_000 && (!generatedRightFootDominant || !generatedLeftFootDominant); i++)
        {
            var (rightLegRating, leftLegRating) = PlayerLegRatingGenerator.Generate(random);
            generatedRightFootDominant |= rightLegRating > leftLegRating;
            generatedLeftFootDominant |= leftLegRating > rightLegRating;
        }

        Assert.True(generatedRightFootDominant);
        Assert.True(generatedLeftFootDominant);
    }
}
