using Physique.Core;
using Physique.Hediffs;
using RimWorld;
using Verse;

namespace Physique.Utilities
{
    /// <summary>
    /// Pawn-facing body helpers. Fat and muscle are stored in hidden hediffs as adult-equivalent kg;
    /// the visible weight hediff's severity is the resulting BMI. The formulas live in <see cref="BodyMath"/>.
    /// </summary>
    public static class BodyUtility
    {
        public static BodyModelExtension Model => Defs.HediffDefOf.Physique_Weight.GetModExtension<BodyModelExtension>();

        public static bool CanHaveBody(Pawn pawn)
        {
            return pawn?.RaceProps?.Humanlike == true && pawn.needs?.food != null;
        }

        public static Hediff WeightHediff(this Pawn pawn) => pawn?.health?.hediffSet?.GetFirstHediffOfDef(Defs.HediffDefOf.Physique_Weight);

        public static Hediff FatHediff(this Pawn pawn) => pawn?.health?.hediffSet?.GetFirstHediffOfDef(Defs.HediffDefOf.Physique_Fat);

        public static Hediff MuscleHediff(this Pawn pawn) => pawn?.health?.hediffSet?.GetFirstHediffOfDef(Defs.HediffDefOf.Physique_Muscle);

        /// <returns>The pawn's adult-equivalent body composition, or null if it has none.</returns>
        public static BodyComposition? Composition(this Pawn pawn)
        {
            Hediff fat = pawn.FatHediff();
            Hediff muscle = pawn.MuscleHediff();
            if (fat is null || muscle is null)
                return null;

            BodyModelExtension model = Model;
            return new BodyComposition(model.referenceHeightCm, model.frameKg, muscle.Severity, fat.Severity);
        }

        /// <returns>The pawn's real weight in kg, or 0 if it has no body composition.</returns>
        public static float Weight(this Pawn pawn)
        {
            BodyComposition? body = pawn.Composition();
            return body is null ? 0f : BodyMath.ToRealKilos(body.Value.WeightKg, pawn.BodySize);
        }

        /// <summary>Adds (or, if negative, removes) <paramref name="kilos"/> of real body fat.</summary>
        public static void ChangeFat(Pawn pawn, float kilos) => ChangeStore(pawn, pawn.FatHediff(), kilos);

        /// <summary>Adds (or, if negative, removes) <paramref name="kilos"/> of real muscle.</summary>
        public static void ChangeMuscle(Pawn pawn, float kilos) => ChangeStore(pawn, pawn.MuscleHediff(), kilos);

        static void ChangeStore(Pawn pawn, Hediff store, float kilos)
        {
            if (store is null || kilos == 0f)
                return;

            // The hediff def's min and max severity clamp the result.
            store.Severity += BodyMath.ToAdultKilos(kilos, pawn.BodySize);
            UpdateWeight(pawn);
        }

        /// <summary>Recomputes the visible weight hediff (BMI) from fat and muscle.</summary>
        public static void UpdateWeight(Pawn pawn)
        {
            BodyComposition? body = pawn.Composition();
            Hediff weight = pawn.WeightHediff();
            if (body is null || weight is null)
                return;

            weight.Severity = body.Value.Bmi;
        }

        /// <summary>Gives a pawn that can have a body any parts of it that are missing.</summary>
        public static void EnsureBody(Pawn pawn)
        {
            if (!CanHaveBody(pawn))
                return;

            if (pawn.FatHediff() is null || pawn.MuscleHediff() is null)
            {
                BodyModelExtension model = Model;
                BodyComposition start = BodyMath.StartingComposition(
                    model.referenceHeightCm,
                    model.frameKg,
                    model.baselineMuscleKg + Rand.Range(-3f, 3f),
                    model.essentialFatKg,
                    StartingWeightUtility.RandomStartingAdultKilos(pawn));

                AddIfMissing(pawn, Defs.HediffDefOf.Physique_Muscle, start.MuscleKg);
                AddIfMissing(pawn, Defs.HediffDefOf.Physique_Fat, start.FatKg);
            }

            AddIfMissing(pawn, Defs.HediffDefOf.Physique_Weight, Defs.HediffDefOf.Physique_Weight.initialSeverity);
            UpdateWeight(pawn);
        }

        static void AddIfMissing(Pawn pawn, HediffDef def, float severity)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(def) != null)
                return;

            Hediff hediff = HediffMaker.MakeHediff(def, pawn);
            hediff.Severity = severity;
            pawn.health.AddHediff(hediff);
        }
    }
}
