# CLAUDE.md · LeVRn Potential and Kinetic Energy Module Upgrade

## What this project is
An educational VR module teaching Potential and Kinetic Energy to JSS 2 Basic
Science students (Nigerian curriculum, second term, Work Energy and Power
strand). The existing build in this repo targets the discontinued Oculus Go
(Unity 2019.3.3f1, legacy Oculus Integration, Lightweight Render Pipeline).
The upgrade migrates it to Meta Quest 2 and 3 and rebuilds the lesson around
embodied, hands-on physics with an adaptive authored tutor.

Authoritative documents (ask the maintainer if not present):
Coding Brief v0.6, Story World Bible, Platform Options and Effort Breakdown,
WebXR Brief (comparison only, not the build target).

## Locked decisions · do not revisit
- Native Quest app. Target devices: Quest 2 and Quest 3. Quest 2 is the
  performance floor.
- Offline-first. No network calls at run time, ever. No telemetry, no
  analytics, no CDN fetches. Headsets are provisioned once, pre-loaded.
- The AI tutor is AUTHORED AND GROUNDED: a state machine retrieving from
  authored response banks. There is NO language model, on-device or cloud.
  Do not add one.
- Learner input is controller and menu only. No voice input.
- Content scope: the full PE/KE topic only. Adjacent topics are out of scope.
- A teacher progress view is in scope, delivered as a companion web page
  outside the headset.
- Physics uses the Nigerian school convention g = 10 m/s^2.

## The world and characters (canon, from the Story World Bible)
Setting: Yaba, Lagos Mainland. Dry season, with a featured afternoon downpour
as the energy-conversion centrepiece. Three peer guides, each a teaching lens:
- Niyat: maths lead, tinkerer, market examples (her mother is a trader).
- Morenike: natural world, reader, mango/bird/water examples.
- Ejike: mechanic in training, okada/keke/engine examples (father's workshop).
Adaptive rule: wrong once gets a hint from the same guide; wrong twice
triggers a LENS SWITCH to a different guide's framing; wrong three times gets
a walkthrough and flags the objective for review.

## Reference implementations · source of truth for behaviour
`reference/tutor_prototype.html` and `reference/embodied_prototype.html`
contain verified, working JavaScript for the tutor engine and the physics
models. When porting, match their observable behaviour exactly. The
acceptance tests in each task file encode that behaviour; they were run
against these implementations before this package was written.

## Repository facts (from the audit)
- Project code lives in `Assets/Scripts` (~50 files) and `Assets/Scenes`
  (Field, LoadingScene, Module2Testing, Module2_SimulationRoom,
  Module2_Street). Everything under `Assets/Oculus` and
  `Assets/OVRInputSelection` is vendored SDK, not project code.
- `keystore.txt` is committed at the repo root. Do NOT rewrite git history
  or delete it without explicit maintainer approval; flag it in your report.
- Licence is GPL-3.0 and under separate review. Keep new code in clearly
  separated directories/assemblies so licensing decisions stay easy.

## Engineering conventions
- New runtime code: C#, engine-agnostic where the task says so, meaning no
  `UnityEngine` references in core logic assemblies; keep Unity adapters thin
  and separate. Target netstandard2.1-compatible C#.
- Every task ships with its acceptance tests implemented and passing. Tests
  must run headless (plain `dotnet test` or NUnit console; no Editor).
- No new third-party dependencies without recording the reason and licence
  in the task report.
- No em dashes in any authored learner-facing text. Use commas or periods.
- Determinism: physics and tutor logic must be reproducible in tests; take
  time steps and randomness (if any) as injectable parameters.
- Each task ends with a short TASK_REPORT.md: what was done, decisions taken,
  anything flagged, how to run the tests.

## Task order
Run the tasks in numeric order; 01 and 02 are independent of the Unity
project and should be completed first.
1. `tasks/01-tutor-engine.md` · port the tutor engine to C# (highest value)
2. `tasks/02-physics-models.md` · port the verified physics models
3. `tasks/03-repo-hygiene.md` · cleanup branch (careful scope)
4. `tasks/04-api-migration-map.md` · deprecated API inventory and mapping
5. `tasks/05-progress-data.md` · learner progress model, save and resume
6. `tasks/06-teacher-view.md` · offline teacher companion page

## Hard boundaries
- Do not attempt Unity Editor work, scene edits, prefab wiring, material
  fixes, or APK builds; those are human tasks with hardware.
- Do not add any network call, account system, or remote logging.
- Do not modify or reinterpret curriculum content values (formulae, g = 10,
  worked answers). If content seems wrong, flag it; do not fix silently.
