# Task 03 · Repository hygiene · CLEANUP_REPORT

Branch: `chore/cleanup` (created from `master`, applies cleanly on master).
This is preparation for the Quest migration, not the migration itself. No file
under `Assets/Scripts` or `Assets/Scenes` was modified.

## Summary
| | |
| --- | --- |
| Vendored sample folders removed | 8 |
| Files removed (incl. `.meta`) | 316 |
| `Assets/` size before | 2,169,216 KB (~2.069 GB) |
| `Assets/` size after | 2,145,769 KB (~2.046 GB) |
| Reclaimed | 23,447 KB (~22.9 MB) |
| Packages removed from manifest | 2 |

## 1. Removed sample content (with evidence)

Every folder below was removed together with its sibling `.meta`. Before
removal, all 490 GUIDs contained in these folders were cross-referenced against
the project surface (`Assets/Scenes`, `Assets/Scripts`, `Assets/Prefabs`,
`Assets/Prefeabs`, `Assets/Resources`, `ProjectSettings`) and against project
C# code by type name. None were referenced.

| Removed path | Size | Files | Evidence it was unreferenced |
| --- | --: | --: | --- |
| `Assets/Oculus/Avatar/Samples/` | 389 KB | 70 | No GUID in project surface; sample scripts (SocialStarter P2P/Room/Voip) referenced only within the folder |
| `Assets/Oculus/Platform/Samples/NetChat/` | 50 KB | 4 | No GUID in project surface; namespace `Oculus.Platform.Samples.NetChat` used only internally |
| `Assets/Oculus/Platform/Samples/VrBoardGame/` | 154 KB | 30 | No GUID in project surface; namespace declared/used only internally |
| `Assets/Oculus/Platform/Samples/VrHoops/` | 279 KB | 54 | No GUID in project surface; namespace declared/used only internally |
| `Assets/Oculus/Platform/Samples/VrVoiceChat/` | 86 KB | 18 | No GUID in project surface; namespace declared/used only internally |
| `Assets/Oculus/LipSync/Scenes/` | 174 KB | 9 | Demo scene only; its scripts (`EnableSwitch`, `LipSyncDemo_Control`, `LipSyncDemo_SetCurrentTarget`) have zero references anywhere in `Assets/`. LipSync runtime (Scripts/Plugins/Prefabs) kept |
| `Assets/Oculus/Spatializer/scenes/` | 15,521 KB | 70 | Demo scenes + art only (RedBallGreenBall, YellowBall); no scripts here. Spatializer runtime (Plugins/scripts/editor) kept |
| `Assets/Oculus/VoiceMod/` | 6,793 KB | 53 | Self-contained demo; its classes (`OVRVoiceMod`, `OVRVoiceModContext`, `OVRMessenger`, `OVRDebugConsole`, `OVRMicInput`) referenced only inside VoiceMod (11/11 matches internal) |

Verification method (reproducible):
- GUID extraction from every `*.meta` in each candidate folder, then a literal
  scan of the project-surface files for those GUIDs (0 hits for these 8).
- Type-name / namespace grep of project code and of the whole `Assets/` tree
  for the sample classes and namespaces (0 external hits).

## 2. Kept deliberately (scope and safety)
- `Assets/Oculus/VR`, `Assets/Oculus/SampleFramework/Core`,
  `Assets/Oculus/AudioManager`, `Assets/OVRInputSelection`: runtime the project
  scripts may depend on until the migration replaces them (per task scope-out).
- LipSync, Spatializer runtime code and plugins (only their demo scenes/art were
  removed).
- `Assets/Oculus/Platform/Samples/EntitlementCheck` and `.../SimplePlatformSample`:
  not named in the task scope, so left in place (conservative).

## 3. NOT removed despite being listed in the task — `SampleFramework/Usage`
The task lists "SampleFramework Usage scenes" as in-scope, but verification found
that the project's own scenes depend on art assets inside that folder, so it was
**kept**. Removing it would break scenes and violate "must not break the
project's ability to open." The project references:

| Referenced asset (under `SampleFramework/Usage/`) | Used by project scene(s) |
| --- | --- |
| `SharedAssets/Models/ball_soccer_01.fbx` | Field |
| `SharedAssets/Materials/SoccerBlack.mat`, `SoccerWhite.mat`, `GroundMat.mat` | Field, Module2Testing |
| `SharedAssets/Skyboxes/Sky_01/Skybox01.mat` | Field, Module2_Street |
| `DistanceGrab/Materials/MAT_watergun_01.mat`, `DistanceGrab/GroundMaterial.mat` | Module2Testing, Module2_Street |
| `GuardianBoundarySystem/PlayAreaCubeMat.mat`, `Materials/MAT_floor_wood_02.mat`, `Materials/MeterGrid.mat` | Module2Testing |
| `Locomotion/Materials/MAT_grid_01_dark.mat`, `MAT_floor_wood_01.mat` | Module2Testing, Module2_Street |

Recommendation: during the migration, re-home these few art assets into a
project-owned folder, then `SampleFramework/Usage` can be dropped in full.

## 4. Packages/manifest.json
Removed two deprecated dependencies (JSON re-validated after editing):
- `com.unity.collab-proxy` (1.2.16) — deprecated Unity Collaborate proxy.
- `com.unity.xr.openvr.standalone` (2.0.5) — OpenVR is not a target platform.

## 5. .gitignore
No change required. `master` already ships the canonical GitHub Unity
`.gitignore`, which ignores `/[Ll]ibrary/`, `/[Tt]emp/`, `/[Oo]bj/`,
`/[Bb]uild/`, `/[Bb]uilds/`, `/[Ll]ogs/`, and `/[Mm]emoryCaptures/`, and keeps
`*.meta` for tracked assets. Confirmed that zero files under any of those
generated directories are currently tracked, so there was nothing to
`git rm --cached`.

## 6. FLAG · keystore.txt (not touched)
`keystore.txt` remains committed at the repo root. It was NOT deleted and git
history was NOT rewritten, per the task boundary (that needs maintainer
approval).

Recommendation:
- Treat the committed keystore and any password it protects as compromised.
  Generate a new keystore, rotate the signing key/secret, and store it outside
  version control (a secrets manager or the CI signing store).
- Once a replacement is in place and approved, purge `keystore.txt` from history
  (e.g. `git filter-repo`) and force-update the remote. This is a separate,
  approval-gated action and is intentionally out of scope for this branch.

## How to review
- `git show --stat` on the cleanup commit lists every removed path.
- Project opens with the same scenes and scripts; only unused vendored samples,
  two deprecated packages, and their exclusive assets are gone.
