# Physique — roadmap

**Physique** models a pawn's body as a few connected systems instead of one "weight" number:

- **Growth**: height and frame, set during childhood.
- **Fat**: the energy store. It's gained by eating past a full food bar and lost by going hungry.
- **Muscle**: strength, built by physical work and lost through inactivity, starvation and age.

Total weight is *derived* from these systems (frame + muscle + fat), not stored on its own, and weight stages follow BMI from each pawn's own height.

Guiding principles:

1. **Realistic first, tunable second.** Effects come from real physiology. Pacing lives in settings.
2. **True to scale.** Bodies look as big as they are, up to the huge top stages, while staying compatible with vanilla mechanics, apparel and genes wherever the art allows.
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

## Phase 2 — Growth ✅

- [x] Each pawn rolls an adult height from a sex-based normal distribution (men 175 ± 7 cm, women 162 ± 6.5 cm, clamped to ±3 SD), stored in a hidden `Physique_Height` hediff. Vanilla `BodySize` is untouched; other races' real height scales with the cube root of their base body size.
- [x] Children grow along a human growth curve stretched to the race's adult age. Malnutrition while growing permanently stunts adult height (0.1 cm per day at full severity, up to 12 cm).
- [x] Frame and baseline muscle scale with height², so BMI is fair across heights. Starting weights and the weight cap are BMI, so every height starts in the same classes and can reach Gigantic II.
- [x] The weight tooltip shows current height (and a child's adult height), stunting, BMI, body fat % and muscle.
- Deferred: draw scale from height moves to Phase 4. Vanilla has no height genes; revisit for modded genes and HAR races in Phase 5.

## Phase 3 — Muscle

The muscle hediff and starvation wasting already exist from Phase 1. Phase 3 makes muscle a system of its own:

- [ ] Seed starting muscle from sex, backstory and skills, instead of 26 ± 3 kg for everyone. Give women higher essential fat (about 12% versus 3%).
- [ ] **Gain:** strength work (mining, construction, hauling, melee, plant cutting) adds a training stimulus. Muscle grows with stimulus plus adequate food, with diminishing returns toward a genetic ceiling.
- [ ] **Loss:** inactivity (bed rest, downed) and age-related decline (sarcopenia), on top of starvation wasting.
- [ ] **Effects:** melee damage, carrying capacity, mining and construction speed, and a slight Moving bonus. Muscle burns more energy at rest, so hunger rises too. Muscle also partly offsets the Moving penalty from fat.
- [ ] Make muscle visible on the health tab once it has effects of its own.

## Phase 4 — Appearance: full-size bodies

The goal is RimRound's scale, not a few steps between vanilla's Thin and Fat. Every weight stage gets its own body, drawn at the size that stage really is: a Gigantic II pawn (about 900 kg) is several times wider than any vanilla body and spills well past its tile.

- [ ] **One body per weight stage** (Emaciated through Gigantic II), each a `BodyTypeDef` with its own naked texture, `bodyGraphicScale` and head offset, chosen from the pawn's weight stage. Muscle (Phase 3) can pick muscular variants of the lean and average stages.
- [ ] **Sizes from the body model.** Derive each stage's drawn width from its weight and height (body volume spread over the pawn's height) in `BodyMath`, so sprites grow consistently and the scale is tested rather than eyeballed. Height adds a modest vertical scale.
- [ ] **Art in the "vanilla-plus" style**: white fill for skin tinting, soft radial shading, heavy black outline, minimal pec/ab/navel lines. Sprites are generated from a scripted vector (SVG) pipeline per stage × facing × sex, so all three facings stay consistent and shapes are easy to tweak.
- [ ] **Apparel**: vanilla apparel only exists for vanilla body shapes.
  - Up to about Obese, reuse the vanilla Fat body's apparel, scaled to fit.
  - Above that, draw a stage-specific clothing overlay tinted with the worn apparel's colour, as RimRound's clothing sets did, instead of stretching vanilla art past breaking point.
  - Exactly how far to go here is a decision for the owner.
- [ ] **Rendering huge bodies:** large meshes, portraits (scale to fit), beds and downed pawns, and keeping the head on top of the body.

> The uploaded `2662457442` folder (*Erin's Body Retexture*) has no license, so it's a style reference only: we match the style but don't ship, trace or edit those files. Shipping them would need Erin's written permission.

## Phase 5 — Polish and compatibility

- [ ] Balance pass from playtesting: passive gain from meal overshoot, fasting loss rate, muscle growth pacing.
- [ ] Compatibility: Humanoid Alien Races (per-race body data), Biotech genes, Vanilla Expanded body textures.
- [ ] Messages on meaningful changes (for example "X has become obese") with hysteresis so they don't spam.
- [ ] Localization keys for all player-facing text.
