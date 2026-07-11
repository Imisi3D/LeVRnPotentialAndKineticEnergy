# Task 06 · Teacher companion view (offline web page)

## Goal
A single self-contained HTML file a teacher opens on any laptop or phone,
loading an exported progress JSON bundle and showing the class at a glance.
No server, no network, no build step.

## Requirements
- File input (and drag-drop) accepting the Task 05 export bundle.
- Views: class overview (per-learner-slot mastery across the five
  objectives, review flags prominent), and a learner detail (attempts,
  lens switches, station events like throw speeds and hill iterations).
- Plain-language labels a non-specialist teacher understands; the five
  objectives named exactly as in the curriculum.
- Works offline from a file:// open; no external fonts, scripts, or styles.
- Font stack starts with Calibri. No em dashes in any copy.
- Prints acceptably to one page per class for paper record-keeping.

## Acceptance
- Opens and renders correctly from file:// with the network disabled.
- Loads a provided sample export (create sample fixtures for a 12-learner
  class including one review-flagged learner) and shows the review flag
  within one glance-level view.
- Zero network requests (verify: no external URLs anywhere in the file).
- Handles a malformed file with a clear, non-technical error message.
