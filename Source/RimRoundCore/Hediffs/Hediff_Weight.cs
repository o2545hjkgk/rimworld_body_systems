using RimRound.Utilities;
using RimWorld;
using Verse;

namespace RimRound.Hediffs
{
    /// <summary>
    /// Body weight. Weight is gained from food eaten past a full stomach (see Thing_Ingested_GainWeightFromSurplus)
    /// and lost here, when the pawn goes hungry and has fat to burn.
    /// </summary>
    public class Hediff_Weight : Hediff
    {
        const int MetabolismIntervalTicks = 250;

        public override string SeverityLabel => WeightUtility.FormatWeight(WeightUtility.SeverityToKilos(Severity, pawn));

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
            if (food is null || food.CurLevelPercentage >= food.PercentageThreshHungry)
                return;

            float nutritionBurned = food.FoodFallPerTickAssumingCategory(HungerCategory.Fed, true) * ticks;
            float kilosBurned = nutritionBurned * RimRoundMod.Settings.kgPerNutrition * RimRoundMod.Settings.weightLossMultiplier;

            if (pawn.AdultEquivalentWeight() > WeightUtility.Extension.fatReserveFloor)
            {
                food.CurLevel += nutritionBurned;
                WeightUtility.ChangeWeight(pawn, -kilosBurned);
            }
            else if (food.CurCategory == HungerCategory.Starving)
            {
                WeightUtility.ChangeWeight(pawn, -kilosBurned);
            }
        }
    }
}
