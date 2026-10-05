using RimWorld;
using Verse;

namespace RimRound.Defs
{
    [DefOf]
    public static class HediffDefOf
    {
        public static HediffDef RimRound_Weight;

        static HediffDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }

    [DefOf]
    public static class TraitDefOf
    {
        public static TraitDef RR_WeightOpinion_Hate_Trait;
        public static TraitDef RR_WeightOpinion_Dislike_Trait;
        public static TraitDef RR_WeightOpinion_NeutralMinus_Trait;
        public static TraitDef RR_WeightOpinion_Neutral_Trait;
        public static TraitDef RR_WeightOpinion_NeutralPlus_Trait;
        public static TraitDef RR_WeightOpinion_Like_Trait;
        public static TraitDef RR_WeightOpinion_Love_Trait;
        public static TraitDef RR_WeightOpinion_Fanatical_Trait;

        static TraitDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
    }
}
