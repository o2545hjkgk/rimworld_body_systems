using System.Collections.Generic;
using RimWorld;
using Verse;
using TraitDefOf = Physique.Defs.TraitDefOf;

namespace Physique.Utilities
{
    public static class WeightOpinionUtility
    {
        public const string ExclusionTag = "Physique_WeightOpinion";

        /// <summary>Opinion traits from hate to fanatical, and how likely a new pawn is to roll each one.</summary>
        static readonly List<Pair<TraitDef, float>> TraitCommonality = new List<Pair<TraitDef, float>>
        {
            new Pair<TraitDef, float>(TraitDefOf.Physique_WeightOpinion_Hate,         0.08f),
            new Pair<TraitDef, float>(TraitDefOf.Physique_WeightOpinion_Dislike,      0.17f),
            new Pair<TraitDef, float>(TraitDefOf.Physique_WeightOpinion_NeutralMinus, 0.28f),
            new Pair<TraitDef, float>(TraitDefOf.Physique_WeightOpinion_Neutral,      0.17f),
            new Pair<TraitDef, float>(TraitDefOf.Physique_WeightOpinion_NeutralPlus,  0.14f),
            new Pair<TraitDef, float>(TraitDefOf.Physique_WeightOpinion_Like,         0.08f),
            new Pair<TraitDef, float>(TraitDefOf.Physique_WeightOpinion_Love,         0.06f),
            new Pair<TraitDef, float>(TraitDefOf.Physique_WeightOpinion_Fanatical,    0.03f),
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

        /// <summary>
        /// Pawns who like being heavier start out a bit heavier, and vice versa.
        /// </summary>
        public static float StartingWeightBonusKilos(Pawn pawn)
        {
            TraitDef trait = WeightOpinionTrait(pawn);
            if (trait == TraitDefOf.Physique_WeightOpinion_Hate)        return Rand.Range(-20f, -10f);
            if (trait == TraitDefOf.Physique_WeightOpinion_Dislike)     return Rand.Range(-10f, 0f);
            if (trait == TraitDefOf.Physique_WeightOpinion_Neutral)     return Rand.Range(0f, 10f);
            if (trait == TraitDefOf.Physique_WeightOpinion_NeutralPlus) return Rand.Range(0f, 20f);
            if (trait == TraitDefOf.Physique_WeightOpinion_Like)        return Rand.Range(0f, 30f);
            if (trait == TraitDefOf.Physique_WeightOpinion_Love)        return Rand.Range(20f, 70f);
            if (trait == TraitDefOf.Physique_WeightOpinion_Fanatical)   return Rand.Range(20f, 100f);
            return 0f;
        }
    }
}
