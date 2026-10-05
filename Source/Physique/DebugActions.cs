using LudeonTK;
using Physique.Utilities;
using Verse;

namespace Physique
{
    public static class DebugActions
    {
        [DebugAction("Physique", "Add 10 kg fat", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Add10KilosFat(Pawn pawn) => ChangeFat(pawn, 10f);

        [DebugAction("Physique", "Remove 10 kg fat", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Remove10KilosFat(Pawn pawn) => ChangeFat(pawn, -10f);

        [DebugAction("Physique", "Add 100 kg fat", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Add100KilosFat(Pawn pawn) => ChangeFat(pawn, 100f);

        [DebugAction("Physique", "Add 5 kg muscle", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Add5KilosMuscle(Pawn pawn) => ChangeMuscle(pawn, 5f);

        [DebugAction("Physique", "Remove 5 kg muscle", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void Remove5KilosMuscle(Pawn pawn) => ChangeMuscle(pawn, -5f);

        [DebugAction("Physique", "Cycle weight opinion", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void CycleWeightOpinion(Pawn pawn)
        {
            if (!BodyUtility.CanHaveBody(pawn))
                return;

            WeightOpinionUtility.SetWeightOpinion(pawn, WeightOpinionUtility.NextWeightOpinion(WeightOpinionUtility.WeightOpinionTrait(pawn)));
        }

        static void ChangeFat(Pawn pawn, float kilos)
        {
            BodyUtility.EnsureBody(pawn);
            BodyUtility.ChangeFat(pawn, kilos);
        }

        static void ChangeMuscle(Pawn pawn, float kilos)
        {
            BodyUtility.EnsureBody(pawn);
            BodyUtility.ChangeMuscle(pawn, kilos);
        }
    }
}
