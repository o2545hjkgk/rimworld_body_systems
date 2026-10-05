using Physique.Core;
using RimWorld;
using Verse;

namespace Physique.Utilities
{
    public static class StartingWeightUtility
    {
        /// <summary>
        /// Rolls a starting BMI from the pawn's faction distribution plus a nudge from its weight opinion.
        /// BodyMath.StartingComposition keeps it above the leanest healthy body.
        /// </summary>
        public static float RandomStartingBmi(Pawn pawn)
        {
            return BodyMath.SampleDistribution(DistributionFor(pawn), Rand.Value, Rand.Value)
                + WeightOpinionUtility.StartingBmiBonus(pawn);
        }

        // (cumulative probability, BMI); see BodyMath.SampleDistribution.
        static readonly (float, float)[] PlayerFaction =
        {
            (0.19301f,  21.2f),
            (0.45035f,  22.9f),
            (0.70770f,  26.1f),
            (0.83637f,  32.7f),
            (0.90071f,  37.6f),
            (0.93931f,  42.4f),
            (0.96505f,  50.6f),
            (0.97791f,  60.4f),
            (0.98435f,  71.8f),
            (0.99078f,  86.5f),
            (0.99464f,  96.3f),
            (0.99721f, 112.7f),
            (0.99850f, 135.5f),
            (0.99914f, 161.6f),
            (0.99953f, 195.9f),
            (0.99979f, 236.7f),
            (1.00000f, 282.4f),
        };

        static readonly (float, float)[] HostileFaction =
        {
            (0.19737f,  21.2f),
            (0.46053f,  22.9f),
            (0.72368f,  26.1f),
            (0.85526f,  32.7f),
            (0.92105f,  37.6f),
            (0.96053f,  42.4f),
            (0.98684f,  50.6f),
            (1.00000f,  60.4f),
        };

        static readonly (float, float)[] FriendlyFaction =
        {
            (0.20979f,  21.2f),
            (0.48951f,  22.9f),
            (0.76923f,  26.1f),
            (0.90909f,  32.7f),
            (0.97902f,  37.6f),
            (1.00000f,  42.4f),
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
