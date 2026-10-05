# Physique — roadmap

**Physique** models a pawn's body as a few connected systems instead of one "weight" number:

- **Growth**: height and frame, set during childhood.
- **Fat**: the energy store. It's gained by eating past a full food bar and lost by going hungry.
- **Muscle**: strength, built by physical work and lost through inactivity, starvation and age.

Total weight is *derived* from these systems (lean frame + muscle + fat), not stored on its own. Once growth exists, height is known, so fat effects can use BMI or body-fat % instead of the kg thresholds that assume a 1.75 m adult.

Guiding principles:

1. **Realistic first, tunable second.** Effects come from real physiology. Pacing lives in settings.
2. **Vanilla-compatible.** Work with vanilla body types, apparel and genes before adding custom art.
3. **Small surface.** Each system is one hediff plus one utility class, with no stomach, perks or buildings.

---

## Phase 0 — Clean slate and identity ✅

- [x] Delete the remaining legacy RimRound content: old source, the 1.3–1.5 folders, textures, sounds, and the unused 1.6 defs and patches.
- [x] Rename: namespace and assembly `Physique`, Harmony id `o2545hjkgk.Physique`, defs prefixed `Physique_`, placeholder preview and icon.
- [x] Move the pure formulas into `Core/BodyMath.cs` and add `Source/Physique.Tests`, which also cross-checks the XML defs.
- [x] Branch layout: the rebuilt core goes to `main` by pull request.

## Phase 1 — Body model refactor ✅

- [x] `BodyComposition` (in `Core/BodyMath.cs`): height, frame, muscle, fat, and derived weight, BMI and body-fat %.
- [x] Fat and muscle are stored in hidden hediffs (`Physique_Fat`, `Physique_Muscle`). Frame is a constant until Phase 2. Nothing has been released, so there's no save migration.
- [x] Weight stages re-keyed to BMI, at a reference height of 1.75 m until Phase 2. The overweight calibration is unchanged; the underweight bands now follow the WHO thinness grades instead of unsurvivable kg values.
- [x] The flat 60 kg fat reserve floor is replaced by essential fat. Below it, starvation wastes muscle, which is now the only way to become underweight.
- [ ] Follow-up: move obesity effects onto fat % and wasting effects onto muscle once growth and muscle exist, so they no longer ride on BMI alone.

## Phase 2 — Growth

- [ ] Generate adult height per pawn from a sex- and race-dependent distribution. Respect Biotech body-size genes and HAR races, and leave vanilla `BodySize` untouched.
- [ ] Children grow along a curve tied to life stage and age. Sustained malnutrition in childhood stunts final height.
- [ ] Height drives lean frame mass, and through it expected weight, BMI and a small draw-scale tweak, if render-safe.
- [ ] Show height and BMI on the health tab.

## Phase 3 — Muscle

- [ ] Add a muscle hediff (severity = muscle mass relative to frame), seeded from backstory and skills at generation.
- [ ] **Gain:** strength work (mining, construction, hauling, melee, plant cutting) adds a training stimulus. Muscle grows with stimulus plus adequate food, with diminishing returns toward a genetic ceiling.
- [ ] **Loss:** inactivity (bed rest, downed), starvation (muscle is burned once fat runs out) and age-related decline (sarcopenia).
- [ ] **Effects:** melee damage, carrying capacity, mining and construction speed, and a slight Moving bonus. Muscle burns more energy at rest, so hunger rises too. Muscle also partly offsets the Moving penalty from fat.

## Phase 4 — Appearance

1. [ ] **Vanilla body-type mapping (no new art).** Choose Thin / Male / Female / Fat / Hulk from fat % and muscle. This works with vanilla apparel and with any body retexture the player has installed.
2. [ ] **Original textures in the "vanilla-plus" style** (white fill, soft radial shading, heavy black outline, minimal pec/ab/navel lines, 512 px). Draw intermediate fat and muscle steps between the vanilla types, built with a scripted SVG/vector pipeline so all three facings stay consistent.
3. [ ] **Apparel:** reuse vanilla apparel textures by drawing in-between bodies with the nearest vanilla body type's apparel plus a small scale factor, rather than drawing apparel for every new body.

> The uploaded `2662457442` folder (*Erin's Body Retexture*) has no license, so it's reference only: we match the style but don't ship, trace or edit those files. Shipping them would need Erin's written permission.

## Phase 5 — Polish and compatibility

- [ ] Balance pass from playtesting: passive gain from meal overshoot, fasting loss rate, muscle growth pacing.
- [ ] Compatibility: Humanoid Alien Races (per-race body data), Biotech genes, Vanilla Expanded body textures.
- [ ] Messages on meaningful changes (for example "X has become obese") with hysteresis so they don't spam.
- [ ] Localization keys for all player-facing text.
