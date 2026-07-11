# Task 01 · Port the tutor engine to C#

## Goal
Reimplement the verified tutor engine from `reference/tutor_prototype.html`
(the `<script>` block) as an engine-agnostic C# library plus tests. This is
the adaptive core both the VR module and future modules will reuse.

## Deliverables
```
src/LeVRn.Tutor/            netstandard2.1 class library, no UnityEngine
  TutorEngine.cs            state machine: step, answer, adapt
  TutorScript.cs            node types: Say, Ask, Scene, End
  ResponseBank.cs           authored content container (affirm/hint/switch/walkthrough)
  Mastery.cs                per-objective asked/scored tracking
  Guides.cs                 Niyat, Morenike, Ejike identities and lenses
  Events.cs                 engine events the UI layer subscribes to
tests/LeVRn.Tutor.Tests/    NUnit or xUnit, runs with dotnet test
content/pe-ke.json          the full authored script from the prototype,
                            externalised as data (verbatim text, no edits)
TASK_REPORT.md
```

## Behaviour to match exactly (from the reference implementation)
- Node sequence: Say advances on Continue; Ask presents options.
- Wrong answer handling per question:
  attempt 1 wrong -> hint text, same guide, re-ask;
  attempt 2 wrong -> lens switch: switch guide, switch text, re-ask;
  attempt 3 wrong -> walkthrough text, objective counted as asked with
  score 0 (flags review), advance.
- Scoring gains by attempt on which the correct answer landed:
  attempt 1 = 1.0, attempt 2 = 0.6, attempt 3 = 0.3, never = 0.0.
- Difficulty adaptation: if any `define` objective question needed more than
  one attempt, insert the two extra classification items immediately before
  Niyat's formula introduction, exactly once per session.
- Objectives: define, classify, formula, convert, conserve.

## Acceptance tests (must all pass; expected values are pre-verified)
1. STRONG learner (every answer correct first try):
   - questions asked per objective == [2, 2, 2, 3, 1]
   - mastery per objective == 100% for all five
   - lens switches == 0; extra items NOT inserted.
2. WEAK learner (define needs 3 attempts, classify 2, formula never correct,
   convert 2, conserve 1):
   - extra classification items ARE inserted (classify asked == 4)
   - lens switches >= 4
   - formula objective flagged for review (score 0, asked > 0)
   - mastery pattern == [30%, 60%, 0%, 60%, 100%] within rounding.
3. Adaptation idempotence: extras insert at most once even if the insertion
   point is evaluated repeatedly.
4. Content integrity: loading `content/pe-ke.json` yields the same number of
   authored responses as the reference (count Say nodes once and each Ask
   node's affirm, hint, switchText, walkthrough).
5. Determinism: two runs with the same answer profile produce identical
   event sequences.

## Notes
- Externalise ALL learner-facing text into `content/pe-ke.json`; the engine
  must contain no hardcoded lesson text. Copy text verbatim from the
  reference, including punctuation. No em dashes exist in it; keep it so.
- Expose the engine through events/callbacks so a Unity layer, a web layer,
  or a test harness can drive it identically.
