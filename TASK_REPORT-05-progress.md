# Task 05 · Learner progress model, save and resume · TASK_REPORT

## What was done
Built `LeVRn.Progress`, the on-device progress layer the module and the teacher
view consume: an anonymous, schema-versioned learner record, JSON persistence
behind an `IStorage` abstraction with a corruption-safe loader, and a
teacher-view export bundle with a validator that defines the schema Task 06 will
read. All four acceptance tests pass; the existing tutor and physics suites still
pass.

### Deliverables produced
```
src/LeVRn.Progress/               netstandard2.1, no UnityEngine, zero third-party deps
  Json.cs                         tiny dependency-free JSON reader/writer (object graph)
  LearnerProgress.cs              session + per-objective mastery + station log + JSON mapping
  ProgressStore.cs                IStorage, FileStorage, ProgressStore (save + corruption-safe load)
  Export.cs                       teacher-view export bundle + schema validator (Task 06 contract)
tests/LeVRn.Progress.Tests/       NUnit, headless
  TutorProgressAdapter.cs         maps Task 01 engine events to progress; deterministic driver
  ProgressAcceptanceTests.cs      the four acceptance tests
LeVRn.Tutor.sln                   both new projects added (solution now has 6 projects)
```

## How to run the tests
```
& "$env:USERPROFILE\.dotnet\dotnet.exe" test LeVRn.Tutor.sln
```
Latest result:
```
Passed!  - Failed: 0, Passed: 8, Skipped: 0, Total: 8  - LeVRn.Physics.Tests.dll  (net8.0)
Passed!  - Failed: 0, Passed: 5, Skipped: 0, Total: 5  - LeVRn.Tutor.Tests.dll     (net8.0)
Passed!  - Failed: 0, Passed: 4, Skipped: 0, Total: 4  - LeVRn.Progress.Tests.dll  (net8.0)
```

| Acceptance test | Method | Observed |
| --- | --- | --- |
| 1 · round-trip resume | `Round_trip_resume_matches_an_uninterrupted_run` | saved at k=6 of n=12 resolved questions; resumed run identical (overall 78%, 2 lens switches, per-objective asked/scored/attempts/flag all equal) |
| 2 · corrupt file | `Corrupt_file_loads_as_a_fresh_session_and_is_logged` | 5 kinds of junk (bad JSON, binary, wrong-root array, bare number, empty) all load as a fresh session, logged, no crash |
| 3 · no personal data | `Serialized_json_contains_no_personal_data` | serialized JSON contains none of name/age/dob/email/phone/gender/address; only `learnerSlotId` |
| 4 · export validates | `Export_bundle_validates_against_the_task06_schema` | bundle built from real physics station events validates; a tampered bundle with a `name` field is rejected |

## Design decisions
- **`LeVRn.Progress` has no dependency on `LeVRn.Tutor` or `LeVRn.Physics`.** It is
  pure data plus persistence. The event-to-progress mapping (`TutorProgressAdapter`)
  lives in the test project, and station values are produced by the physics models
  at the call site. This keeps the progress library reusable and keeps licensing
  and Unity-integration decisions simple, mirroring the Task 01/02 split.
- **No personal data, by construction.** The record holds only a device-local
  anonymous `LearnerSlotId` (a headset seat, not a person), timestamps, and
  learning telemetry. There is no field for name, age, or anything personal, and
  the export validator actively rejects a bundle that carries such a key
  (acceptance test 3 and the negative control in test 4).
- **Save after every scored event.** `TutorProgressAdapter` calls
  `ProgressStore.Save` on every `AffirmGiven`/`WalkthroughGiven` (the moments the
  engine records mastery) and at session end. `FileStorage` writes to a temp file
  then moves it into place, so a crash mid-write cannot corrupt the file.
- **Resume by deterministic replay.** The Task 01 engine has no serialized cursor,
  but it is deterministic, so a resumed session is reconstructed by replaying the
  same answer profile on a fresh engine. The adapter is told how many scored
  events were already persisted and skips re-applying that head, then records the
  tail. The resume cursor is simply the sum of per-objective `Asked`
  (`ResolvedQuestions`), so nothing extra needs storing. The test proves the final
  mastery equals an uninterrupted run.
- **Progress mastery mirrors the engine exactly.** Per-objective `Asked`/`Scored`
  come from the same events the engine uses to call `Mastery.Record`, so
  `MasteryPercent`, `FlaggedForReview`, and `OverallPercent()` reproduce the Task
  01 values (round(scored/asked*100); review if asked and below 50; overall = mean
  of ratios across the full objective set).
- **Schema versioned, forward-compatible.** Each record carries
  `schemaVersion` (currently 1). Loading maps only known keys, so unknown fields
  from a future writer are ignored; a newer `schemaVersion` is logged and read
  best-effort rather than rejected.
- **Corruption never crashes.** `ProgressStore.LoadOrNew` catches any parse or
  structural error, logs it through an injected sink, and returns a fresh session.
  The corrupt bytes are left on disk untouched so they can be inspected, rather
  than a valid file being silently overwritten.
- **Dependency-free JSON.** `Json.cs` is a small hand-written reader/writer
  (matching Task 01's choice to avoid a `System.Text.Json` NuGet dependency on
  netstandard2.1). Numbers parse to double; the model converts to int where
  needed. Timestamps serialize as round-trip ISO 8601 (`o`) in invariant culture.
- **Determinism.** No time or randomness inside the library. Every mutating call
  takes an explicit `nowUtc`; the tests inject a fake incrementing clock, so
  serialized output and behaviour are reproducible.

## The export schema (contract for Task 06)
`Export.BuildJson` produces, and `Export.Validate` enforces, a bundle of this
shape (Task 06 must read exactly these fields):
```
exportSchemaVersion : int
generatedUtc        : string (ISO 8601)
learnerSlotId       : string
startedUtc          : string
lastUpdatedUtc      : string
completed           : bool
overallPercent      : number
lensSwitches        : int
objectives[]        : { id, asked, scored, attempts, masteryPercent, flaggedForReview }
stations            : {
  throws[]             : { velocityMetresPerSecond, kineticEnergyJoules, atUtc }
  hillRuns[]           : { releaseHeightMetres, outcome, atUtc }
  pendulumShortfalls[] : { shortfallRadians, atUtc }
}
```
`Validate` also fails the bundle if any personal-data key is present.

## Flagged
- **Report filename.** This report is `TASK_REPORT-05-progress.md` so it does not
  overwrite Task 01's `TASK_REPORT.md` or Task 02's report. Same convention note
  as Task 02.
- No curriculum values were introduced or changed; the progress layer only
  records values produced by the (already reviewed) tutor and physics code.
- `keystore.txt` and the GPL-3.0 review remain as flagged in earlier tasks;
  untouched. New code stays isolated under `src/` and `tests/`.

## Notes for Task 06 (teacher view)
- Consume `Export.BuildJson` output; the field list above is the whole contract.
  A folder of these bundles (one per slot) is the teacher page's input; it never
  needs to read the raw `LearnerProgress` file or reference this assembly's code.
- The bundle is intentionally free of personal data, so the offline teacher page
  shows seats and mastery, not identities.
