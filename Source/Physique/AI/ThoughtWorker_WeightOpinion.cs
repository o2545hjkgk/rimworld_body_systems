using Physique.Core;
using Physique.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Physique.AI
{
    /// <summary>
    /// Mood from how a pawn feels about its own weight. Each opinion has its own ThoughtDef,
    /// tied to its trait through requiredTraits.
    /// </summary>
    public class ThoughtWorker_WeightOpinion : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!PhysiqueMod.Settings.weightOpinionMoodlets || def.requiredTraits.NullOrEmpty() || p.story?.traits is null)
                return false;

            foreach (TraitDef trait in def.requiredTraits)
            {
                if (!p.story.traits.HasTrait(trait))
                    return false;
            }

            Hediff weight = p.WeightHediff();
            if (weight is null)
                return false;

            // Weight severity is BMI.
            int stage = Mathf.Min(BodyMath.OpinionMoodStageIndex(weight.Severity), def.stages.Count - 1);
            return ThoughtState.ActiveAtStage(stage);
        }
    }
}
