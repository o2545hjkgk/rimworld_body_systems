using HarmonyLib;
using RimRound.Utilities;
using Verse;

namespace RimRound.Patches
{
    /// <summary>
    /// Nutrition that doesn't fit in the food need (vanilla throws it away) is stored as body weight.
    /// The caller adds the result to the need after Ingested returns, so CurLevel is still the pre-meal level here.
    /// </summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
    public static class Thing_Ingested_GainWeightFromSurplus
    {
        public static void Postfix(Pawn __0, float __result)
        {
            Pawn ingester = __0;
            if (__result <= 0f || !WeightUtility.CanHaveWeight(ingester))
                return;

            float surplus = __result - (ingester.needs.food.MaxLevel - ingester.needs.food.CurLevel);
            if (surplus <= 0f)
                return;

            WeightUtility.ChangeWeight(ingester, surplus * RimRoundMod.Settings.kgPerNutrition * RimRoundMod.Settings.weightGainMultiplier);
        }
    }
}
