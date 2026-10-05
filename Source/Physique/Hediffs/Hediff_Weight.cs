using System.Text;
using Physique.Core;
using Physique.Utilities;
using Verse;

namespace Physique.Hediffs
{
    /// <summary>
    /// Body weight. Severity is BMI, derived from the pawn's frame, muscle and fat (see BodyUtility.UpdateWeight);
    /// stages and their effects key off it.
    /// </summary>
    public class Hediff_Weight : Hediff
    {
        public override string SeverityLabel => BodyMath.FormatWeight(pawn.Weight(), PhysiqueMod.Settings.usePounds);

        public override string TipStringExtra
        {
            get
            {
                var tip = new StringBuilder(base.TipStringExtra);
                if (pawn.Composition() is BodyComposition body)
                {
                    if (tip.Length > 0 && tip[tip.Length - 1] != '\n')
                        tip.AppendLine();
                    bool pounds = PhysiqueMod.Settings.usePounds;
                    AppendHeight(tip, body, pounds);
                    tip.AppendLine("Physique_Tip_Bmi".Translate(body.Bmi.ToString("0.0")));
                    tip.AppendLine("Physique_Tip_BodyFat".Translate(body.BodyFatFraction.ToStringPercent(), BodyMath.FormatWeight(BodyMath.ToRealKilos(body.FatKg, pawn.BodySize), pounds)));
                    tip.AppendLine("Physique_Tip_Muscle".Translate(BodyMath.FormatWeight(BodyMath.ToRealKilos(body.MuscleKg, pawn.BodySize), pounds)));
                }
                return tip.ToString().TrimEndNewlines();
            }
        }

        void AppendHeight(StringBuilder tip, BodyComposition body, bool imperial)
        {
            string current = BodyMath.FormatHeight(pawn.CurrentHeightCm(), imperial);
            if (pawn.ageTracker.Adult)
                tip.AppendLine("Physique_Tip_Height".Translate(current));
            else
                tip.AppendLine("Physique_Tip_HeightGrowing".Translate(current, BodyMath.FormatHeight(body.HeightCm * BodyMath.RaceHeightScale(pawn.RaceProps.baseBodySize), imperial)));

            if (pawn.HeightHediff() is Hediff_Height height && height.geneticHeightCm - height.Severity >= 0.5f)
                tip.AppendLine("Physique_Tip_Stunted".Translate((height.geneticHeightCm - height.Severity).ToString("0")));
        }
    }
}
