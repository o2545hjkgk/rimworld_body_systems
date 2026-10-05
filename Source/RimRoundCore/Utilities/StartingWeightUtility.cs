using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimRound.Utilities
{
    public static class StartingWeightUtility
    {
        /// <summary>
        /// Rolls a starting weight, in adult-equivalent kg, from the pawn's faction distribution
        /// plus a nudge from its weight opinion.
        /// </summary>
        public static float RandomStartingAdultKilos(Pawn pawn)
        {
            float kilos = RandomFromDistribution(DistributionFor(pawn)) + WeightOpinionUtility.StartingWeightBonusKilos(pawn);
            return Mathf.Max(kilos, WeightUtility.SeverityToAdultKilos(Defs.HediffDefOf.RimRound_Weight.minSeverity));
        }

        // Cumulative probability -> adult kg. A roll at or below an entry's probability lands
        // uniformly between the previous entry's weight and this one's.
        static readonly List<Pair<float, float>> PlayerFaction = new List<Pair<float, float>>
        {
            new Pair<float, float>(0.19301f,  65f),
            new Pair<float, float>(0.45035f,  70f),
            new Pair<float, float>(0.70770f,  80f),
            new Pair<float, float>(0.83637f, 100f),
            new Pair<float, float>(0.90071f, 115f),
            new Pair<float, float>(0.93931f, 130f),
            new Pair<float, float>(0.96505f, 155f),
            new Pair<float, float>(0.97791f, 185f),
            new Pair<float, float>(0.98435f, 220f),
            new Pair<float, float>(0.99078f, 265f),
            new Pair<float, float>(0.99464f, 295f),
            new Pair<float, float>(0.99721f, 345f),
            new Pair<float, float>(0.99850f, 415f),
            new Pair<float, float>(0.99914f, 495f),
            new Pair<float, float>(0.99953f, 600f),
            new Pair<float, float>(0.99979f, 725f),
            new Pair<float, float>(1.00000f, 865f),
        };

        static readonly List<Pair<float, float>> HostileFaction = new List<Pair<float, float>>
        {
            new Pair<float, float>(0.19737f,  65f),
            new Pair<float, float>(0.46053f,  70f),
            new Pair<float, float>(0.72368f,  80f),
            new Pair<float, float>(0.85526f, 100f),
            new Pair<float, float>(0.92105f, 115f),
            new Pair<float, float>(0.96053f, 130f),
            new Pair<float, float>(0.98684f, 155f),
            new Pair<float, float>(1.00000f, 185f),
        };

        static readonly List<Pair<float, float>> FriendlyFaction = new List<Pair<float, float>>
        {
            new Pair<float, float>(0.20979f,  65f),
            new Pair<float, float>(0.48951f,  70f),
            new Pair<float, float>(0.76923f,  80f),
            new Pair<float, float>(0.90909f, 100f),
            new Pair<float, float>(0.97902f, 115f),
            new Pair<float, float>(1.00000f, 130f),
        };

        static float RandomFromDistribution(List<Pair<float, float>> distribution)
        {
            float roll = Rand.Value;
            for (int i = 0; i < distribution.Count; ++i)
            {
                if (roll > distribution[i].First)
                    continue;

                return i == 0
                    ? Rand.Range(distribution[0].Second, distribution[1].Second)
                    : Rand.Range(distribution[i - 1].Second, distribution[i].Second);
            }

            return distribution[distribution.Count - 1].Second;
        }

        static List<Pair<float, float>> DistributionFor(Pawn pawn)
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
