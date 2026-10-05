using Physique.Core;
using Physique.Utilities;
using RimWorld;
using Verse;

namespace Physique.Hediffs
{
    /// <summary>
    /// Body weight. Weight is gained from food eaten past a full stomach (see Thing_Ingested_GainWeightFromSurplus)
    /// and lost here, when the pawn goes hungry and has fat to burn.
    /// </summary>
    public class Hediff_Weight : Hediff
    {
        const int MetabolismIntervalTicks = 250;

        public override string SeverityLabel => BodyMath.FormatWeight(WeightUtility.SeverityToKilos(Severity, pawn), PhysiqueMod.Settings.usePounds);

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            if (pawn.IsHashIntervalTick(MetabolismIntervalTicks, delta))
                BurnFatIfHungry(MetabolismIntervalTicks);
        }

        /// <summary>
        /// While the pawn is hungry its body runs on stored fat. Above the fat reserve floor this
        /// covers the food need, so the pawn stays hungry rather than starving and slims down.
        /// At or below the floor, vanilla starvation runs its course and the pawn keeps wasting away.
        /// </summary>
        void BurnFatIfHungry(int ticks)
        {
            Need_Food food = pawn.needs?.food;
            if (food is null)
                return;

            BodyMath.FastingResponse response = BodyMath.Fasting(
                food.CurLevelPercentage,
                food.PercentageThreshHungry,
                food.CurCategory == HungerCategory.Starving,
                pawn.AdultEquivalentWeight(),
                WeightUtility.Extension.fatReserveFloor);

            if (response == BodyMath.FastingResponse.None)
                return;

            float nutritionBurned = food.FoodFallPerTickAssumingCategory(HungerCategory.Fed, true) * ticks;
            if (response == BodyMath.FastingResponse.BurnFatForFood)
                food.CurLevel += nutritionBurned;

            WeightUtility.ChangeWeight(pawn, -nutritionBurned * PhysiqueMod.Settings.kgPerNutrition * PhysiqueMod.Settings.weightLossMultiplier);
        }
    }
}
