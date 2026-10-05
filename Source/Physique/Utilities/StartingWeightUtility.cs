using Physique.Core;
using RimWorld;
using Verse;

namespace Physique.Utilities
{
    public static class StartingWeightUtility
    {
        /// <summary>
        /// Rolls a starting weight, in adult-equivalent kg, from the pawn's faction distribution
        /// plus a nudge from its weight opinion. BodyMath.StartingComposition keeps it above the leanest healthy body.
        /// </summary>
        public static float RandomStartingAdultKilos(Pawn pawn)
        {
            return BodyMath.SampleWeightDistribution(DistributionFor(pawn), Rand.Value, Rand.Value)
                + WeightOpinionUtility.StartingWeightBonusKilos(pawn);
        }

        // (cumulative probability, adult kg); see BodyMath.SampleWeightDistribution.
        static readonly (float, float)[] PlayerFaction =
        {
            (0.19301f,  65f),
            (0.45035f,  70f),
            (0.70770f,  80f),
            (0.83637f, 100f),
            (0.90071f, 115f),
            (0.93931f, 130f),
            (0.96505f, 155f),
            (0.97791f, 185f),
            (0.98435f, 220f),
            (0.99078f, 265f),
            (0.99464f, 295f),
            (0.99721f, 345f),
            (0.99850f, 415f),
            (0.99914f, 495f),
            (0.99953f, 600f),
            (0.99979f, 725f),
            (1.00000f, 865f),
        };

        static readonly (float, float)[] HostileFaction =
        {
            (0.19737f,  65f),
            (0.46053f,  70f),
            (0.72368f,  80f),
            (0.85526f, 100f),
            (0.92105f, 115f),
            (0.96053f, 130f),
            (0.98684f, 155f),
            (1.00000f, 185f),
        };

        static readonly (float, float)[] FriendlyFaction =
        {
            (0.20979f,  65f),
            (0.48951f,  70f),
            (0.76923f,  80f),
            (0.90909f, 100f),
            (0.97902f, 115f),
            (1.00000f, 130f),
        };

        static (float, float)[] DistributionFor(Pawn pawn)
        {
            Faction faction = pawn?.Faction;
            if (faction is null || faction.IsPlayer || Faction.OfPlayerSilentFail is null)
                return PlayerFaction;
            if (faction.HostileTo(Faction.OfPlayer))
                return HostileFaction;
            return FriendlyFaction;
        }
    }
}
