using RimRound.Hediffs;
using UnityEngine;
using Verse;

namespace RimRound.Utilities
{
    /// <summary>
    /// Conversions between weight hediff severity and kilograms.
    ///
    /// Severity is stored relative to body size: for an adult human (body size 1),
    /// kg = severity * 1000 + baseWeight. Other pawns weigh that much times their body size,
    /// so children and larger or smaller races reach each stage at proportional weights.
    /// </summary>
    public static class WeightUtility
    {
        public const float SeverityPerKilo = 0.001f;
        public const float PoundsPerKilo = 2.20462f;

        public static HediffDef_Weight Extension => Defs.HediffDefOf.RimRound_Weight.GetModExtension<HediffDef_Weight>();

        public static bool CanHaveWeight(Pawn pawn)
        {
            return pawn?.RaceProps?.Humanlike == true && pawn.needs?.food != null;
        }

        public static float BodySizeFactor(Pawn pawn)
        {
            return Mathf.Max(pawn?.BodySize ?? 1f, 0.05f);
        }

        public static float SeverityToAdultKilos(float severity)
        {
            return severity / SeverityPerKilo + Extension.baseWeight;
        }

        public static float AdultKilosToSeverity(float kilos)
        {
            return (kilos - Extension.baseWeight) * SeverityPerKilo;
        }

        public static float SeverityToKilos(float severity, Pawn pawn)
        {
            return SeverityToAdultKilos(severity) * BodySizeFactor(pawn);
        }

        public static Hediff WeightHediff(this Pawn pawn)
        {
            return pawn?.health?.hediffSet?.GetFirstHediffOfDef(Defs.HediffDefOf.RimRound_Weight);
        }

        /// <returns>The pawn's weight in kg, or 0 if it has no weight hediff.</returns>
        public static float Weight(this Pawn pawn)
        {
            Hediff weight = pawn.WeightHediff();
            return weight is null ? 0f : SeverityToKilos(weight.Severity, pawn);
        }

        /// <returns>The pawn's weight in kg as if it were an adult of body size 1.</returns>
        public static float AdultEquivalentWeight(this Pawn pawn)
        {
            Hediff weight = pawn.WeightHediff();
            return weight is null ? 0f : SeverityToAdultKilos(weight.Severity);
        }

        /// <summary>
        /// Changes the pawn's weight by <paramref name="kilos"/> (negative to lose weight).
        /// The hediff def's min and max severity clamp the result.
        /// </summary>
        public static void ChangeWeight(Pawn pawn, float kilos)
        {
            Hediff weight = pawn.WeightHediff();
            if (weight is null || kilos == 0f)
                return;

            weight.Severity += kilos / BodySizeFactor(pawn) * SeverityPerKilo;
        }

        public static string FormatWeight(float kilos)
        {
            return RimRoundMod.Settings.usePounds
                ? $"{kilos * PoundsPerKilo:F1} lbs"
                : $"{kilos:F1} kg";
        }

        public static Hediff EnsureWeightHediff(Pawn pawn)
        {
            if (!CanHaveWeight(pawn))
                return null;

            Hediff weight = pawn.WeightHediff();
            if (weight != null)
                return weight;

            weight = HediffMaker.MakeHediff(Defs.HediffDefOf.RimRound_Weight, pawn);
            weight.Severity = AdultKilosToSeverity(StartingWeightUtility.RandomStartingAdultKilos(pawn));
            pawn.health.AddHediff(weight);
            return weight;
        }
    }
}
