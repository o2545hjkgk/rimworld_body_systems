using HarmonyLib;
using Physique.Core;
using Physique.Utilities;
using Verse;

namespace Physique.Patches
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

            float surplus = BodyMath.SurplusNutrition(__result, ingester.needs.food.CurLevel, ingester.needs.food.MaxLevel);
            if (surplus <= 0f)
                return;

            WeightUtility.ChangeWeight(ingester, surplus * PhysiqueMod.Settings.kgPerNutrition * PhysiqueMod.Settings.weightGainMultiplier);
        }
    }
}
