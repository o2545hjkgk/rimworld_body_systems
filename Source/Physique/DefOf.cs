using RimWorld;
using Verse;

namespace Physique.Defs
{
    [DefOf]
    public static class HediffDefOf
    {
        public static HediffDef Physique_Weight;
        public static HediffDef Physique_Fat;
        public static HediffDef Physique_Muscle;

        static HediffDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }

    [DefOf]
    public static class TraitDefOf
    {
        public static TraitDef Physique_WeightOpinion_Hate;
        public static TraitDef Physique_WeightOpinion_Dislike;
        public static TraitDef Physique_WeightOpinion_NeutralMinus;
        public static TraitDef Physique_WeightOpinion_Neutral;
        public static TraitDef Physique_WeightOpinion_NeutralPlus;
        public static TraitDef Physique_WeightOpinion_Like;
        public static TraitDef Physique_WeightOpinion_Love;
        public static TraitDef Physique_WeightOpinion_Fanatical;

        static TraitDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
    }
}
