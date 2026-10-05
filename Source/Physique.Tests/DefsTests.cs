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

        static XElement WeightHediff() =>
            XDocument.Load(Path.Combine(DefsDir, "HediffDefs", "Physique_Weight.xml")).Root.Element("HediffDef");

        [Fact]
        public void WeightStagesAscendAndEndAtGiganticII()
        {
            var stages = WeightHediff().Element("stages").Elements("li").ToList();
            float[] minSeverities = stages.Select(s => (float?)s.Element("minSeverity") ?? 0f).ToArray();

            Assert.Equal(17, stages.Count);
            Assert.Equal("Gigantic II", (string)stages.Last().Element("label"));
            Assert.Equal(minSeverities.OrderBy(x => x), minSeverities);
            Assert.Equal(minSeverities.Length, minSeverities.Distinct().Count());
        }

        [Fact]
        public void MaxWeightFallsInsideTheLastStage()
        {
            XElement hediff = WeightHediff();
            float max = (float)hediff.Element("maxSeverity");
            float lastStageMin = (float)hediff.Element("stages").Elements("li").Last().Element("minSeverity");
            float baseWeight = (float)hediff.Element("modExtensions").Element("li").Element("baseWeight");

            Assert.True(max >= lastStageMin);
            Assert.True(BodyMath.SeverityToAdultKilos(max, baseWeight) < 990f, "Cap must stay below where Titanic began.");
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
                Assert.Equal(BodyMath.OpinionMoodStageMaxSeverity.Length + 1, mood.Element("stages").Elements("li").Count());
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
