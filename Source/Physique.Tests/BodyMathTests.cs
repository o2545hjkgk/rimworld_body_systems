using Physique.Core;
using Xunit;

namespace Physique.Tests
{
    public class BodyMathTests
    {
        const float BaseWeight = 25f;

        [Theory]
        [InlineData(0.001f, 26f)]
        [InlineData(0.035f, 60f)]   // Thick
        [InlineData(0.800f, 825f)]  // Gigantic II
        [InlineData(0.964f, 989f)]  // max severity
        public void AdultSeverityMapsToKilos(float severity, float kilos)
        {
            Assert.Equal(kilos, BodyMath.SeverityToAdultKilos(severity, BaseWeight), 3);
            Assert.Equal(severity, BodyMath.AdultKilosToSeverity(kilos, BaseWeight), 5);
        }

        [Fact]
        public void KilosScaleWithBodySize()
        {
            Assert.Equal(40f, BodyMath.SeverityToKilos(0.055f, BaseWeight, 0.5f), 3);
            Assert.Equal(0.020f, BodyMath.SeverityDeltaForKilos(10f, 0.5f), 5);
        }

        [Fact]
        public void BodySizeHasAFloor()
        {
            Assert.Equal(BodyMath.MinBodySizeFactor, BodyMath.BodySizeFactor(0f));
            Assert.True(float.IsFinite(BodyMath.SeverityDeltaForKilos(1f, 0f)));
        }

        [Fact]
        public void GainingThenLosingTheSameKilosIsANoOp()
        {
            const float bodySize = 0.8f;
            float severity = 0.1f + BodyMath.SeverityDeltaForKilos(12.5f, bodySize) + BodyMath.SeverityDeltaForKilos(-12.5f, bodySize);
            Assert.Equal(0.1f, severity, 5);
        }

        [Fact]
        public void FormatsKilosAndPounds()
        {
            Assert.Equal("100.0 kg", BodyMath.FormatWeight(100f, false));
            Assert.Equal("220.5 lbs", BodyMath.FormatWeight(100f, true));
        }

        [Theory]
        [InlineData(0.0f, 0)]
        [InlineData(0.010f, 0)]
        [InlineData(0.011f, 1)]
        [InlineData(0.800f, 15)]
        [InlineData(0.801f, 16)]
        [InlineData(0.964f, 16)]
        public void OpinionMoodStageCoversEveryWeightUpToTheCap(float severity, int stage)
        {
            Assert.Equal(stage, BodyMath.OpinionMoodStageIndex(severity));
        }

        [Theory]
        [InlineData(0.9f, 0.3f, 1.0f, 0.2f)]   // meal at 30% overshoots by 0.2
        [InlineData(0.9f, 0.05f, 1.0f, 0f)]    // fits
        [InlineData(0.5f, 1.0f, 1.0f, 0.5f)]   // already full
        public void SurplusIsWhatDoesNotFitInTheFoodNeed(float nutrition, float curLevel, float maxLevel, float surplus)
        {
            Assert.Equal(surplus, BodyMath.SurplusNutrition(nutrition, curLevel, maxLevel), 5);
        }

        [Theory]
        [InlineData(0.30f, false, 120f, BodyMath.FastingResponse.None)]           // not hungry
        [InlineData(0.20f, false, 120f, BodyMath.FastingResponse.BurnFatForFood)] // hungry, has reserves
        [InlineData(0.00f, true, 120f, BodyMath.FastingResponse.BurnFatForFood)]  // reserves still cover it
        [InlineData(0.20f, false, 55f, BodyMath.FastingResponse.None)]            // hungry, lean: vanilla
        [InlineData(0.00f, true, 55f, BodyMath.FastingResponse.WasteAway)]        // starving, lean
        [InlineData(0.00f, true, 60f, BodyMath.FastingResponse.WasteAway)]        // floor itself is not a reserve
        public void FastingResponseDependsOnHungerAndReserves(float foodPct, bool starving, float adultKilos, BodyMath.FastingResponse expected)
        {
            Assert.Equal(expected, BodyMath.Fasting(foodPct, 0.25f, starving, adultKilos, 60f));
        }

        static readonly (float, float)[] Distribution =
        {
            (0.2f, 65f),
            (0.5f, 70f),
            (1.0f, 100f),
        };

        [Theory]
        [InlineData(0.0f, 0.0f, 65f)]
        [InlineData(0.1f, 1.0f, 70f)]   // first band spans the first two points
        [InlineData(0.3f, 0.5f, 67.5f)]
        [InlineData(0.9f, 0.5f, 85f)]
        [InlineData(1.0f, 1.0f, 100f)]
        public void SamplesBetweenNeighbouringPoints(float roll, float lerpRoll, float kilos)
        {
            Assert.Equal(kilos, BodyMath.SampleWeightDistribution(Distribution, roll, lerpRoll), 3);
        }

        [Fact]
        public void RollPastTheLastPointReturnsTheLastWeight()
        {
            Assert.Equal(100f, BodyMath.SampleWeightDistribution(Distribution, 1.5f, 0f));
        }
    }
}
