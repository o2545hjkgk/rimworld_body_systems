# Physique — RimWorld 1.6

Realistic body systems for RimWorld pawns, built from a stripped-down [RimRound](https://github.com/Niwatori401/RimRound) by Niwatori401. See [ROADMAP.md](ROADMAP.md) for planned growth and muscle systems.

Currently implemented:

- **Weight hediff.** Every humanlike pawn with a food need has a body weight, shown in kg (or lbs) on the health tab. The weight stage applies realistic effects to movement, manipulation, breathing, blood pumping, hunger, rest, immunity, temperature comfort and fertility.
- **Weight opinion.** Each pawn gets one of eight opinion traits, from *Hate* to *Fanatical*. Each trait comes with a moodlet that depends on the pawn's current weight.
- **Weight gain and loss.** Food eaten past a full food bar is stored as weight. A pawn that goes hungry while carrying fat burns it instead of starving.

The heaviest stage is currently **Gigantic II**. Weight is capped at 989 kg by `maxSeverity` in `1.6/Defs/HediffDefs/RimRound_Weight.xml`; lower that value to lower the cap.

## How weight is calculated

For an adult human (body size 1):

```
kg = severity × 1000 + 25
```

Other pawns weigh that amount times their body size. Stages and moodlets are therefore judged relative to the pawn's frame: a child or a small race isn't "emaciated" just for weighing less than an adult human.

Height doesn't exist yet, so BMI can't be computed per pawn. The effects below were calibrated against an assumed 1.75 m adult. The BMI column shows what each band means at that height.

## Stages

Capacity changes are multipliers (`postFactor`), so a pawn with bionic legs keeps proportionally more of their speed. Moving also drops slightly from the Breathing and Blood pumping penalties, because vanilla factors those into it.

| Stage | Weight (kg) | BMI @1.75 m | Hunger | Rest fall | Moving | Manip. | Breathing | Blood pump. | Other |
|---|---|---|---|---|---|---|---|---|---|
| Emaciated | 26–30 | < 10 | ×0.80 | ×1.15 | ×0.75 | ×0.85 | | ×0.90 | Consciousness ×0.90, immunity −25%, comfy min +8 °C, fertility ×0.3 |
| Very Thin | 30–40 | 10–13 | ×0.85 | ×1.05 | ×0.90 | ×0.95 | | ×0.95 | Consciousness ×0.95, immunity −15%, comfy min +5 °C, fertility ×0.6 |
| Thin | 40–60 | 13–20 | ×0.95 | | | | | | immunity −5%, comfy min +2 °C, fertility ×0.9 |
| Thick | 60–75 | 20–24 | | | | | | | baseline, no effects |
| Chunky | 75–90 | 24–29 | ×1.05 | | | | | | comfy range −1 °C |
| Chubby | 90–115 | 29–38 | ×1.10 | ×1.03 | ×0.95 | | ×0.97 | | comfy range −2 °C, fertility ×0.95 |
| Corpulent | 115–145 | 38–47 | ×1.20 | ×1.06 | ×0.90 | ×0.98 | ×0.94 | ×0.97 | immunity −3%, comfy range −3 °C, fertility ×0.9 |
| Fat | 145–180 | 47–59 | ×1.35 | ×1.10 | ×0.82 | ×0.95 | ×0.90 | ×0.94 | immunity −5%, comfy range −4 °C, fertility ×0.85 |
| Obese | 180–225 | 59–73 | ×1.50 | ×1.14 | ×0.72 | ×0.92 | ×0.86 | ×0.91 | immunity −8%, comfy range −5 °C, fertility ×0.8 |
| Morbidly Obese | 225–255 | 73–83 | ×1.70 | ×1.18 | ×0.60 | ×0.88 | ×0.82 | ×0.88 | immunity −10%, comfy range −6 °C, fertility ×0.7 |
| Morbidly Obese II | 255–305 | 83–100 | ×1.80 | ×1.22 | ×0.50 | ×0.84 | ×0.78 | ×0.85 | immunity −12%, comfy range −7 °C, fertility ×0.65 |
| Lardy | 305–375 | 100–122 | ×2.10 | ×1.27 | ×0.38 | ×0.78 | ×0.72 | ×0.80 | immunity −15%, comfy range −8 °C, fertility ×0.55 |
| Lardy II | 375–455 | 122–149 | ×2.40 | ×1.32 | ×0.27 | ×0.72 | ×0.66 | ×0.75 | immunity −18%, comfy range −9 °C, fertility ×0.45 |
| Enormous | 455–560 | 149–183 | ×2.80 | ×1.37 | ×0.18 | ×0.65 | ×0.60 | ×0.70 | immunity −20%, comfy range −10 °C, fertility ×0.35 |
| Enormous II | 560–685 | 183–224 | ×3.30 | ×1.42 | ×0.12 | ×0.58 | ×0.55 | ×0.65 | immunity −22%, comfy range −11 °C, fertility ×0.25 |
| Gigantic | 685–825 | 224–269 | ×4.00 | ×1.46 | ×0.06 | ×0.50 | ×0.50 | ×0.60 | immunity −25%, comfy range −12 °C, fertility ×0.2 |
| Gigantic II | 825–989 | 269–323 | ×4.60 | ×1.50 | ×0 | ×0.42 | ×0.45 | ×0.55 | immunity −28%, comfy range −13 °C, fertility ×0.15 |

"Comfy range −N °C" means both ends of the comfortable temperature range drop: more fat insulates against cold and makes heat harder to tolerate.

What drives these numbers:

- **Hunger** follows resting energy expenditure (Mifflin–St Jeor), which rises roughly linearly with mass, with activity falling off at higher weights. Underweight bodies burn less.
- **Moving** declines steadily with obesity. Below vanilla's 15% minimum a pawn can't walk and is downed, which happens from **Enormous II** (560 kg) onward. People above roughly 450–550 kg are usually bed-bound.
- **Manipulation** is mostly reach and range of motion, so it falls more slowly than Moving.
- **Breathing / Blood pumping** cover obesity hypoventilation and cardiac load.
- **Rest fall** covers fatigue and sleep apnea.
- Being **underweight** hurts strength, immunity, cold tolerance and fertility.

## Gaining and losing weight

- **Gain.** Vanilla throws away nutrition that doesn't fit in the food bar, for example eating a 0.9-nutrition meal when only 30% hungry. That surplus becomes weight.
- **Loss.** While a pawn's food bar is below the *Hungry* threshold and they weigh more than `fatReserveFloor` (60 kg adult-equivalent), their body burns fat to cover the hunger. They stay hungry but don't starve, and they lose weight at their normal metabolic rate. At or below the floor, vanilla starvation applies and they keep losing weight while starving.

Settings (Options → Mod settings → RimRound; to be renamed to Physique in roadmap Phase 0):

- **Kilograms per nutrition**: default 1.0, RimRound's original pacing. About 0.2 is physiologically realistic: 1 nutrition is roughly 1,500 kcal and 1 kg of fat roughly 7,700 kcal.
- **Gain / loss multipliers**, **show pounds**, and **weight opinion moodlets** on or off.

Dev mode adds *RimRound* debug actions: add or remove 10 kg, add 100 kg, and cycle a pawn's weight opinion.

## Building

The C# project is `Source/RimRoundCore`. It uses NuGet reference packages, so it builds without a RimWorld install:

```
dotnet build Source/RimRoundCore -c Release
```

The output goes to `1.6/Assemblies/RimRound.dll`. Harmony is required at runtime.

## License

This project is, unless otherwise specified, licensed under the Unlicense. Weight opinion moodlet text by rngsusd#9608.
