using System.IO;
using System.Linq;
using System.Xml.Linq;
using Physique.Core;
using Xunit;

namespace Physique.Tests
{
    /// <summary>Checks the XML defs agree with each other and with the code that reads them.</summary>
    public class DefsTests
    {
        static readonly string DefsDir = Path.Combine(RepoRoot(), "1.6", "Defs");

        static string RepoRoot()
        {
            var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "About", "About.xml")))
                dir = dir.Parent;
            Assert.NotNull(dir);
            return dir.FullName;
        }

        static XElement HediffDef(string file, string defName) =>
            XDocument.Load(Path.Combine(DefsDir, "HediffDefs", file)).Root
                .Elements("HediffDef").Single(d => (string)d.Element("defName") == defName);

        static XElement Weight => HediffDef("Physique_Weight.xml", "Physique_Weight");
        static XElement Fat => HediffDef("Physique_BodyComposition.xml", "Physique_Fat");
        static XElement Muscle => HediffDef("Physique_BodyComposition.xml", "Physique_Muscle");
        static XElement Model => Weight.Element("modExtensions").Element("li");

        static float ModelValue(string name) => (float)Model.Element(name);

        static float StageMin(XElement stage) => (float?)stage.Element("minSeverity") ?? 0f;

        static string StageForBmi(float bmi) =>
            (string)Weight.Element("stages").Elements("li").Last(s => bmi >= StageMin(s)).Element("label");

        /// <summary>Weight stage a reference-height adult of this composition would be in.</summary>
        static string StageFor(float muscleKg, float fatKg) =>
            StageForBmi(new BodyComposition(ModelValue("referenceHeightCm"), ModelValue("frameKg"), muscleKg, fatKg).Bmi);

        [Fact]
        public void WeightStagesAscendAndEndAtGiganticII()
        {
            var stages = Weight.Element("stages").Elements("li").ToList();
            float[] minBmis = stages.Select(StageMin).ToArray();

            Assert.Equal(17, stages.Count);
            Assert.Equal("Gigantic II", (string)stages.Last().Element("label"));
            Assert.Equal(minBmis.OrderBy(x => x), minBmis);
            Assert.Equal(minBmis.Length, minBmis.Distinct().Count());
        }

        [Fact]
        public void BmiCapIsInsideGiganticIIAndBelowTitanic()
        {
            float maxBmi = ModelValue("maxBmi");
            Assert.Equal("Gigantic II", StageForBmi(maxBmi));
            Assert.True(BodyMath.WeightForBmi(maxBmi, ModelValue("referenceHeightCm")) < 990f, "Cap must stay below where Titanic began.");
        }

        [Theory]
        [InlineData(154f)]
        [InlineData(196f)]
        public void EveryHeightCanReachGiganticIIWithinTheFatSafetyLimit(float heightCm)
        {
            float scale = BodyMath.MassScaleForHeight(heightCm, ModelValue("referenceHeightCm"));
            float maxFat = BodyMath.MaxFatKg(heightCm, ModelValue("frameKg") * scale, ModelValue("baselineMuscleKg") * scale, ModelValue("maxBmi"));
            Assert.True(maxFat < (float)Fat.Element("maxSeverity"), "The fat hediff's safety limit must not cut in before the BMI cap.");
        }

        [Fact]
        public void HeightDistributionsFitTheHeightLimits()
        {
            XElement height = HediffDef("Physique_BodyComposition.xml", "Physique_Height");
            foreach (string sex in new[] { "male", "female" })
            {
                float mean = ModelValue(sex + "HeightCm"), sd = ModelValue(sex + "HeightSdCm");
                Assert.InRange(BodyMath.AdultHeightCm(mean, sd, -3f) - ModelValue("maxStuntingCm"), (float)height.Element("minSeverity"), (float)height.Element("maxSeverity"));
                Assert.InRange(BodyMath.AdultHeightCm(mean, sd, 3f), (float)height.Element("minSeverity"), (float)height.Element("maxSeverity"));
            }
        }

        [Fact]
        public void FastingDownToEssentialFatLeavesAHealthyWeight()
        {
            Assert.Equal("Thick", StageFor(ModelValue("baselineMuscleKg"), ModelValue("essentialFatKg")));
        }

        [Fact]
        public void StarvationWastingCanReachEmaciated()
        {
            float minMuscle = (float)Muscle.Element("minSeverity");
            Assert.Equal("Emaciated", StageFor(minMuscle, ModelValue("essentialFatKg")));
        }

        [Fact]
        public void BaselineMuscleIsWithinMuscleLimits()
        {
            float baseline = ModelValue("baselineMuscleKg");
            Assert.InRange(baseline, (float)Muscle.Element("minSeverity"), (float)Muscle.Element("maxSeverity"));
            Assert.Equal(baseline, (float)Muscle.Element("initialSeverity"));
        }

        [Fact]
        public void EveryOpinionHasATraitAndAMoodWithOneStagePerWeightBand()
        {
            var traits = XDocument.Load(Path.Combine(DefsDir, "TraitDefs", "Physique_WeightOpinionTraits.xml"))
                .Root.Elements("TraitDef").Select(t => (string)t.Element("defName")).ToHashSet();
            var moods = XDocument.Load(Path.Combine(DefsDir, "ThoughtDefs", "Physique_WeightOpinionThoughts.xml"))
                .Root.Elements("ThoughtDef").ToList();

            Assert.Equal(8, traits.Count);
            Assert.Equal(8, moods.Count);
            foreach (XElement mood in moods)
            {
                string requiredTrait = (string)mood.Element("requiredTraits").Element("li");
                Assert.Contains(requiredTrait, traits);
                Assert.Equal(BodyMath.OpinionMoodStageMaxBmi.Length + 1, mood.Element("stages").Elements("li").Count());
            }
        }

        [Fact]
        public void NoLegacyNamesRemain()
        {
            foreach (string file in Directory.EnumerateFiles(Path.Combine(RepoRoot(), "1.6"), "*.xml", SearchOption.AllDirectories))
            {
                string text = File.ReadAllText(file);
                Assert.DoesNotContain("RimRound_", text);
                Assert.DoesNotContain("RR_", text);
                Assert.DoesNotContain("RimRound.", text);
            }
        }
    }
}
