using System.Collections.Generic;
using RimWorld;
using Verse;
using TraitDefOf = RimRound.Defs.TraitDefOf;

namespace RimRound.Utilities
{
    public static class WeightOpinionUtility
    {
        public const string ExclusionTag = "RR_Trait_WeightOpinion";

        /// <summary>Opinion traits and how likely a new pawn is to roll each one.</summary>
        static readonly List<Pair<TraitDef, float>> TraitCommonality = new List<Pair<TraitDef, float>>
        {
            new Pair<TraitDef, float>(TraitDefOf.RR_WeightOpinion_Hate_Trait,         0.08f),
            new Pair<TraitDef, float>(TraitDefOf.RR_WeightOpinion_Dislike_Trait,      0.17f),
            new Pair<TraitDef, float>(TraitDefOf.RR_WeightOpinion_NeutralMinus_Trait, 0.28f),
            new Pair<TraitDef, float>(TraitDefOf.RR_WeightOpinion_Neutral_Trait,      0.17f),
            new Pair<TraitDef, float>(TraitDefOf.RR_WeightOpinion_NeutralPlus_Trait,  0.14f),
            new Pair<TraitDef, float>(TraitDefOf.RR_WeightOpinion_Like_Trait,         0.08f),
            new Pair<TraitDef, float>(TraitDefOf.RR_WeightOpinion_Love_Trait,         0.06f),
            new Pair<TraitDef, float>(TraitDefOf.RR_WeightOpinion_Fanatical_Trait,    0.03f),
        };

        /// <summary>
        /// Upper severity bound of each opinion thought stage. Severities above the last bound
        /// use the final stage (Gigantic II).
        /// </summary>
        static readonly float[] ThoughtStageMaxSeverity =
        {
            0.010f, 0.020f, 0.035f, 0.050f, 0.070f, 0.095f, 0.120f, 0.150f,
            0.190f, 0.235f, 0.295f, 0.360f, 0.440f, 0.530f, 0.660f, 0.800f,
        };

        public static TraitDef WeightOpinionTrait(Pawn pawn)
        {
            List<Trait> traits = pawn?.story?.traits?.allTraits;
            if (traits is null)
                return null;

            foreach (Trait trait in traits)
            {
                if (trait.def.exclusionTags?.Contains(ExclusionTag) == true)
                    return trait.def;
            }
            return null;
        }

        public static void AssignWeightOpinionIfMissing(Pawn pawn)
        {
            if (!WeightUtility.CanHaveWeight(pawn) || pawn.story?.traits is null || WeightOpinionTrait(pawn) != null)
                return;

            TraitDef trait = TraitCommonality.RandomElementByWeight(x => x.Second).First;
            pawn.story.traits.GainTrait(new Trait(trait, 0, true));
        }

        public static void SetWeightOpinion(Pawn pawn, TraitDef trait)
        {
            if (pawn.story?.traits is null)
                return;

            TraitDef current = WeightOpinionTrait(pawn);
            if (current != null)
                pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(current));

            pawn.story.traits.GainTrait(new Trait(trait, 0, true));
        }

        /// <returns>The next opinion trait from hate to fanatical, wrapping around.</returns>
        public static TraitDef NextWeightOpinion(TraitDef current)
        {
            int index = TraitCommonality.FindIndex(x => x.First == current);
            return TraitCommonality[(index + 1) % TraitCommonality.Count].First;
        }

        public static int ThoughtStageIndex(float weightSeverity)
        {
            for (int i = 0; i < ThoughtStageMaxSeverity.Length; ++i)
            {
                if (weightSeverity <= ThoughtStageMaxSeverity[i])
                    return i;
            }
            return ThoughtStageMaxSeverity.Length;
        }

        /// <summary>
        /// Pawns who like being heavier start out a bit heavier, and vice versa.
        /// </summary>
        public static float StartingWeightBonusKilos(Pawn pawn)
        {
            TraitDef trait = WeightOpinionTrait(pawn);
            if (trait == TraitDefOf.RR_WeightOpinion_Hate_Trait)        return Rand.Range(-20f, -10f);
            if (trait == TraitDefOf.RR_WeightOpinion_Dislike_Trait)     return Rand.Range(-10f, 0f);
            if (trait == TraitDefOf.RR_WeightOpinion_Neutral_Trait)     return Rand.Range(0f, 10f);
            if (trait == TraitDefOf.RR_WeightOpinion_NeutralPlus_Trait) return Rand.Range(0f, 20f);
            if (trait == TraitDefOf.RR_WeightOpinion_Like_Trait)        return Rand.Range(0f, 30f);
            if (trait == TraitDefOf.RR_WeightOpinion_Love_Trait)        return Rand.Range(20f, 70f);
            if (trait == TraitDefOf.RR_WeightOpinion_Fanatical_Trait)   return Rand.Range(20f, 100f);
            return 0f;
        }
    }
}
