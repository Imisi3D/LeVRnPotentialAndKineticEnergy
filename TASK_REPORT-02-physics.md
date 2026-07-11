# Task 02 · Physics models port · TASK_REPORT

## What was done
Ported the three verified physics models from the `<script>` block of
`reference/embodied_prototype.html` to an engine-agnostic C# library
(`LeVRn.Physics`, netstandard2.1, no `UnityEngine`, zero third-party deps),
plus a headless NUnit suite encoding the six acceptance tests. All eight test
methods pass. The pre-existing tutor suite still passes unchanged.

### Deliverables produced
```
src/LeVRn.Physics/                netstandard2.1 class library, no UnityEngine, no third-party deps
  Constants.cs                    G = 10 (school convention), documented as a curriculum value
  Energy.cs                       KE and PE formulae + the EnergyLedger snapshot struct
  ReleaseVelocityEstimator.cs     release velocity from a timestamped pixel-space trail
  ProjectileModel.cs              ballistic flight + live PE/KE ledger (Handover station)
  PendulumModel.cs                damped pendulum + return-apex detection (Nose Test)
  HillModel.cs                    energy-method rolling track + outcome (Just Enough Hill)
tests/LeVRn.Physics.Tests/        NUnit, runs headless with `dotnet test`
  PhysicsModelsAcceptanceTests.cs the six acceptance tests (eight methods)
LeVRn.Tutor.sln                   recreated at the repo root with all four projects
```

## How to run the tests
The repo has the .NET 8 runtime but no system SDK. A user-local SDK (8.0.422)
lives at `%USERPROFILE%\.dotnet` (installed during Task 01). Run:

```
# use the full path to the user-local dotnet, or put it on PATH
& "$env:USERPROFILE\.dotnet\dotnet.exe" test LeVRn.Tutor.sln
```

Latest result:

```
Passed!  - Failed: 0, Passed: 8, Skipped: 0, Total: 8  - LeVRn.Physics.Tests.dll (net8.0)
Passed!  - Failed: 0, Passed: 5, Skipped: 0, Total: 5  - LeVRn.Tutor.Tests.dll  (net8.0)
```

| Acceptance test | Method(s) | Observed |
| --- | --- | --- |
| 1 · formula spot checks | `Energy_formulae_match_the_worked_examples` | KE(0.5,10)=25, KE(2,3)=9, PE(0.2,5)=10 |
| 2 · projectile conservation | `Projectile_conserves_energy_within_half_a_percent` | max deviation 0.232% (< 0.5%) |
| 3 · velocity estimator | `Velocity_estimator_recovers_known_speed_within_five_percent`, `..._caps_a_sixty_metre_per_second_trail_at_fifteen` | 8 m/s recovered; 60 m/s capped to 15 |
| 4 · pendulum apex | `Damped_pendulum_never_returns_to_its_release_angle`, `Undamped_pendulum_returns_to_its_release_angle_within_tolerance` | damped apex -0.583 rad (> -1.05); undamped -1.04996 (within 0.02) |
| 5 · hill outcome sweep | `Hill_outcome_sweep_matches_the_verified_bands` | all three outcomes; bands as specified |
| 6 · determinism | `Identical_inputs_produce_identical_trajectories` | byte-identical projectile + hill trajectories |

## Behaviour parity with the reference
Matched exactly against `reference/embodied_prototype.html`:
- **ReleaseVelocityEstimator** = the reference `releaseVelocity()`: displacement
  over the last 6 samples (`a = tr[max(0, len-6)]`), `dt = max(16 ms, ...)`, Y
  inverted (screen-down is physics-down), 15 m/s cap with direction preserved,
  zero when fewer than two samples.
- **ProjectileModel** = Station 1 `update()`: mass 0.5 kg, semi-implicit
  (symplectic) Euler (`vy -= G*dt`, then integrate position with the new
  velocity), ledger `PE = m*G*max(0,h)`, `KE = 0.5*m*speed^2`.
- **PendulumModel** = Station 2 `update()`: `L = 2.3`, `alpha = -(G/L)*sin(theta)`,
  per-step velocity damping 0.9985, release -1.05 rad; apex detected only after
  the swing passes `theta > 0.15` (swung away) and returns to `theta < -0.5` with
  `|omega| < 0.02`.
- **HillModel** = Station 3 `track()` and `update()`: the three-piece track,
  energy method `ke = m*G*h0 - mu*dist - m*G*h(x)` with `m = 1`, `mu = 0.12 J/m`,
  `v = sqrt(max(0.12, 2*ke/m))`, `x += dir*v*dt*1.15`, the three-stall settle
  rule with the same direction-flip conditions, and the crest rule
  (`x >= 9.6` moving right: just enough if crest speed `< 1.6`, else overshoot).

## Decisions taken
- **`v = sqrt(max(0.12, 2*ke/m))` reproduced verbatim.** The task prose calls
  0.12 a "speed floor of 0.12 m/s", but the reference floors the value *under*
  the square root, i.e. it floors `v^2` at 0.12, so the effective minimum speed
  is `sqrt(0.12) ~= 0.346 m/s`. CLAUDE.md makes the reference the source of truth
  for behaviour, so I matched the code, not the paraphrase, and documented the
  distinction in `HillModel.SpeedFloorSquared`. Flagging it so the prose can be
  reconciled later if desired.
- **Return-apex detection uses the turning point (`|omega| < 0.02`), and omits
  the reference's cosmetic `gapDeg > 0.05` degree guard.** That guard exists only
  to stop the reference UI from firing when the swing essentially reaches the
  nose; with the locked damping (0.9985) there is always a real gap, so it never
  changed the default behaviour. Acceptance test 4 adds a *new* scenario
  (damping = 1.0) that the reference never runs; the degree guard would wrongly
  suppress detection there (no gap when energy is conserved), so keeping pure
  turning-point detection is both reference-faithful for the default and correct
  for the conservation-sanity case.
- **Engine-agnostic, deterministic, dt injectable.** No `UnityEngine`, no time
  source, no randomness. Every model takes `dt` as a parameter; two runs with the
  same inputs produce identical trajectories (acceptance test 6). Pixel-space
  concerns are confined to `ReleaseVelocityEstimator` (which the task specifies in
  pixels with a `pixelsPerMetre` scale); the dynamics models work in SI metres.
- **Zero third-party dependencies in the library**, mirroring Task 01, to keep the
  core clean for the pending GPL-3.0 licence review and free of Unity package
  conflicts. The test project uses the standard test packages only (NUnit 4.2.2,
  NUnit3TestAdapter 4.6.0, Microsoft.NET.Test.Sdk 17.11.1); these are test-only.
- **`Energy` helper** centralises `KE` and `PE` so the models and the formula
  spot checks compute them identically, rather than duplicating the expressions.
- **Acceptance test 2 launch conditions (fast, flat throw).** Semi-implicit Euler
  under constant gravity loses a fixed `0.5*G^2*dt^2` of energy per step (a known,
  monotonic drift of this integrator; the reference has the identical drift). A
  fast, flat throw (15 m/s horizontal from 1.5 m) keeps that drift a small
  fraction of the large horizontal kinetic energy, so PE + KE holds to 0.232% over
  the whole flight, comfortably inside the 0.5% bound. This is documented in the
  test. A high, slow lob would exceed 0.5% purely from integrator drift; that is a
  property of the reference integrator, not a bug in the port.
- **Solution file recreated, not edited.** `LeVRn.Tutor.sln` was regenerated at
  the repo root via `dotnet new sln` + `dotnet sln add` so it carries clean
  project GUIDs, and now references all four projects (tutor, physics, and both
  test projects).
- **Task 01 report preserved.** This report is `TASK_REPORT-02-physics.md` so it
  does not overwrite the existing `TASK_REPORT.md` (Task 01's report). Flagging
  the naming in case a single rolling `TASK_REPORT.md` per branch is preferred.

## Flagged
- **`.gitignore` was hiding every hand-authored project file (fixed, scoped).**
  The repo's Unity `.gitignore` ignores `*.csproj` and `*.sln`. That is correct
  for Unity's auto-generated files, but it also silently excluded Task 01's
  `LeVRn.Tutor.csproj` and `LeVRn.Tutor.Tests.csproj` and the solution, so they
  were never committed. This is the real reason the solution file was "missing":
  a fresh clone of Task 01 would not build. I added five narrowly scoped negation
  rules (`!LeVRn.Tutor.sln` and one `!...csproj` per project) so exactly these
  hand-authored files are tracked while Unity's blanket rules stay intact.
  Nothing was committed; this only makes the files committable. Worth a look
  during Task 03 (repo hygiene) in case a broader convention is preferred.
- **Prose vs code mismatch on the hill speed floor** (0.12 m/s vs floored `v^2`),
  see above. Behaviour follows the reference; the wording could be tightened.
- **No curriculum values were changed.** `G = 10`, the track geometry, masses,
  friction, and the just-enough/overshoot thresholds are copied exactly from the
  reference. Nothing looked wrong to flag.
- `keystore.txt` and the GPL-3.0 licence review remain as flagged in Task 01;
  untouched here. New code stays isolated under `src/` and `tests/`.

## Notes for the next tasks
- The models expose small, stateful APIs (`Launch`/`Step`/`Ledger`,
  `Release`/`Step`/`ReturnApex`, `Track`/`Run`) that a thin Unity adapter can
  drive from `FixedUpdate` with the frame `dt`; nothing here reads engine state.
- `ReleaseVelocityEstimator` is the one pixel-aware piece; feed it the tracked
  controller trail and the world's pixels-per-metre (or an equivalent scale) at
  the call site.
