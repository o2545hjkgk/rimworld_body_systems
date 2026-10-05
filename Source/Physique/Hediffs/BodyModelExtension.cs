using Verse;

namespace Physique.Hediffs
{
    /// <summary>
    /// Body model constants, in adult-equivalent units for body size 1. Lives on Physique_Weight.
    /// The masses are for an adult of <see cref="referenceHeightCm"/> and scale with height squared.
    /// </summary>
    public class BodyModelExtension : DefModExtension
    {
        public float referenceHeightCm = 175f;

        /// <summary>Bone, organs, blood, skin: everything but skeletal muscle and fat.</summary>
        public float frameKg = 30f;

        public float baselineMuscleKg = 26f;

        /// <summary>Fat the body won't burn for energy. Below it, starvation wastes muscle instead.</summary>
        public float essentialFatKg = 3f;

        /// <summary>Weight cap as BMI, so it applies equally at every height.</summary>
        public float maxBmi = 322f;

        public float maleHeightCm = 175f;
        public float maleHeightSdCm = 7f;
        public float femaleHeightCm = 162f;
        public float femaleHeightSdCm = 6.5f;

        /// <summary>Adult height lost per day of full-severity malnutrition while the pawn is still growing.</summary>
        public float stuntingCmPerDay = 0.1f;

        /// <summary>Most adult height childhood malnutrition can take away.</summary>
        public float maxStuntingCm = 12f;
    }
}
