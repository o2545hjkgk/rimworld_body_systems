using Physique.Core;
using Physique.Hediffs;
using Verse;

namespace Physique.Utilities
{
    /// <summary>
    /// Pawn-facing weight helpers. The formulas themselves live in <see cref="BodyMath"/>.
    /// </summary>
    public static class WeightUtility
    {
        public static HediffDef_Weight Extension => Defs.HediffDefOf.Physique_Weight.GetModExtension<HediffDef_Weight>();

        public static bool CanHaveWeight(Pawn pawn)
        {
            return pawn?.RaceProps?.Humanlike == true && pawn.needs?.food != null;
        }

        public static float SeverityToAdultKilos(float severity)
        {
            return BodyMath.SeverityToAdultKilos(severity, Extension.baseWeight);
        }

        public static float AdultKilosToSeverity(float adultKilos)
        {
            return BodyMath.AdultKilosToSeverity(adultKilos, Extension.baseWeight);
        }

        public static float SeverityToKilos(float severity, Pawn pawn)
        {
            return BodyMath.SeverityToKilos(severity, Extension.baseWeight, pawn?.BodySize ?? 1f);
        }

        public static Hediff WeightHediff(this Pawn pawn)
        {
            return pawn?.health?.hediffSet?.GetFirstHediffOfDef(Defs.HediffDefOf.Physique_Weight);
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

            weight.Severity += BodyMath.SeverityDeltaForKilos(kilos, pawn.BodySize);
        }

        public static Hediff EnsureWeightHediff(Pawn pawn)
        {
            if (!CanHaveWeight(pawn))
                return null;

            Hediff weight = pawn.WeightHediff();
            if (weight != null)
                return weight;

            weight = HediffMaker.MakeHediff(Defs.HediffDefOf.Physique_Weight, pawn);
            weight.Severity = AdultKilosToSeverity(StartingWeightUtility.RandomStartingAdultKilos(pawn));
            pawn.health.AddHediff(weight);
            return weight;
        }
    }
}
