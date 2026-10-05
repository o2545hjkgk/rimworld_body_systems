using LudeonTK;
using RimRound.Utilities;
using Verse;

namespace RimRound
{
    public static class DebugActions
    {
        [DebugAction("RimRound", "Add 10 kg", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Add10Kilos(Pawn pawn) => ChangeWeight(pawn, 10f);

        [DebugAction("RimRound", "Remove 10 kg", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Remove10Kilos(Pawn pawn) => ChangeWeight(pawn, -10f);

        [DebugAction("RimRound", "Add 100 kg", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Add100Kilos(Pawn pawn) => ChangeWeight(pawn, 100f);

        [DebugAction("RimRound", "Cycle weight opinion", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
