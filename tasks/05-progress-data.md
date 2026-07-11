# Task 05 · Learner progress model, save and resume

## Goal
The on-device progress layer both the module and the teacher view consume.

## Deliverables
```
src/LeVRn.Progress/          netstandard2.1, no UnityEngine
  LearnerProgress.cs         session + per-objective mastery + station log
  ProgressStore.cs           JSON persistence behind an IStorage interface
  Export.cs                  teacher-view export bundle (see Task 06)
tests/LeVRn.Progress.Tests/
TASK_REPORT.md
```

## Requirements
- Data per learner session: anonymous learner slot id (device-local, no
  personal data fields AT ALL), timestamps, per-objective asked/scored,
  attempts, lens switches, station events (throws with v and KE, hill runs
  with h0 and outcome, pendulum shortfall), completion state.
- Save after every scored event; a resumed session continues mid-script.
- IStorage abstraction with a file implementation (tests) so the Unity
  layer can supply Application.persistentDataPath later.
- Schema versioned; unknown future fields ignored on load.

## Acceptance tests
1. Round trip: save mid-session, load, resume; final mastery identical to
   an uninterrupted run with the same answer profile (drive with the Task 01
   engine if present, else a stub emitting the same events).
2. Corrupt file loads as a fresh session with the corruption logged, never
   a crash.
3. Serialized JSON contains no fields for name, age, or any personal data.
4. Export bundle validates against the schema Task 06 consumes.
