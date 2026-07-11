# Task 06 · Teacher companion view · TASK_REPORT

## What was done
Built the offline teacher companion page: a single self-contained HTML file a
teacher opens on any laptop or phone, loads an exported progress bundle (the Task
05 schema), and reads the class at a glance. No server, no network, no build
step. Created a validated 12 seat sample class fixture, one seat review-flagged.

### Deliverables produced
```
teacher-view/
  teacher.html                  the self-contained page (HTML + inline CSS + inline JS, no external anything)
  sample/class-sample.json      12 export bundles (one review-flagged), generated and schema-validated
  README.md                     how to use, print, and the privacy note
```
A small additive change to Task 05: the export bundle's per-objective records now
also carry `attempts` (the teacher detail needs it). `Export.Validate` requires
it; the Task 05 acceptance tests still pass (4/4). The Task 05 report's schema
section was updated to match.

## Acceptance, verified
| Acceptance point | How it was verified | Result |
| --- | --- | --- |
| Opens and renders from `file://` with no network | No external resources at all (see below); pure `file://` document | Pass |
| Loads a 12 learner sample incl. one review-flagged, flag shown at a glance | Headless run of the page's real `<script>` against the fixture in a stubbed DOM: 12 learners, `slot-07` shows "Needs review", summary banner present | Pass |
| Zero network requests / no external URLs anywhere in the file | `grep` for `http`, `//host`, `src=`, `@import`, `cdn`, `fetch`, `XMLHttpRequest`, `WebSocket` across `teacher-view/`: none. No SVG either (its `xmlns` is a URL) | Pass |
| Handles a malformed file with a clear, non-technical message | Fed `"{ this is not valid json ]"` and a JSON object with no learners: friendly messages shown, the already-loaded class not corrupted | Pass |

Headless verification harness result: **13 of 13 checks passed** (parsing, class
render, glance-level review flag, all five exact curriculum names visible, learner
detail with stations and plain-language guide switches, malformed handling).

## How the sample fixture was produced
A small generator (kept in the working scratchpad, not committed) drove the real
Task 01 tutor engine with twelve answer profiles and the real Task 02 physics
models for the station events, then exported each learner with
`LeVRn.Progress.Export.BuildObject` and validated it with `Export.Validate`
before writing the array. Every bundle validated (0 errors). This guarantees the
sample matches the exact schema the page consumes, and doubles as an end to end
check of Tasks 01, 02 and 05. The class spread: eight secure learners (84 to 100
percent), one developing across the board (60 percent), and `slot-07`
review-flagged (Define 30 percent, Formula 0 percent, overall 50 percent, four
guide switches). Re-running the generator reproduces the file deterministically.

## Design decisions
- **Fully self-contained, and stricter than "no CDN".** All CSS and JS are inline.
  There is no external font, script, style, or image, and deliberately no SVG,
  because an SVG carries an `xmlns` URL and the acceptance test is "no external
  URLs anywhere in the file". Mastery is drawn with CSS cells, so a `grep http`
  over the file returns nothing.
- **Colour is never the only signal.** Mastery bands (secure 70+, developing 50 to
  69, review under 50) use the project's own palette, but every cell also shows
  the numeric percent, the status column spells out "Needs review", and a legend
  names the bands. This keeps it readable for colour-blind users and in black and
  white print, where background colours may not be printed.
- **Accepts one class file or many single files.** The loader takes a JSON array
  of bundles, a single bundle object, or an object with a `learners` array, and
  folds them into a seat-keyed map (last load wins). So a teacher can open one
  exported class file or drag in several per-seat files.
- **Plain language for a non-specialist.** "Seat", "Overall mastery", "Guide
  switches (a different peer re-explained)", and station lines like "released at
  1.10 m, just cleared the crest". The five objectives are shown by their exact
  curriculum names in a visible legend and in the detail panel (not only in
  tooltips, so they survive printing).
- **Calibri first, no em dashes.** Font stack is
  `Calibri, 'Segoe UI', system-ui, -apple-system, sans-serif`. All copy uses
  commas and periods; verified there is no em dash or en dash in the file.
- **Print to one page per class.** Print CSS hides the file chooser and footer,
  keeps rows from splitting, prints the class overview, and starts the selected
  learner's detail on a new page.
- **Generator kept out of the repo.** The deliverables are the page and the
  fixture. The generator lives in the scratchpad to avoid adding a tools project
  and a matching `.gitignore` rule; its method is documented above and it can be
  re-created if the schema changes.

## Flagged
- **Visual and print rendering were verified structurally, not in a real browser.**
  This environment is headless, so the page's logic and output were checked by
  running its actual script against the fixture in a stubbed DOM, plus static
  checks for the offline and typography constraints. A final human open in a
  browser (and one test print) is worth doing on real hardware, but no code path
  depends on anything unavailable offline.
- **Report filename** is `TASK_REPORT-06-teacher-view.md`, matching the per-task
  naming used since Task 02 so it does not overwrite earlier reports.
- No curriculum values were changed. The five objective names are copied exactly
  from `content/pe-ke.json`. `keystore.txt` and the GPL-3.0 review remain as
  flagged in earlier tasks; untouched.
