using Verse;

namespace RimRound.Hediffs
{
    public class HediffDef_Weight : DefModExtension
    {
        /// <summary>Weight in kg at severity 0, for an adult of body size 1.</summary>
        public float baseWeight = 25f;

        /// <summary>
        /// Adult-equivalent weight in kg above which a hungry pawn burns body fat
        /// to cover its food need instead of starving.
        /// </summary>
        public float fatReserveFloor = 60f;
    }
}
