// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.Models.People;

namespace SoccerOpenServer.Services
{
    public static class PlayerPreferredMoveGenerator
    {
        public static IReadOnlyList<PreferredMove> Generate(
            Random random,
            PlayerPosition primaryPosition,
            IReadOnlyCollection<PlayerRole> trainedRoles)
        {
            ArgumentNullException.ThrowIfNull(random);
            ArgumentNullException.ThrowIfNull(trainedRoles);

            var candidates = new HashSet<PreferredMove>();
            var isWide = primaryPosition is PlayerPosition.RightMidfielder or PlayerPosition.LeftMidfielder
                or PlayerPosition.RightWinger or PlayerPosition.LeftWinger
                or PlayerPosition.RightAttackingMidfielder or PlayerPosition.LeftAttackingMidfielder
                || trainedRoles.Any(role => role is PlayerRole.WideMidfielder or PlayerRole.WidePlaymaker
                    or PlayerRole.Winger or PlayerRole.InvertedWinger or PlayerRole.InsideForward
                    or PlayerRole.Raumdeuter or PlayerRole.CentralWinger);
            var isAttacking = primaryPosition is PlayerPosition.RightAttackingMidfielder
                or PlayerPosition.CentralAttackingMidfielder or PlayerPosition.LeftAttackingMidfielder
                or PlayerPosition.RightStriker or PlayerPosition.CentralStriker or PlayerPosition.LeftStriker
                || trainedRoles.Any(role => role is PlayerRole.AttackingMidfielder or PlayerRole.AdvancedPlaymaker
                    or PlayerRole.ShadowStriker or PlayerRole.Trequartista or PlayerRole.SecondStriker
                    or PlayerRole.AdvancedForward or PlayerRole.CompleteForward or PlayerRole.Poacher
                    or PlayerRole.TargetMan or PlayerRole.DeepLyingForward or PlayerRole.FalseNine
                    or PlayerRole.TrequartistaForward);
            var isCreative = trainedRoles.Any(role => role is PlayerRole.DeepLyingPlaymaker or PlayerRole.Regista
                or PlayerRole.AdvancedPlaymaker or PlayerRole.RoamingPlaymaker or PlayerRole.WidePlaymaker
                or PlayerRole.Trequartista or PlayerRole.TrequartistaForward or PlayerRole.FalseNine);
            var isDefensive = primaryPosition is PlayerPosition.RightBack or PlayerPosition.RightCenterBack
                or PlayerPosition.CentralCenterBack or PlayerPosition.LeftCenterBack or PlayerPosition.LeftBack
                or PlayerPosition.RightWingBack or PlayerPosition.RightDefensiveMidfielder
                or PlayerPosition.CentralDefensiveMidfielder or PlayerPosition.LeftDefensiveMidfielder
                or PlayerPosition.LeftWingBack
                || trainedRoles.Any(role => role is PlayerRole.CenterBack or PlayerRole.NoNonsenseCenterBack
                    or PlayerRole.Stopper or PlayerRole.Cover or PlayerRole.FullBack or PlayerRole.WingBack
                    or PlayerRole.CompleteWingBack or PlayerRole.DefensiveMidfielder or PlayerRole.Anchorman
                    or PlayerRole.HalfBack or PlayerRole.BallWinningMidfielder);

            if (isWide)
            {
                candidates.Add(PreferredMove.CutsInside);
                candidates.Add(PreferredMove.RunsWithBall);
            }

            if (isCreative || isAttacking)
            {
                candidates.Add(PreferredMove.TriesThroughBalls);
                candidates.Add(PreferredMove.ShootsFromDistance);
            }

            if (isCreative || isDefensive && trainedRoles.Any(role => role is PlayerRole.BallPlayingDefender or PlayerRole.DeepLyingPlaymaker or PlayerRole.Regista))
            {
                candidates.Add(PreferredMove.LikesToSwitchBall);
            }

            if (isAttacking || trainedRoles.Any(role => role is PlayerRole.WingBack or PlayerRole.CompleteWingBack or PlayerRole.BoxToBoxMidfielder or PlayerRole.Mezzala))
            {
                candidates.Add(PreferredMove.GetsForward);
            }

            if (isAttacking && trainedRoles.Any(role => role is PlayerRole.DeepLyingForward or PlayerRole.FalseNine or PlayerRole.TargetMan or PlayerRole.TrequartistaForward))
            {
                candidates.Add(PreferredMove.ComesDeepToGetBall);
            }

            if (isDefensive && !isWide)
            {
                candidates.Add(PreferredMove.StaysBack);
            }

            // Most generated players have no preferred move; the few who do get at most two.
            var moveCount = random.Next(100) switch
            {
                < 65 => 0,
                < 90 => 1,
                _ => 2
            };

            return candidates
                .OrderBy(_ => random.Next())
                .Take(moveCount)
                .ToArray();
        }
    }
}
