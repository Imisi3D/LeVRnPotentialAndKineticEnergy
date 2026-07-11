# Task 01 · Tutor engine port · TASK_REPORT

## What was done
Ported the verified tutor engine from `reference/tutor_prototype.html` (the
`<script>` block) to an engine-agnostic C# library with all authored text moved
out into a data file, plus a headless NUnit test suite that encodes the five
acceptance tests. All five pass.

### Deliverables produced
```
src/LeVRn.Tutor/                netstandard2.1 class library, no UnityEngine, zero third-party deps
  Guides.cs                     Guide + GuideSet (Niyat, Morenike, Ejike identities and lenses)
  TutorScript.cs                node types (Say, Ask, Scene, End), Objective, TutorScript container
  ResponseBank.cs               per-question authored content (affirm/hint/switchTo/switchText/walkthrough)
  Mastery.cs                    per-objective asked/scored tracking, percent, review flag, overall
  Events.cs                     TutorEvent stream the UI layer subscribes to
  TutorEngine.cs                the state machine: step, answer, adapt
  Json.cs                       minimal in-house JSON reader (keeps the library dependency-free)
  TutorScriptLoader.cs          builds a TutorScript from content/pe-ke.json
tests/LeVRn.Tutor.Tests/        NUnit, runs headless with `dotnet test`
  LearnerSimulator.cs           deterministic driver: answers by a per-objective profile
  TutorEngineAcceptanceTests.cs the five acceptance tests
content/pe-ke.json              full authored script, externalised verbatim
LeVRn.Tutor.sln                 solution tying the two projects together
```

## How to run the tests
The repo had the .NET 8 runtime but no SDK. A user-local SDK (8.0.422) was
installed to `%USERPROFILE%\.dotnet` via Microsoft's official install script
(no admin, removable, does not touch the system). To run:

```
# with that SDK on PATH (or use the full path to dotnet.exe)
dotnet test LeVRn.Tutor.sln
```

Latest result: **Passed! Failed: 0, Passed: 5, Skipped: 0, Total: 5.**

| Acceptance test | Method |
| --- | --- |
| 1 · strong learner | `Strong_learner_masters_everything_with_no_adaptation` |
| 2 · weak learner | `Weak_learner_triggers_adaptation_switches_and_review_flag` |
| 3 · adaptation idempotence | `Extras_are_inserted_at_most_once` |
| 4 · content integrity | `Content_yields_the_same_number_of_authored_responses_as_the_reference` |
| 5 · determinism | `Same_profile_produces_identical_event_sequences` |

## Behaviour parity with the reference
Matched exactly:
- Node flow: Scene nodes auto-advance; Say advances on Continue; Ask presents options.
- Wrong-answer handling per question: attempt 1 -> hint (same guide) and re-ask;
  attempt 2 -> lens switch to the guide's `switchTo`, re-ask with `switchText`;
  attempt 3 -> walkthrough (spoken by the switch guide), objective counted as
  asked with score 0 (flags review), then advance.
- Scoring by the attempt the correct answer landed on: 1.0, 0.6, 0.3, else 0.0.
- Affirmation speaker: the switch guide if a switch happened, otherwise the
  original guide (mirrors the reference `setSpeaker` ternary).
- Difficulty adaptation: if any `define` question needed more than one attempt
  (`defineStruggle`), the two extra classification items are spliced in
  immediately before Niyat's formula introduction, exactly once per session.
- Mastery percent = round(scored/asked*100); review = asked > 0 and percent < 50;
  overall = mean of per-objective ratios. These reproduce the reference values:
  strong learner asked `[2,2,2,3,1]` at 100% each with 0 switches; weak learner
  mastery `[30,60,0,60,100]`, classify asked 4, 4 lens switches, formula flagged.
- Authored-response count = 54 (6 Say +1 each, 12 Ask +4 each), matching the
  reference `countBank()` badge.

## Decisions taken
- **No language model, no network, offline.** The engine is a pure state machine
  over authored banks, as locked. Nothing in the library performs I/O except the
  optional `TutorScriptLoader`, which only reads a local file.
- **Zero third-party dependencies.** Rather than pull in `System.Text.Json`
  (which would be a NuGet dependency on netstandard2.1 and a known friction point
  for later Unity integration), I wrote a small in-house JSON reader (`Json.cs`).
  This keeps the reusable core clean for the pending GPL-3.0 licence review and
  removes any Unity package-conflict risk. The test project does use the standard
  test packages (NUnit 4.2.2, NUnit3TestAdapter 4.6.0, Microsoft.NET.Test.Sdk
  17.11.1); these are test-only and do not ship in the engine assembly.
- **Engine-agnostic, event-driven.** The engine exposes `Start()`, `Continue()`
  and `Answer(int)` and reports everything through `TutorEvent`s. A Unity layer, a
  web layer and the test harness all drive it identically. No `UnityEngine`
  reference exists in the library.
- **Determinism.** No time or randomness in the engine; the driver is a scripted
  profile. Two runs with the same profile emit byte-identical event signatures.
- **All learner-facing text externalised** to `content/pe-ke.json`, copied
  verbatim from the reference (punctuation preserved; the `½ × ²` glyphs kept;
  no em dashes present or introduced). The engine holds no lesson text. The only
  string literals in engine logic are the structural identifiers `"define"` and
  `"niyat"`, which encode the adaptation rule itself (struggle objective and
  insertion anchor), not learner-facing content; they are named constants with
  comments.
- **`CurrentQuestion` exposes the correct option index.** A production UI ignores
  it; it exists so a headless harness can simulate a learner profile
  deterministically. Documented in code.

## Flagged (not fixed, per the hard boundaries)
- **`keystore.txt` is still committed at the repo root.** Untouched, as required;
  flagging it here for the repo-hygiene task (03).
- **Licence is GPL-3.0 and under review.** New code is isolated under `src/` and
  `tests/` with its own solution and assembly, keeping licensing decisions easy.
- No curriculum values were changed. The formulae, `g = 10`, and all worked
  answers are copied exactly from the reference. Nothing looked wrong to flag.

## Notes for the next tasks
- The `TutorScript`/`TutorEngine`/`TutorEvent` surface is intended to be the
  reusable adaptive core for future modules. A Unity adapter should subscribe to
  `EventEmitted` and translate events to scene/dialogue updates; it never needs to
  read engine internals.
- If a Unity asset pipeline is preferred over a JSON file later, implement an
  alternative loader that returns a `TutorScript`; the engine is unaffected.
