// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

namespace SoccerOpenServer.Services
{
    public static class PlayerLegRatingGenerator
    {
        public static (byte RightLegRating, byte LeftLegRating) Generate(Random random)
        {
            ArgumentNullException.ThrowIfNull(random);

            var strongRating = (byte)random.Next(85, 101);
            var weakRating = (byte)random.Next(25, strongRating);

            return random.Next(0, 2) == 0
                ? (strongRating, weakRating)
                : (weakRating, strongRating);
        }
    }
}
