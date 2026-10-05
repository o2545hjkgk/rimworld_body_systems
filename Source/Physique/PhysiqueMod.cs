using HarmonyLib;
using UnityEngine;
using Verse;

namespace Physique
{
    public class PhysiqueMod : Mod
    {
        public static PhysiqueSettings Settings { get; private set; }

        public PhysiqueMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<PhysiqueSettings>();
            new Harmony("o2545hjkgk.Physique").PatchAll();
        }

        public override string SettingsCategory() => "Physique";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect);
        }
    }

    public class PhysiqueSettings : ModSettings
    {
        // 1 nutrition is roughly 1,500 kcal and 1 kg of body fat is roughly 7,700 kcal,
        // so ~0.2 is the physiological value. 1.0 keeps RimRound's original pacing.
        public float kgPerNutrition = 1f;
        public float weightGainMultiplier = 1f;
        public float weightLossMultiplier = 1f;
        public bool usePounds = false;
        public bool weightOpinionMoodlets = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref kgPerNutrition, "kgPerNutrition", 1f);
            Scribe_Values.Look(ref weightGainMultiplier, "weightGainMultiplier", 1f);
            Scribe_Values.Look(ref weightLossMultiplier, "weightLossMultiplier", 1f);
            Scribe_Values.Look(ref usePounds, "usePounds", false);
            Scribe_Values.Look(ref weightOpinionMoodlets, "weightOpinionMoodlets", true);
        }

        public void DoWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            kgPerNutrition = listing.SliderLabeled("Physique_Settings_KgPerNutrition".Translate(kgPerNutrition.ToString("0.00")), kgPerNutrition, 0.05f, 2f, tooltip: "Physique_Settings_KgPerNutrition_Tip".Translate());
            weightGainMultiplier = listing.SliderLabeled("Physique_Settings_GainMultiplier".Translate(weightGainMultiplier.ToStringPercent()), weightGainMultiplier, 0f, 5f);
            weightLossMultiplier = listing.SliderLabeled("Physique_Settings_LossMultiplier".Translate(weightLossMultiplier.ToStringPercent()), weightLossMultiplier, 0f, 5f);
            listing.Gap();
            listing.CheckboxLabeled("Physique_Settings_UsePounds".Translate(), ref usePounds);
            listing.CheckboxLabeled("Physique_Settings_OpinionMoodlets".Translate(), ref weightOpinionMoodlets);

            listing.End();
        }
    }
}
