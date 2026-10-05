using Physique.Core;
using Xunit;

namespace Physique.Tests
{
    public class BodyMathTests
    {
        static BodyComposition Body(float muscle, float fat) => new BodyComposition(175f, 30f, muscle, fat);

        [Fact]
        public void WeightIsFramePlusMusclePlusFat()
        {
            BodyComposition body = Body(26f, 14f);
            Assert.Equal(70f, body.WeightKg, 3);
            Assert.Equal(56f, body.LeanKg, 3);
            Assert.Equal(0.2f, body.BodyFatFraction, 3);
        }

        [Theory]
        [InlineData(70f, 175f, 22.86f)]
        [InlineData(825f, 175f, 269.39f)]   // Gigantic II used to start at 825 kg
        [InlineData(70f, 160f, 27.34f)]
        public void BmiIsWeightOverHeightSquared(float kilos, float heightCm, float bmi)
        {
            Assert.Equal(bmi, BodyMath.Bmi(kilos, heightCm), 2);
        }

        [Fact]
        public void RealKilosScaleWithBodySize()
        {
            Assert.Equal(35f, BodyMath.ToRealKilos(70f, 0.5f), 3);
            Assert.Equal(20f, BodyMath.ToAdultKilos(10f, 0.5f), 3);
            Assert.Equal(12.5f, BodyMath.ToRealKilos(BodyMath.ToAdultKilos(12.5f, 0.8f), 0.8f), 3);
        }

        [Fact]
        public void BodySizeHasAFloor()
        {
            Assert.Equal(BodyMath.MinBodySizeFactor, BodyMath.BodySizeFactor(0f));
            Assert.True(float.IsFinite(BodyMath.ToAdultKilos(1f, 0f)));
        }

        [Fact]
        public void FormatsKilosAndPounds()
        {
            Assert.Equal("100.0 kg", BodyMath.FormatWeight(100f, false));
            Assert.Equal("220.5 lbs", BodyMath.FormatWeight(100f, true));
        }

        [Theory]
        [InlineData(10f, 0)]
        [InlineData(11.4f, 0)]
        [InlineData(11.5f, 1)]
        [InlineData(22.9f, 3)]
        [InlineData(269.4f, 15)]
        [InlineData(269.5f, 16)]
        [InlineData(322f, 16)]
        public void OpinionMoodStageCoversEveryBmiUpToTheCap(float bmi, int stage)
        {
            Assert.Equal(stage, BodyMath.OpinionMoodStageIndex(bmi));
        }

        [Fact]
        public void StartingWeightAboveLeanMassBecomesFat()
        {
            BodyComposition body = BodyMath.StartingComposition(175f, 30f, 26f, 3f, 90f);
            Assert.Equal(26f, body.MuscleKg);
            Assert.Equal(34f, body.FatKg, 3);
            Assert.Equal(90f, body.WeightKg, 3);
        }

        [Fact]
        public void StartingWeightBelowLeanMassKeepsEssentialFat()
        {
            BodyComposition body = BodyMath.StartingComposition(175f, 30f, 26f, 3f, 45f);
            Assert.Equal(26f, body.MuscleKg);
            Assert.Equal(3f, body.FatKg);
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
        [InlineData(0.30f, false, 14f, BodyMath.FastingResponse.None)]        // not hungry
        [InlineData(0.20f, false, 14f, BodyMath.FastingResponse.BurnFat)]     // hungry, has fat to spare
        [InlineData(0.00f, true, 14f, BodyMath.FastingResponse.BurnFat)]      // fat still covers it
        [InlineData(0.20f, false, 3f, BodyMath.FastingResponse.None)]         // hungry, essential fat only: vanilla
        [InlineData(0.00f, true, 3f, BodyMath.FastingResponse.WasteMuscle)]   // starving, essential fat only
        [InlineData(0.00f, true, 2f, BodyMath.FastingResponse.WasteMuscle)]
        public void FastingBurnsFatBeforeMuscle(float foodPct, bool starving, float fatKg, BodyMath.FastingResponse expected)
        {
            Assert.Equal(expected, BodyMath.Fasting(foodPct, 0.25f, starving, fatKg, 3f));
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
