# MIGRATION.md · Legacy OVR/Oculus API inventory and migration map

Scope: **project code only** (`Assets/Scripts`) plus the project scenes
(`Assets/Scenes/*.unity`) and the two project prefabs that carry OVR components.
Vendored SDK code under `Assets/Oculus` and `Assets/OVRInputSelection` is the
thing being replaced, so it is treated as the source of the legacy APIs, not as
code to migrate.

Target stack for every recommendation (all current, none deprecated):
- **XR Plugin Management + OpenXR** (with the Meta Quest feature group) or the
  **Oculus XR** provider, replacing the old built-in VR / `com.unity.xr.openvr.standalone`.
- **XR Interaction Toolkit (XRI)** `com.unity.xr.interaction.toolkit` for the rig,
  ray interaction, and world-space UI.
- **Input System** `com.unity.inputsystem` for controller input (actions), replacing `OVRInput`.
- **Meta XR Core SDK** `com.meta.xr.sdk.core` where a Meta-specific building block
  is genuinely needed (it still ships `OVRCameraRig`/`OVRManager`, but the
  future-proof path below prefers the vendor-neutral XRI/OpenXR equivalents).

No recommendation below points at the legacy Oculus Integration, OpenVR, or LWRP.

---

## 1. Summary table

### 1a. Project code (`Assets/Scripts`)

| File | Lines | Legacy API | What it does | Modern replacement | Difficulty |
| --- | --- | --- | --- | --- | --- |
| `PlayerEvents.cs` | 22-24, 31, 38-46, 60, 72-84, 91-109, 121-133, 151-176 | `OVRManager.HMDMounted/HMDUnmounted`; `OVRInput.Get/GetDown/GetUp`; `OVRInput.IsControllerConnected`; `OVRInput.GetActiveController`; `OVRInput.Controller/Button/Axis1D` enums | Central input hub. Pauses/resumes on headset mount; reads analog trigger; detects which single 3DOF remote (or touchpad/head) is active; raises app-wide `onTriggerDown/Up`, `onTouchpadDown/Up`, `OnControllerSource`, `Back` events | Rebuild on Input System actions (trigger, activate, select, menu) bound to XR controller bindings; headset presence via OpenXR user presence; re-model to two 6DOF Touch controllers. **Keep the existing `static UnityAction` event surface** so `Pointer.cs` and the canvas scripts need no change | **Needs design** |
| `Pointer.cs` | 170-181 (`UpdateOrigin`) | `OVRInput.Controller` (parameter + compare to `.Touchpad`) | Sets the laser origin to the active controller transform; hides the line when the source is the head/touchpad | Drive origin from the XR controller transform (XR Origin hand). The raycast, highlight, drag/drop logic is plain `Physics.Raycast`/`LineRenderer` and is engine-generic (no change). Optionally replace the whole pointer with an `XRRayInteractor` | **Needs design** (input source only; game logic is reusable) |
| `CanvasControllerForClass2.cs` | 171, 192, 225, 249, 281, 305 | `ControllerSelection.OVRRaycaster` (`.enabled`) | Enables/disables world-space UI raycasting per canvas as lesson steps change | `TrackedDeviceGraphicRaycaster.enabled` (XRI) | **Mechanical** (type swap) |
| `MultiStepCanvasInput.cs` | 229, 267 | `ControllerSelection.OVRRaycaster` (`GetComponent` + `.enabled`) | Same per-canvas raycaster toggle | `TrackedDeviceGraphicRaycaster` | **Mechanical** |
| `VoiceImageCanvasSync.cs` | 348 | `ControllerSelection.OVRRaycaster` (`.enabled`) | Same per-canvas raycaster toggle | `TrackedDeviceGraphicRaycaster` | **Mechanical** |

### 1b. Scenes and project prefabs (components placed in-editor)

| Asset(s) | Component (source) | Count | What it does | Modern replacement | Difficulty |
| --- | --- | --- | --- | --- | --- |
| All 5 scenes | `OVRCameraRig.prefab` instance (`OVRManager`, `OVRCameraRig`, tracking anchors) from `Assets/Oculus/VR/Prefabs` | Field 29, Loading 36, Module2Testing 29, SimRoom 37, Street 37 (prefab-instance refs) | The VR camera rig and runtime manager (tracking origin, eye cameras, hand anchors, HMD/runtime settings) | **XR Origin (Action-based)** with a Main Camera + `TrackedPoseDriver` and left/right controller objects. `OVRManager`'s runtime settings move to XR Plug-in Management / OpenXR provider settings | **Needs scene work** |
| All 5 scenes | `OVRInputModule` (`Assets/OVRInputSelection/InputSystem`) on the EventSystem | 1 each | Feeds VR pointer events into uGUI | `XRUIInputModule` (XRI) on the EventSystem | **Mechanical** (scene) |
| All 5 scenes | `OVRPointerVisualizer` (`Assets/OVRInputSelection/InputSystem`) | 1 each | Draws the laser/pointer visual | `XRInteractorLineVisual` on the ray interactor (XRI) | **Mechanical** (scene) |
| All 5 scenes | `BackButtonLogic` (`Assets/OVRInputSelection/Scripts`) | 1 each | Handles the hardware Back button for menu/return | Re-map to a Touch menu button (B/Y) driving in-app UI. See section 4 | **Needs design** |
| Field 3, Module2Testing 114, SimRoom 82, Street 43; `Assets/Prefeabs/InputCanvas.prefab`, `Assets/Scripts/EndGameControls.prefab` | `OVRRaycaster` (`Assets/OVRInputSelection/InputSystem`) on each world-space canvas | 242 refs total | Makes each world-space canvas a target for the VR pointer | `TrackedDeviceGraphicRaycaster` (XRI) on each canvas | **Mechanical but high volume** (scene work: hundreds of canvases) |

Note: the code reference `ControllerSelection.OVRRaycaster` (section 1a) and the
scene component `OVRRaycaster` are the same type, from
`Assets/OVRInputSelection/InputSystem/OVRRaycaster.cs`. Migrating the component
in the scenes and the `GetComponent<...>` calls in code must be done together.

---

## 2. Per-file detail

### `PlayerEvents.cs` (the input hub, highest-value target)
Legacy surface, by concern:
- **Headset presence** (lines 38-46): `OVRManager.HMDMounted/HMDUnmounted` toggle
  `inputActive`. Replace with OpenXR user-presence (subscribe to
  `InputDevices` / `XRDevice` presence, or the OpenXR user-presence feature).
- **Analog trigger** (line 60): `OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger)`
  → an Input System `float` action bound to `<XRController>/trigger`.
- **Controller discovery** (lines 72-84, 121-133): builds a set of
  `LTrackedRemote / RTrackedRemote / Touchpad` and picks the single active one.
  This is the Go/GearVR 3DOF model. Re-model to two persistent 6DOF Touch
  controllers (left/right). See section 4.
- **Per-controller button probes** (lines 91-109): `OVRInput.GetDown(Button.Any, ...)`
  and `OVRInput.GetActiveController()` → device-scoped actions or per-hand action maps.
- **App events** (lines 151-176): trigger down/up → `activate`; touchpad down/up
  → thumbstick-click or a face button; `Button.Back` → menu button. The public
  `static UnityAction` fields (`onTriggerDown`, `onTouchpadDown`, `OnControllerSource`,
  etc.) should be **preserved** so downstream scripts do not change.

Recommendation: keep `PlayerEvents` as the single indirection layer. Rewrite its
internals against Input System actions; keep its event API identical. This
localises the design work to one file and leaves `Pointer.cs` and the canvas
scripts as mechanical or no-op changes.

### `Pointer.cs`
Only OVR touch point is `UpdateOrigin(OVRInput.Controller, GameObject)` (lines
170-181): it takes the controller enum from `PlayerEvents.OnControllerSource`
and hides the `LineRenderer` when the source is the touchpad/head. Everything
else (raycast, highlight via `VRObject.applyHighlight`, drag/drop) is standard
Unity and needs no change. After `PlayerEvents` is migrated, either (a) keep the
callback but change the parameter type to a hand enum, or (b) replace the whole
pointer with an `XRRayInteractor` + `XRInteractorLineVisual` and forward its
hover/select to the existing `VRObject`/`Interactible` logic.

### `CanvasControllerForClass2.cs`, `MultiStepCanvasInput.cs`, `VoiceImageCanvasSync.cs`
Each only calls `.enabled` on a canvas's `ControllerSelection.OVRRaycaster`
(and one `GetComponent<ControllerSelection.OVRRaycaster>()` in
`MultiStepCanvasInput.cs:229`). After the scenes swap `OVRRaycaster` for
`TrackedDeviceGraphicRaycaster`, change the type name in these calls. Behaviour
(enable/disable per-canvas selection per lesson step) is unchanged. Purely
mechanical once the component type is chosen.

### Scenes
All five scenes share the same rig and pointer setup:
- Every scene instantiates the SDK `OVRCameraRig.prefab` (replace with one
  reusable XR Origin rig prefab).
- Every scene's EventSystem has `OVRInputModule` + a pointer using
  `OVRPointerVisualizer`, and a `BackButtonLogic`.
- `Module2Testing`, `Module2_SimulationRoom`, and `Module2_Street` carry many
  world-space canvases each with an `OVRRaycaster` (114 / 82 / 43 references).
  This is the bulk of the manual scene work: a repetitive component swap on
  every canvas.

No `OVRGrabbable`, `OVRGrabber`, `DistanceGrabber`, or other grab components
appear in the project scenes or prefabs. Object pickup/drag is implemented in
project code (`VRObject`, `VRDraggableObject`, `Pointer.Drag/Drop`), which is
engine-generic and carries over unchanged.

---

## 3. Order of attack

1. **Provider and packages (prerequisite, no gameplay code).** Add XR Plug-in
   Management with OpenXR (Meta Quest feature group) or the Oculus provider;
   add XR Interaction Toolkit and Input System. Remove
   `com.unity.xr.openvr.standalone`. (Separately, LWRP →  URP; see appendix.)
2. **Rig once.** Build one XR Origin rig prefab (camera + two controllers +
   ray interactors + line visuals + `XRUIInputModule`), then drop it into each
   scene in place of `OVRCameraRig`. This retires `OVRCameraRig`, `OVRManager`,
   `OVRInputModule`, and `OVRPointerVisualizer` in one reusable asset.
3. **Input hub.** Rewrite `PlayerEvents` internals on Input System actions while
   preserving its `static UnityAction` API. This is the one true design task and
   unblocks everything downstream.
4. **Pointer.** Repoint `Pointer.cs` origin at the XR controller (or swap to
   `XRRayInteractor`). No change to its highlight/drag logic.
5. **UI raycasters (bulk).** Swap `OVRRaycaster` → `TrackedDeviceGraphicRaycaster`
   on every world-space canvas, then update the three canvas scripts' type
   references. Mechanical but high-volume; do it scene by scene
   (Module2Testing is the largest at 114).
6. **Back button.** Design a Touch menu-button affordance to replace
   `BackButtonLogic` and `OVRInput.Button.Back` (pause/exit/restart canvas).

---

## 4. No clean modern equivalent (RECOMMENDED default mapping, to confirm on hardware)

These come from the Oculus Go / GearVR **3DOF, single-remote** input model. Quest
2/3 use two **6DOF Touch** controllers, so there is no 1:1 mapping. The mappings
below are **RECOMMENDED, not decided**. They follow the locked project decisions
(target Quest 2/3; learner input is controller and menu only) and reproduce the
current pointer-and-menu behaviour with the least design churn. The final input
mapping should be confirmed against the actual XRI defaults during the migration,
since none of this has been tested on hardware yet.

1. **Single active 3DOF controller + head-gaze/touchpad fallback**
   (`OVRInput.Controller.LTrackedRemote / RTrackedRemote / Touchpad`, and the
   "pick the one connected remote, else head" logic in
   `PlayerEvents.checkForController` and `UpdateSource`). Quest always has two
   tracked controllers, so the single-active-source model does not carry over.
   **RECOMMENDED:** a single XRI `XRRayInteractor` on the **dominant hand** drives
   the laser pointer, reproducing today's single-pointer 3DOF behaviour and the
   "one pointer at a time" assumption the lesson logic already makes. Leave the
   non-dominant hand idle for now (it can mirror the ray later if wanted), and
   drop the head-gaze fallback unless later accessibility testing calls for it.
2. **Hardware Back button** (`OVRInput.Button.Back`, `BackButtonLogic`) and
   **touchpad click** (`OVRInput.Button.PrimaryTouchpad`, the `onTouchpad*`
   events, and the `Touchpad` line-hiding branch in `Pointer.UpdateOrigin`).
   Touch controllers have neither a Back button nor a touchpad.
   **RECOMMENDED:** map the controller **menu button** to open the dialogue and
   menu system, taking over both the old touchpad role (advance/select in the
   menu) and the Back-button role (open the pause/exit menu). This matches the
   locked decision that learner input is controller and menu only. The trigger
   stays the primary select/interact action (`activate`) via the ray interactor.
3. **Confirm on hardware.** The exact button (the menu button versus a face
   button such as B/Y) and the precise XRI action-map bindings must be verified
   against the XRI defaults and on a real Quest 2/3 during the migration. Treat
   the mappings above as the starting point, not the final binding.

Everything else in the table is either mechanical (component/type swap) or a
localised design change inside `PlayerEvents`.

---

## Appendix · Related migration work, out of scope for this document
These items are not OVR/Oculus API and are listed only so this inventory is not
mistaken for exhaustive coverage of the migration. See the Coding Brief (v0.6),
Workstream 0, for the detail and sequencing of each.

- `com.unity.xr.openvr.standalone` (2.0.5): deprecated and off-target; remove.
  (Already removed on the `chore/cleanup` branch; see `CLEANUP_REPORT.md`.)
- `com.unity.render-pipelines.lightweight` (7.2.1, LWRP): superseded by URP
  (`com.unity.render-pipelines.universal`). Not an OVR API, but it is a
  prerequisite of the engine upgrade and affects shaders/materials in the scenes.
- The vendored `Assets/Oculus` (legacy Oculus Integration) is fully superseded by
  the Meta XR SDK packages; it should be removed only after the rig, input, and
  UI migrations above are complete and verified in the Editor.

## How this map was produced (reproducible)
- Text scan of `Assets/Scripts/**/*.cs` for `OVR`/`Oculus` identifiers and
  `using` directives (found 5 files).
- GUID map of all `*.cs.meta` under `Assets/Oculus` and `Assets/OVRInputSelection`
  (407 script GUIDs), cross-referenced against the 5 scene `.unity` files and the
  project prefabs to find in-editor OVR components and the `OVRCameraRig` prefab
  instances.
