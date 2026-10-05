using System;

namespace Physique.Core
{
    /// <summary>
    /// Body formulas with no RimWorld dependency, so they can be unit tested outside the game.
    /// </summary>
    public static class BodyMath
    {
        public const float SeverityPerKilo = 0.001f;
        public const float PoundsPerKilo = 2.20462f;
        public const float MinBodySizeFactor = 0.05f;

        /// <summary>
        /// Upper weight-severity bound of each weight opinion mood stage. Severities above the
        /// last bound use the stage after it (Gigantic II).
        /// </summary>
        public static readonly float[] OpinionMoodStageMaxSeverity =
        {
            0.010f, 0.020f, 0.035f, 0.050f, 0.070f, 0.095f, 0.120f, 0.150f,
            0.190f, 0.235f, 0.295f, 0.360f, 0.440f, 0.530f, 0.660f, 0.800f,
        };

        // Weight severity is stored relative to body size: for an adult human (body size 1),
        // kg = severity * 1000 + baseWeight. Other pawns weigh that much times their body size.

        public static float SeverityToAdultKilos(float severity, float baseWeight)
        {
            return severity / SeverityPerKilo + baseWeight;
        }

        public static float AdultKilosToSeverity(float adultKilos, float baseWeight)
        {
            return (adultKilos - baseWeight) * SeverityPerKilo;
        }

        public static float BodySizeFactor(float bodySize)
        {
            return Math.Max(bodySize, MinBodySizeFactor);
        }

        public static float SeverityToKilos(float severity, float baseWeight, float bodySize)
        {
            return SeverityToAdultKilos(severity, baseWeight) * BodySizeFactor(bodySize);
        }

        /// <summary>Severity change for gaining (or, if negative, losing) <paramref name="kilos"/> of real weight.</summary>
        public static float SeverityDeltaForKilos(float kilos, float bodySize)
        {
            return kilos / BodySizeFactor(bodySize) * SeverityPerKilo;
        }

        public static string FormatWeight(float kilos, bool usePounds)
        {
            return usePounds
                ? $"{kilos * PoundsPerKilo:F1} lbs"
                : $"{kilos:F1} kg";
        }

        public static int OpinionMoodStageIndex(float weightSeverity)
        {
            for (int i = 0; i < OpinionMoodStageMaxSeverity.Length; ++i)
            {
                if (weightSeverity <= OpinionMoodStageMaxSeverity[i])
                    return i;
            }
            return OpinionMoodStageMaxSeverity.Length;
        }

        /// <summary>
        /// Nutrition from a meal that doesn't fit in the food need, which vanilla discards.
        /// </summary>
        public static float SurplusNutrition(float nutritionIngested, float foodCurLevel, float foodMaxLevel)
        {
            return Math.Max(0f, nutritionIngested - (foodMaxLevel - foodCurLevel));
        }

        public enum FastingResponse
        {
            /// <summary>Not hungry enough for the body to touch its stores.</summary>
            None,
            /// <summary>Fat covers the food need: the pawn stays hungry but doesn't starve, and loses weight.</summary>
            BurnFatForFood,
            /// <summary>No fat left to spare and starving: the pawn wastes away on top of vanilla malnutrition.</summary>
            WasteAway,
        }

        public static FastingResponse Fasting(float foodLevelPercentage, float hungryThresholdPercentage, bool starving, float adultKilos, float fatReserveFloorKilos)
        {
            if (foodLevelPercentage >= hungryThresholdPercentage)
                return FastingResponse.None;
            if (adultKilos > fatReserveFloorKilos)
                return FastingResponse.BurnFatForFood;
            return starving ? FastingResponse.WasteAway : FastingResponse.None;
        }

        /// <summary>
        /// Samples a cumulative distribution of (cumulative probability, adult kg) points.
        /// A <paramref name="roll"/> at or below a point's probability lands between the previous
        /// point's weight and this one's, placed by <paramref name="lerpRoll"/>. The first band
        /// spans the first two points.
        /// </summary>
        public static float SampleWeightDistribution((float cumulativeProbability, float kilos)[] distribution, float roll, float lerpRoll)
        {
            for (int i = 0; i < distribution.Length; ++i)
            {
                if (roll > distribution[i].cumulativeProbability)
                    continue;

                float low = i == 0 ? distribution[0].kilos : distribution[i - 1].kilos;
                float high = i == 0 ? distribution[1].kilos : distribution[i].kilos;
                return low + (high - low) * lerpRoll;
            }

            return distribution[distribution.Length - 1].kilos;
        }
    }
}
