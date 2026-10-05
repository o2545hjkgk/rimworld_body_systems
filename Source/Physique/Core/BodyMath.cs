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

    public enum Sex
    {
        Unknown,
        Male,
        Female,
    }

    /// <summary>
    /// Body formulas with no RimWorld dependency, so they can be unit tested outside the game.
    /// </summary>
    public static class BodyMath
    {
        public const float PoundsPerKilo = 2.20462f;
        public const float CentimetresPerInch = 2.54f;
        public const float MinBodySizeFactor = 0.05f;

        /// <summary>Age at which a human reaches adult height, in years.</summary>
        public const float HumanAdultAge = 18f;

        /// <summary>
        /// Fraction of adult height reached at each age (years), from typical human growth charts.
        /// Linear in between; 1 from <see cref="HumanAdultAge"/> on.
        /// </summary>
        static readonly (float age, float fraction)[] GrowthCurve =
        {
            (0f, 0.29f), (1f, 0.43f), (2f, 0.50f), (3f, 0.55f), (5f, 0.63f), (8f, 0.73f),
            (10f, 0.79f), (12f, 0.85f), (14f, 0.93f), (16f, 0.98f), (18f, 1.00f),
        };

        /// <summary>
        /// Upper BMI bound of each weight opinion mood stage. BMIs above the last bound use the
        /// stage after it (Gigantic II).
        /// </summary>
        public static readonly float[] OpinionMoodStageMaxBmi =
        {
            11.4f, 14.7f, 19.6f, 24.5f, 31.0f, 39.2f, 47.3f, 57.1f,
            70.2f, 84.9f, 104.5f, 125.7f, 151.8f, 181.2f, 223.7f, 269.4f,
        };

        /// <summary>
        /// Adult height for a z-score (standard deviations from the mean), clamped to ±3 SD.
        /// </summary>
        public static float AdultHeightCm(float meanCm, float sdCm, float z)
        {
            return meanCm + sdCm * Math.Max(-3f, Math.Min(3f, z));
        }

        /// <summary>
        /// Fraction of adult height reached at <paramref name="ageYears"/>, for a species that is
        /// adult at <paramref name="adultAgeYears"/> (its age is mapped onto the human growth curve).
        /// </summary>
        public static float GrowthFraction(float ageYears, float adultAgeYears)
        {
            float humanAge = adultAgeYears > 0f ? ageYears / adultAgeYears * HumanAdultAge : HumanAdultAge;
            if (humanAge <= GrowthCurve[0].age)
                return GrowthCurve[0].fraction;

            for (int i = 1; i < GrowthCurve.Length; ++i)
            {
                if (humanAge > GrowthCurve[i].age)
                    continue;

                (float a0, float f0) = GrowthCurve[i - 1];
                (float a1, float f1) = GrowthCurve[i];
                return f0 + (f1 - f0) * (humanAge - a0) / (a1 - a0);
            }
            return 1f;
        }

        /// <summary>
        /// How much a mass defined at <paramref name="referenceHeightCm"/> scales for a body of
        /// <paramref name="heightCm"/>. Lean mass scales with height squared, which is what keeps BMI comparable.
        /// </summary>
        public static float MassScaleForHeight(float heightCm, float referenceHeightCm)
        {
            float ratio = heightCm / referenceHeightCm;
            return ratio * ratio;
        }

        /// <summary>Real height of a non-human race whose base body size differs from a human's (mass scales with length cubed).</summary>
        public static float RaceHeightScale(float baseBodySize)
        {
            return (float)Math.Pow(Math.Max(baseBodySize, MinBodySizeFactor), 1.0 / 3.0);
        }

        /// <summary>
        /// Adult height lost to childhood malnutrition over <paramref name="days"/>, before the cap.
        /// </summary>
        public static float StuntingCm(float malnutritionSeverity, float days, float cmPerDayAtFullSeverity)
        {
            return Math.Max(0f, malnutritionSeverity) * days * cmPerDayAtFullSeverity;
        }

        public static string FormatHeight(float heightCm, bool imperial)
        {
            if (!imperial)
                return $"{heightCm / 100f:F2} m";

            int totalInches = (int)Math.Round(heightCm / CentimetresPerInch);
            return $"{totalInches / 12}'{totalInches % 12}\"";
        }

        public static float Bmi(float weightKg, float heightCm)
        {
            float heightM = heightCm / 100f;
            return weightKg / (heightM * heightM);
        }

        public static float WeightForBmi(float bmi, float heightCm)
        {
            float heightM = heightCm / 100f;
            return bmi * heightM * heightM;
        }

        /// <summary>Most fat a body can carry before its BMI passes <paramref name="maxBmi"/>.</summary>
        public static float MaxFatKg(float heightCm, float frameKg, float muscleKg, float maxBmi)
        {
            return Math.Max(0f, WeightForBmi(maxBmi, heightCm) - frameKg - muscleKg);
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
        /// Builds a starting body at <paramref name="startingBmi"/> (capped at <paramref name="maxBmi"/>): the weight
        /// that BMI implies at this height, minus frame and muscle, is fat. A BMI that would leave less than
        /// essential fat is raised to the leanest healthy body instead, so nobody starts out wasted.
        /// </summary>
        public static BodyComposition StartingComposition(float heightCm, float frameKg, float muscleKg, float essentialFatKg, float startingBmi, float maxBmi)
        {
            float fatKg = WeightForBmi(Math.Min(startingBmi, maxBmi), heightCm) - frameKg - muscleKg;
            return new BodyComposition(heightCm, frameKg, muscleKg, Math.Max(essentialFatKg, fatKg));
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
        /// Samples a cumulative distribution of (cumulative probability, value) points.
        /// A <paramref name="roll"/> at or below a point's probability lands between the previous
        /// point's value and this one's, placed by <paramref name="lerpRoll"/>. The first band
        /// spans the first two points.
        /// </summary>
        public static float SampleDistribution((float cumulativeProbability, float value)[] distribution, float roll, float lerpRoll)
        {
            for (int i = 0; i < distribution.Length; ++i)
            {
                if (roll > distribution[i].cumulativeProbability)
                    continue;

                float low = i == 0 ? distribution[0].value : distribution[i - 1].value;
                float high = i == 0 ? distribution[1].value : distribution[i].value;
                return low + (high - low) * lerpRoll;
            }

            return distribution[distribution.Length - 1].value;
        }
    }
}
