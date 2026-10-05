using System;

namespace Physique.Core
{
    /// <summary>
    /// A body as frame + muscle + fat, in adult-equivalent kg (real kg = these × body size).
    /// </summary>
    public readonly struct BodyComposition
    {
        public readonly float HeightCm;
        public readonly float FrameKg;
        public readonly float MuscleKg;
        public readonly float FatKg;

        public BodyComposition(float heightCm, float frameKg, float muscleKg, float fatKg)
        {
            HeightCm = heightCm;
            FrameKg = frameKg;
            MuscleKg = muscleKg;
            FatKg = fatKg;
        }

        public float WeightKg => FrameKg + MuscleKg + FatKg;

        public float LeanKg => FrameKg + MuscleKg;

        public float Bmi => BodyMath.Bmi(WeightKg, HeightCm);

        public float BodyFatFraction => WeightKg > 0f ? FatKg / WeightKg : 0f;
    }

    /// <summary>
    /// Body formulas with no RimWorld dependency, so they can be unit tested outside the game.
    /// </summary>
    public static class BodyMath
    {
        public const float PoundsPerKilo = 2.20462f;
        public const float MinBodySizeFactor = 0.05f;

        /// <summary>
        /// Upper BMI bound of each weight opinion mood stage. BMIs above the last bound use the
        /// stage after it (Gigantic II).
        /// </summary>
        public static readonly float[] OpinionMoodStageMaxBmi =
        {
            11.4f, 14.7f, 19.6f, 24.5f, 31.0f, 39.2f, 47.3f, 57.1f,
            70.2f, 84.9f, 104.5f, 125.7f, 151.8f, 181.2f, 223.7f, 269.4f,
        };

        public static float Bmi(float weightKg, float heightCm)
        {
            float heightM = heightCm / 100f;
            return weightKg / (heightM * heightM);
        }

        public static float BodySizeFactor(float bodySize)
        {
            return Math.Max(bodySize, MinBodySizeFactor);
        }

        /// <summary>Converts real kg to adult-equivalent kg, the unit body composition is stored in.</summary>
        public static float ToAdultKilos(float kilos, float bodySize)
        {
            return kilos / BodySizeFactor(bodySize);
        }

        public static float ToRealKilos(float adultKilos, float bodySize)
        {
            return adultKilos * BodySizeFactor(bodySize);
        }

        public static string FormatWeight(float kilos, bool usePounds)
        {
            return usePounds
                ? $"{kilos * PoundsPerKilo:F1} lbs"
                : $"{kilos:F1} kg";
        }

        public static int OpinionMoodStageIndex(float bmi)
        {
            for (int i = 0; i < OpinionMoodStageMaxBmi.Length; ++i)
            {
                if (bmi <= OpinionMoodStageMaxBmi[i])
                    return i;
            }
            return OpinionMoodStageMaxBmi.Length;
        }

        /// <summary>
        /// Splits a starting weight into muscle and fat for a given frame. Weight that would leave
        /// less than essential fat is raised to the leanest healthy body instead, so nobody starts out wasted.
        /// </summary>
        public static BodyComposition StartingComposition(float heightCm, float frameKg, float muscleKg, float essentialFatKg, float weightKg)
        {
            float fatKg = Math.Max(essentialFatKg, weightKg - frameKg - muscleKg);
            return new BodyComposition(heightCm, frameKg, muscleKg, fatKg);
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
            /// <summary>Fat covers the food need: the pawn stays hungry but doesn't starve, and loses fat.</summary>
            BurnFat,
            /// <summary>Down to essential fat and starving: muscle wastes away on top of vanilla malnutrition.</summary>
            WasteMuscle,
        }

        public static FastingResponse Fasting(float foodLevelPercentage, float hungryThresholdPercentage, bool starving, float fatKg, float essentialFatKg)
        {
            if (foodLevelPercentage >= hungryThresholdPercentage)
                return FastingResponse.None;
            if (fatKg > essentialFatKg)
                return FastingResponse.BurnFat;
            return starving ? FastingResponse.WasteMuscle : FastingResponse.None;
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
