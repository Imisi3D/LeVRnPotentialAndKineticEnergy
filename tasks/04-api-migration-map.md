# Task 04 · Deprecated API inventory and migration map

## Goal
A complete, file-by-file inventory of every legacy Oculus/OVR API the
project code uses, mapped to its current Meta XR SDK / OpenXR / XR
Interaction Toolkit equivalent, so the human-led Editor migration is a
checklist rather than a discovery exercise.

## Method
- Scan Assets/Scripts (project code only) for identifiers: OVRInput,
  OVRManager, OVRCameraRig, OVRRaycaster, OVRPointerVisualizer, OVRGrabbable,
  and any other OVR*/Oculus.* usage, plus scene .unity files referencing
  OVR component GUIDs.
- For each usage: file, line, what it does functionally (e.g. "reads
  trigger press", "laser pointer visual"), and the recommended replacement
  (e.g. XRI XRRayInteractor, InputAction, OVRControllerHelper's successor),
  with a difficulty note (mechanical / needs design / needs scene work).

## Deliverable
MIGRATION.md: summary table plus per-file detail; a short "order of attack"
section; explicit list of anything with no clean modern equivalent.

## Acceptance
- Every project script that references OVR/Oculus appears in the table.
- Zero recommendations reference deprecated packages.
- A reviewer can start the Editor migration from this document alone.
