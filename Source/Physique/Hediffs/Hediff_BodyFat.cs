using Physique.Core;
using Physique.Utilities;
using RimWorld;
using Verse;

namespace Physique.Hediffs
{
    /// <summary>
    /// Body fat in adult-equivalent kg. Hidden; it shows up through the weight hediff.
    /// Fat is gained from food eaten past a full stomach (see Thing_Ingested_StoreSurplusAsFat)
    /// and lost here, when the pawn goes hungry.
    /// </summary>
    public class Hediff_BodyFat : Hediff
    {
        const int MetabolismIntervalTicks = 250;

        public override bool Visible => false;

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            if (pawn.IsHashIntervalTick(MetabolismIntervalTicks, delta))
                UseReservesIfHungry(MetabolismIntervalTicks);
        }

        /// <summary>
        /// While the pawn is hungry its body runs on stored fat, which covers the food need: the pawn
        /// stays hungry rather than starving, and slims down. Once fat is down to essential levels,
        /// vanilla starvation runs its course and muscle wastes away with it.
        /// </summary>
        void UseReservesIfHungry(int ticks)
        {
            Need_Food food = pawn.needs?.food;
            if (food is null)
                return;

            BodyMath.FastingResponse response = BodyMath.Fasting(
                food.CurLevelPercentage,
                food.PercentageThreshHungry,
                food.CurCategory == HungerCategory.Starving,
                Severity,
                BodyUtility.EssentialFatKg(pawn.HeightHediff()?.Severity ?? BodyUtility.Model.referenceHeightCm));

            if (response == BodyMath.FastingResponse.None)
                return;

            float nutritionBurned = food.FoodFallPerTickAssumingCategory(HungerCategory.Fed, true) * ticks;
            float kilosBurned = nutritionBurned * PhysiqueMod.Settings.kgPerNutrition * PhysiqueMod.Settings.weightLossMultiplier;

            if (response == BodyMath.FastingResponse.BurnFat)
            {
                food.CurLevel += nutritionBurned;
                BodyUtility.ChangeFat(pawn, -kilosBurned);
            }
            else
            {
                BodyUtility.ChangeMuscle(pawn, -kilosBurned);
            }
        }
    }
}
