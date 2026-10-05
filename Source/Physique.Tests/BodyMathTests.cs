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
        public void StartingBmiAboveLeanMassBecomesFat()
        {
            BodyComposition body = BodyMath.StartingComposition(175f, 30f, 26f, 3f, 30f, 322f);
            Assert.Equal(26f, body.MuscleKg);
            Assert.Equal(91.875f - 56f, body.FatKg, 3);
            Assert.Equal(30f, body.Bmi, 3);
        }

        [Fact]
        public void StartingBmiBelowLeanMassKeepsEssentialFat()
        {
            BodyComposition body = BodyMath.StartingComposition(175f, 30f, 26f, 3f, 14f, 322f);
            Assert.Equal(26f, body.MuscleKg);
            Assert.Equal(3f, body.FatKg);
        }

        [Fact]
        public void StartingBmiIsCapped()
        {
            Assert.Equal(322f, BodyMath.StartingComposition(175f, 30f, 26f, 3f, 400f, 322f).Bmi, 3);
        }

        [Theory]
        [InlineData(175f, 30f, 26f, 322f, 986.1f - 56f)]
        [InlineData(195f, 37.25f, 32.28f, 322f, 1224.4f - 69.53f)]   // taller pawns can carry more before hitting the cap
        [InlineData(175f, 30f, 26f, 15f, 0f)]                       // already past the cap
        public void MaxFatStopsAtTheBmiCap(float heightCm, float frameKg, float muscleKg, float maxBmi, float expected)
        {
            Assert.Equal(expected, BodyMath.MaxFatKg(heightCm, frameKg, muscleKg, maxBmi), 0);
        }

        [Theory]
        [InlineData(0f, 175f)]
        [InlineData(1f, 182f)]
        [InlineData(-2f, 161f)]
        [InlineData(10f, 196f)]    // clamped to +3 SD
        [InlineData(-10f, 154f)]   // clamped to -3 SD
        public void AdultHeightFollowsTheDistribution(float z, float expectedCm)
        {
            Assert.Equal(expectedCm, BodyMath.AdultHeightCm(175f, 7f, z), 3);
        }

        [Theory]
        [InlineData(0f, 18f, 0.29f)]
        [InlineData(3f, 18f, 0.55f)]
        [InlineData(4f, 18f, 0.59f)]     // between 3 and 5
        [InlineData(18f, 18f, 1f)]
        [InlineData(40f, 18f, 1f)]
        [InlineData(5f, 10f, 0.76f)]     // a race adult at 10 is "9 in human years" at 5: between 8 (0.73) and 10 (0.79)
        public void GrowthFollowsTheHumanCurve(float age, float adultAge, float fraction)
        {
            Assert.Equal(fraction, BodyMath.GrowthFraction(age, adultAge), 3);
        }

        [Fact]
        public void GrowthNeverShrinks()
        {
            float previous = 0f;
            for (float age = 0f; age <= 20f; age += 0.25f)
            {
                float fraction = BodyMath.GrowthFraction(age, 18f);
                Assert.True(fraction >= previous, $"Shrank at age {age}");
                previous = fraction;
            }
        }

        [Fact]
        public void LeanMassScalesWithHeightSquared()
        {
            Assert.Equal(1f, BodyMath.MassScaleForHeight(175f, 175f), 5);
            Assert.Equal(0.857f, BodyMath.MassScaleForHeight(162f, 175f), 3);
            // A taller and a shorter body built the same way have the same BMI.
            var tall = new BodyComposition(190f, 30f * BodyMath.MassScaleForHeight(190f, 175f), 26f * BodyMath.MassScaleForHeight(190f, 175f), 14f * BodyMath.MassScaleForHeight(190f, 175f));
            var shortBody = new BodyComposition(155f, 30f * BodyMath.MassScaleForHeight(155f, 175f), 26f * BodyMath.MassScaleForHeight(155f, 175f), 14f * BodyMath.MassScaleForHeight(155f, 175f));
            Assert.Equal(tall.Bmi, shortBody.Bmi, 3);
        }

        [Fact]
        public void RaceHeightScalesWithTheCubeRootOfBodySize()
        {
            Assert.Equal(1f, BodyMath.RaceHeightScale(1f), 5);
            Assert.Equal(2f, BodyMath.RaceHeightScale(8f), 4);
        }

        [Fact]
        public void StuntingIsProportionalToSeverityAndTime()
        {
            Assert.Equal(0.5f, BodyMath.StuntingCm(0.5f, 10f, 0.1f), 5);
            Assert.Equal(0f, BodyMath.StuntingCm(-1f, 10f, 0.1f));
        }

        [Theory]
        [InlineData(175f, false, "1.75 m")]
        [InlineData(175f, true, "5'9\"")]
        [InlineData(152.4f, true, "5'0\"")]
        public void FormatsHeight(float cm, bool imperial, string expected)
        {
            Assert.Equal(expected, BodyMath.FormatHeight(cm, imperial));
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
            Assert.Equal(kilos, BodyMath.SampleDistribution(Distribution, roll, lerpRoll), 3);
        }

        [Fact]
        public void RollPastTheLastPointReturnsTheLastWeight()
        {
            Assert.Equal(100f, BodyMath.SampleDistribution(Distribution, 1.5f, 0f));
        }
    }
}
