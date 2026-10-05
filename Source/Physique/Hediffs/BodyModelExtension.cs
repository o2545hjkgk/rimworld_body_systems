using Verse;

namespace Physique.Hediffs
{
    /// <summary>Body model constants, in adult-equivalent units for body size 1. Lives on Physique_Weight.</summary>
    public class BodyModelExtension : DefModExtension
    {
        /// <summary>Height everyone is assumed to have until growth is modelled.</summary>
        public float referenceHeightCm = 175f;

        /// <summary>Bone, organs, blood, skin: everything but skeletal muscle and fat.</summary>
        public float frameKg = 30f;

        public float baselineMuscleKg = 26f;

        /// <summary>Fat the body won't burn for energy. Below it, starvation wastes muscle instead.</summary>
        public float essentialFatKg = 3f;
    }
}
