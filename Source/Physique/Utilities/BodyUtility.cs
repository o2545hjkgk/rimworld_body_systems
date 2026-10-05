using Physique.Core;
using Physique.Hediffs;
using RimWorld;
using Verse;

namespace Physique.Utilities
{
    /// <summary>
    /// Pawn-facing body helpers. Height (adult cm), muscle and fat (adult-equivalent kg) are stored in
    /// hidden hediffs; the frame follows from height, and the visible weight hediff's severity is the
    /// resulting BMI. The formulas live in <see cref="BodyMath"/>.
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

        public static Hediff HeightHediff(this Pawn pawn) => pawn?.health?.hediffSet?.GetFirstHediffOfDef(Defs.HediffDefOf.Physique_Height);

        /// <returns>
        /// The pawn's adult-equivalent body composition (as if fully grown, at body size 1), or null if it has none.
        /// </returns>
        public static BodyComposition? Composition(this Pawn pawn)
        {
            Hediff fat = pawn.FatHediff();
            Hediff muscle = pawn.MuscleHediff();
            Hediff height = pawn.HeightHediff();
            if (fat is null || muscle is null || height is null)
                return null;

            return new BodyComposition(height.Severity, FrameKg(height.Severity), muscle.Severity, fat.Severity);
        }

        /// <summary>Lean frame mass for an adult of this height.</summary>
        public static float FrameKg(float heightCm) => Model.frameKg * BodyMath.MassScaleForHeight(heightCm, Model.referenceHeightCm);

        /// <summary>Essential fat for an adult of this height.</summary>
        public static float EssentialFatKg(float heightCm) => Model.essentialFatKg * BodyMath.MassScaleForHeight(heightCm, Model.referenceHeightCm);

        /// <returns>How tall the pawn is right now, in cm: its adult height, scaled for age and race. 0 if it has no height.</returns>
        public static float CurrentHeightCm(this Pawn pawn)
        {
            Hediff height = pawn.HeightHediff();
            if (height is null)
                return 0f;

            return height.Severity
                * BodyMath.GrowthFraction(pawn.ageTracker.AgeBiologicalYearsFloat, pawn.ageTracker.AdultMinAge)
                * BodyMath.RaceHeightScale(pawn.RaceProps.baseBodySize);
        }

        /// <returns>The pawn's real weight in kg, or 0 if it has no body composition.</returns>
        public static float Weight(this Pawn pawn)
        {
            BodyComposition? body = pawn.Composition();
            return body is null ? 0f : BodyMath.ToRealKilos(body.Value.WeightKg, pawn.BodySize);
        }

        /// <summary>
        /// Adds (or, if negative, removes) <paramref name="kilos"/> of real body fat, never past the maxBmi weight cap.
        /// </summary>
        public static void ChangeFat(Pawn pawn, float kilos)
        {
            Hediff fat = pawn.FatHediff();
            if (kilos > 0f && fat != null && pawn.Composition() is BodyComposition body)
            {
                float roomKg = BodyMath.MaxFatKg(body.HeightCm, body.FrameKg, body.MuscleKg, Model.maxBmi) - body.FatKg;
                kilos = System.Math.Min(kilos, BodyMath.ToRealKilos(System.Math.Max(0f, roomKg), pawn.BodySize));
            }
            ChangeStore(pawn, fat, kilos);
        }

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

            if (pawn.HeightHediff() is null)
            {
                float adultHeightCm = RandomAdultHeightCm(pawn);
                var height = (Hediff_Height)AddIfMissing(pawn, Defs.HediffDefOf.Physique_Height, adultHeightCm);
                height.geneticHeightCm = adultHeightCm;
            }

            if (pawn.FatHediff() is null || pawn.MuscleHediff() is null)
            {
                float heightCm = pawn.HeightHediff().Severity;
                float massScale = BodyMath.MassScaleForHeight(heightCm, Model.referenceHeightCm);
                BodyComposition start = BodyMath.StartingComposition(
                    heightCm,
                    FrameKg(heightCm),
                    (Model.baselineMuscleKg + Rand.Range(-3f, 3f)) * massScale,
                    EssentialFatKg(heightCm),
                    StartingWeightUtility.RandomStartingBmi(pawn),
                    Model.maxBmi);

                AddIfMissing(pawn, Defs.HediffDefOf.Physique_Muscle, start.MuscleKg);
                AddIfMissing(pawn, Defs.HediffDefOf.Physique_Fat, start.FatKg);
            }

            AddIfMissing(pawn, Defs.HediffDefOf.Physique_Weight, Defs.HediffDefOf.Physique_Weight.initialSeverity);
            UpdateWeight(pawn);
        }

        static float RandomAdultHeightCm(Pawn pawn)
        {
            BodyModelExtension model = Model;
            float z = Rand.Gaussian(0f, 1f);
            switch (pawn.gender)
            {
                case Gender.Male:
                    return BodyMath.AdultHeightCm(model.maleHeightCm, model.maleHeightSdCm, z);
                case Gender.Female:
                    return BodyMath.AdultHeightCm(model.femaleHeightCm, model.femaleHeightSdCm, z);
                default:
                    return BodyMath.AdultHeightCm((model.maleHeightCm + model.femaleHeightCm) / 2f, (model.maleHeightSdCm + model.femaleHeightSdCm) / 2f, z);
            }
        }

        static Hediff AddIfMissing(Pawn pawn, HediffDef def, float severity)
        {
            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(def);
            if (existing != null)
                return existing;

            Hediff hediff = HediffMaker.MakeHediff(def, pawn);
            hediff.Severity = severity;
            pawn.health.AddHediff(hediff);
            return hediff;
        }
    }
}
