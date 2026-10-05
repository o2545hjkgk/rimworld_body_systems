using Physique.Core;
using Physique.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Physique.Hediffs
{
    /// <summary>
    /// Adult height in cm (what the pawn is, or will be once grown). Hidden; it shows up through the weight hediff.
    /// While a pawn is still growing, malnutrition permanently lowers it, down to
    /// <see cref="geneticHeightCm"/> minus <see cref="BodyModelExtension.maxStuntingCm"/>.
    /// </summary>
    public class Hediff_Height : Hediff
    {
        const int GrowthIntervalTicks = GenDate.TicksPerHour;

        /// <summary>The adult height the pawn was born to reach, before any stunting.</summary>
        public float geneticHeightCm;

        public override bool Visible => false;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (geneticHeightCm <= 0f)
                geneticHeightCm = Severity;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref geneticHeightCm, "geneticHeightCm");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && geneticHeightCm <= 0f)
                geneticHeightCm = Severity;
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            if (pawn.IsHashIntervalTick(GrowthIntervalTicks, delta))
                StuntIfMalnourished(GrowthIntervalTicks);
        }

        void StuntIfMalnourished(int ticks)
        {
            if (pawn.ageTracker.Adult)
                return;

            Hediff malnutrition = pawn.health.hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.Malnutrition);
            if (malnutrition is null)
                return;

            BodyModelExtension model = BodyUtility.Model;
            float lost = BodyMath.StuntingCm(malnutrition.Severity, (float)ticks / GenDate.TicksPerDay, model.stuntingCmPerDay);
            float stunted = Mathf.Max(Severity - lost, geneticHeightCm - model.maxStuntingCm);
            if (stunted >= Severity)
                return;

            Severity = stunted;
            BodyUtility.UpdateWeight(pawn);
        }
    }
}
