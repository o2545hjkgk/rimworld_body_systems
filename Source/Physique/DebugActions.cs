using LudeonTK;
using Physique.Utilities;
using Verse;

namespace Physique
{
    public static class DebugActions
    {
        [DebugAction("Physique", "Add 10 kg", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Add10Kilos(Pawn pawn) => ChangeWeight(pawn, 10f);

        [DebugAction("Physique", "Remove 10 kg", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Remove10Kilos(Pawn pawn) => ChangeWeight(pawn, -10f);

        [DebugAction("Physique", "Add 100 kg", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Add100Kilos(Pawn pawn) => ChangeWeight(pawn, 100f);

        [DebugAction("Physique", "Cycle weight opinion", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void CycleWeightOpinion(Pawn pawn)
        {
            if (!WeightUtility.CanHaveWeight(pawn))
                return;

            WeightOpinionUtility.SetWeightOpinion(pawn, WeightOpinionUtility.NextWeightOpinion(WeightOpinionUtility.WeightOpinionTrait(pawn)));
        }

        static void ChangeWeight(Pawn pawn, float kilos)
        {
            WeightUtility.EnsureWeightHediff(pawn);
            WeightUtility.ChangeWeight(pawn, kilos);
        }
    }
}
