# Physique — RimWorld 1.6

Realistic body systems for RimWorld pawns, built from a stripped-down [RimRound](https://github.com/Niwatori401/RimRound) by Niwatori401. See [ROADMAP.md](ROADMAP.md) for what's next (muscle training, then art).

Currently implemented:

- **Body composition.** Every humanlike pawn with a food need has a height, a frame, muscle and fat. Frame, muscle and fat add up to the pawn's weight, shown in kg (or lbs) on the health tab; hover it for height, BMI, body fat % and muscle.
- **Growth.** Each pawn rolls an adult height (men 175 ± 7 cm, women 162 ± 6.5 cm). Children grow toward it along a human growth curve, and malnutrition while growing up permanently stunts it. The weight stage applies realistic effects to movement, manipulation, breathing, blood pumping, hunger, rest, immunity, temperature comfort and fertility.
- **Weight opinion.** Each pawn gets one of eight opinion traits, from *Hate* to *Fanatical*. Each trait comes with a moodlet that depends on the pawn's current weight.
- **Weight gain and loss.** Food eaten past a full food bar is stored as fat. A hungry pawn burns fat instead of starving; once only essential fat is left, starvation wastes muscle.

The heaviest stage is currently **Gigantic II**. Weight is capped at BMI 322 (`maxBmi` in `1.6/Defs/HediffDefs/Physique_Weight.xml`), just under 990 kg at 1.75 m. Lower that value to lower the cap; being a BMI, it applies equally at every height.

## How weight is calculated

A body is four parts. Masses are stored in adult-equivalent kg (as if the pawn were a grown adult of body size 1):

| Part | Stored in | Default | Changes when |
|---|---|---|---|
| Height | hidden `Physique_Height` hediff (adult height, cm) | rolled by sex, normal distribution clamped to ±3 SD | childhood malnutrition stunts it (up to 12 cm) |
| Frame (bone, organs, blood, skin) | derived from height | 30 kg at 1.75 m, × (height / 1.75 m)² | follows height |
| Muscle | hidden `Physique_Muscle` hediff | 26 kg ± 3 at 1.75 m, × (height / 1.75 m)² | starvation wastes it (5 kg minimum); Phase 3 adds training |
| Fat | hidden `Physique_Fat` hediff | from the starting BMI | surplus food adds it, hunger burns it |

```
weight = frame + muscle + fat          real kg = adult-equivalent kg × body size
BMI    = weight / adult height²        real height = adult height × growth for age × race size
```

The visible **weight** hediff's severity is that BMI, and its stages key off it. Lean mass scales with height squared, so a tall and a short pawn built the same way land in the same stage. Children are judged as the adults they'll grow into, so a child isn't "emaciated" just for being small.

Children follow a human growth curve (29% of adult height at birth, 55% at 3, 79% at 10, full height at 18), stretched to the race's own adult age. While a pawn is still growing, every day of malnutrition at full severity takes 0.1 cm off their adult height, up to 12 cm. Stunting shows in the weight tooltip.

Starting weights are rolled as BMI, so tall and short pawns start in the same weight classes.

## Stages

Underweight bands follow the WHO thinness grades (mild < 18.5, severe < 16); below about BMI 13 starvation is life-threatening. Overweight starts at 25 and obesity at 30, as in the WHO scale. Capacity changes are multipliers (`postFactor`), so a pawn with bionic legs keeps proportionally more of their speed. Moving also drops slightly from the Breathing and Blood pumping penalties, because vanilla factors those into it.

| Stage | BMI | Weight at 1.75 m (kg) | Hunger | Rest fall | Moving | Manip. | Breathing | Blood pump. | Other |
|---|---|---|---|---|---|---|---|---|---|
| Emaciated | < 13 | < 40 | ×0.80 | ×1.15 | ×0.75 | ×0.85 |  | ×0.90 | Consciousness ×0.90, immunity −25%, comfy min +8 °C, fertility ×0.3 |
| Very Thin | 13–16 | 40–49 | ×0.85 | ×1.05 | ×0.90 | ×0.95 |  | ×0.95 | Consciousness ×0.95, immunity −15%, comfy min +5 °C, fertility ×0.6 |
| Thin | 16–18.5 | 49–57 | ×0.95 |  |  |  |  |  | immunity −5%, comfy min +2 °C, fertility ×0.9 |
| Thick | 18.5–25 | 57–77 |  |  |  |  |  |  | baseline, no effects |
| Chunky | 25–30 | 77–92 | ×1.05 |  |  |  |  |  | comfy range −1 °C |
| Chubby | 30–37.5 | 92–115 | ×1.10 | ×1.03 | ×0.95 |  | ×0.97 |  | comfy range −2 °C, fertility ×0.95 |
| Corpulent | 37.5–47.5 | 115–145 | ×1.20 | ×1.06 | ×0.90 | ×0.98 | ×0.94 | ×0.97 | immunity −3%, comfy range −3 °C, fertility ×0.9 |
| Fat | 47.5–59 | 145–181 | ×1.35 | ×1.10 | ×0.82 | ×0.95 | ×0.90 | ×0.94 | immunity −5%, comfy range −4 °C, fertility ×0.85 |
| Obese | 59–73.5 | 181–225 | ×1.50 | ×1.14 | ×0.72 | ×0.92 | ×0.86 | ×0.91 | immunity −8%, comfy range −5 °C, fertility ×0.8 |
| Morbidly Obese | 73.5–83.5 | 225–256 | ×1.70 | ×1.18 | ×0.60 | ×0.88 | ×0.82 | ×0.88 | immunity −10%, comfy range −6 °C, fertility ×0.7 |
| Morbidly Obese II | 83.5–100 | 256–306 | ×1.80 | ×1.22 | ×0.50 | ×0.84 | ×0.78 | ×0.85 | immunity −12%, comfy range −7 °C, fertility ×0.65 |
| Lardy | 100–122.5 | 306–375 | ×2.10 | ×1.27 | ×0.38 | ×0.78 | ×0.72 | ×0.80 | immunity −15%, comfy range −8 °C, fertility ×0.55 |
| Lardy II | 122.5–148.5 | 375–455 | ×2.40 | ×1.32 | ×0.27 | ×0.72 | ×0.66 | ×0.75 | immunity −18%, comfy range −9 °C, fertility ×0.45 |
| Enormous | 148.5–183 | 455–560 | ×2.80 | ×1.37 | ×0.18 | ×0.65 | ×0.60 | ×0.70 | immunity −20%, comfy range −10 °C, fertility ×0.35 |
| Enormous II | 183–223.5 | 560–684 | ×3.30 | ×1.42 | ×0.12 | ×0.58 | ×0.55 | ×0.65 | immunity −22%, comfy range −11 °C, fertility ×0.25 |
| Gigantic | 223.5–269.5 | 684–825 | ×4.00 | ×1.46 | ×0.06 | ×0.50 | ×0.50 | ×0.60 | immunity −25%, comfy range −12 °C, fertility ×0.2 |
| Gigantic II | 269.5+ | 825+ | ×4.60 | ×1.50 | ×0 | ×0.42 | ×0.45 | ×0.55 | immunity −28%, comfy range −13 °C, fertility ×0.15 |

"Comfy range −N °C" means both ends of the comfortable temperature range drop: more fat insulates against cold and makes heat harder to tolerate.

What drives these numbers:

- **Hunger** follows resting energy expenditure (Mifflin–St Jeor), which rises roughly linearly with mass, with activity falling off at higher weights. Underweight bodies burn less.
- **Moving** declines steadily with obesity. Below vanilla's 15% minimum a pawn can't walk and is downed, which happens from **Enormous II** (BMI 183, 560 kg at 1.75 m) onward. People above roughly 450–550 kg are usually bed-bound.
- **Manipulation** is mostly reach and range of motion, so it falls more slowly than Moving.
- **Breathing / Blood pumping** cover obesity hypoventilation and cardiac load.
- **Rest fall** covers fatigue and sleep apnea.
- Being **underweight** hurts strength, immunity, cold tolerance and fertility.

## Gaining and losing weight

- **Gain.** Vanilla throws away nutrition that doesn't fit in the food bar, for example eating a 0.9-nutrition meal when only 30% hungry. That surplus becomes weight.
- **Loss.** While a pawn's food bar is below the *Hungry* threshold and they have more than essential fat (`essentialFatKg`, 3 kg), their body burns fat to cover the hunger. They stay hungry but don't starve, and they lose fat at their normal metabolic rate. Burning down to essential fat leaves a lean but healthy body (BMI about 19).
- **Wasting.** With only essential fat left, vanilla starvation applies, and while starving the pawn also loses muscle at the same rate. That is how a pawn becomes Thin, Very Thin and finally Emaciated.

Settings (Options → Mod settings → Physique):

- **Kilograms per nutrition**: default 1.0, RimRound's original pacing. About 0.2 is physiologically realistic: 1 nutrition is roughly 1,500 kcal and 1 kg of fat roughly 7,700 kcal.
- **Gain / loss multipliers**, **show pounds**, and **weight opinion moodlets** on or off.

Dev mode adds *Physique* debug actions: add or remove 10 kg fat, add 100 kg fat, add or remove 5 kg muscle, and cycle a pawn's weight opinion.

## Building

The C# project is `Source/Physique`. It uses NuGet reference packages, so it builds without a RimWorld install:

```
dotnet build Source/Physique -c Release
```

The output goes to `1.6/Assemblies/Physique.dll`. Harmony is required at runtime.

Formulas that don't need the game live in `Source/Physique/Core/BodyMath.cs`. `Source/Physique.Tests` covers them, along with consistency checks on the XML defs (stage order, the BMI cap at every height, height limits, fasting and wasting landing in the right stages, one mood stage per weight band):

```
dotnet test Source/Physique.Tests
```

## License

This project is, unless otherwise specified, licensed under the Unlicense. Weight opinion moodlet text by rngsusd#9608.
