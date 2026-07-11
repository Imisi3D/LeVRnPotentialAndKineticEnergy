# Task 03 · Repository hygiene branch

## Goal
A reviewable branch `chore/cleanup` that slims the repository without
breaking the existing project's ability to open, plus a written report.
This is preparation for migration, not the migration itself.

## Scope · in
- Remove clearly unused vendored sample content: Oculus SDK sample scenes
  and their exclusive assets (Avatar samples, Platform samples VrHoops /
  VrBoardGame / NetChat / VrVoiceChat, LipSync demo scenes, Spatializer
  scenes, VoiceMod, SampleFramework Usage scenes). Verify before each
  removal that nothing under Assets/Scripts or Assets/Scenes references it
  (search by GUID in .meta files and by type name in code).
- Remove `com.unity.collab-proxy` and `com.unity.xr.openvr.standalone`
  from Packages/manifest.json (deprecated; OpenVR is not a target).
- Write a proper Unity .gitignore (Library/, Temp/, Logs/, Obj/, Build/).
- FLAG `keystore.txt` in the report: recommend removal and secret rotation.
  Do NOT delete it or rewrite history in this task; that needs approval.

## Scope · out
- Do not remove Assets/Oculus/VR, Assets/Oculus/AudioManager runtime code,
  or Assets/OVRInputSelection: project scripts may depend on them until the
  migration task replaces them.
- No engine or package upgrades here.

## Acceptance
- A CLEANUP_REPORT.md listing every removed path, the evidence it was
  unreferenced, repo size before and after, and the keystore flag.
- No file under Assets/Scripts or Assets/Scenes modified.
- Branch applies cleanly on master.
