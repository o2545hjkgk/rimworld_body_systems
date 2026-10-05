using HarmonyLib;
using Physique.Utilities;
using Verse;

namespace Physique.Patches
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
    public static class PawnGenerator_GenerateTraits_AddWeightOpinion
    {
        public static void Postfix(Pawn pawn)
        {
            WeightOpinionUtility.AssignWeightOpinionIfMissing(pawn);
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateInitialHediffs")]
    public static class PawnGenerator_GenerateInitialHediffs_AddBody
    {
        public static void Postfix(Pawn pawn)
        {
            // The starting weight depends on the opinion, so make sure it exists first.
            WeightOpinionUtility.AssignWeightOpinionIfMissing(pawn);
            BodyUtility.EnsureBody(pawn);
        }
    }

    /// <summary>Covers pawns that existed before the mod was added to a save.</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Pawn_SpawnSetup_EnsureBody
    {
        public static void Postfix(Pawn __instance)
        {
            WeightOpinionUtility.AssignWeightOpinionIfMissing(__instance);
            BodyUtility.EnsureBody(__instance);
        }
    }
}
