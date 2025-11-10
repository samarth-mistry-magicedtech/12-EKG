# SB12 Scene Builder – Master Prompt for Editor Script Generation

Use this prompt to generate a Unity Editor script that auto-builds the SB12 EKG scene using ONLY assets under `Assets/3DModelsElectrode` and XR Interaction Toolkit samples. The script should be idempotent: re-running updates or replaces created objects safely.

## Requirements

- Unity: 6000.0.32f1
- XR Interaction Toolkit: use `XR Origin (XR Rig)` prefab from Samples
- Active assets folder: `Assets/3DModelsElectrode`
- No references to removed folders (`Assets/Art`, `Assets/Code`, `Assets/YAML`)

## Design goals (optimization + quality)

- Idempotent builder: re-run without duplicating objects; update transforms/materials safely.
- Deterministic hierarchy: fixed names and parentage so lookups are O(1) by path.
- Single-pass creation with minimal `Find`/`GetComponent` calls (cache references).
- Structured logging: clear summary of actions, warnings, and missing assets.
- Undo/Redo support and progress bar while building.
- Zero hard deps on deleted folders; only `Assets/3DModelsElectrode` and XR samples.

## Preflight validation

Before building, the script should validate and report:
- XR packages available: XR Interaction Toolkit installed (presence of sample `XR Origin (XR Rig)` prefab).
- LFS assets present: FBX files from `Assets/3DModelsElectrode` are larger than small pointer files (e.g., > 10 KB). If any are suspiciously small, log a warning suggesting `git lfs pull`.
- Asset existence: patient, machine console, pad with back, backing peeled, electrode, VGA plug. Missing ones are logged; the build proceeds with available items.

## Deliverables (file paths and names)

- Editor script: `Assets/Editor/SB12SceneBuilder.cs` (static class with `[MenuItem("Tools/SB12/Build Scene")]`).
- Runtime scripts (created only if missing):
  - `Assets/Scripts/SB12/GameState.cs`
  - `Assets/Scripts/SB12/PadPlacement.cs`
  - `Assets/Scripts/SB12/SlideController.cs`
  - `Assets/Scripts/SB12/SlideSet.cs` (ScriptableObject data model for slides)
- Scene output: `Assets/Scenes/SB12_Auto.unity` (create folder if missing).
- Optional materials created at build time (unlit color markers): `Assets/Scenes/Generated/Materials/*`.

## Script Output

Create a menu item: `Tools/SB12/Build Scene`
Running it will:

1) Scene bootstrap
- Create/clear root `RoomRoot` GameObject.
- Add `XR Interaction Manager` and `EventSystem` with `XR UI Input Module` if missing.
- Instantiate `XR Origin (XR Rig)` prefab from samples at pos (-1.2,0,-1.2), facing the bed.

2) Room from primitives
- Floor (Plane) at (0,0,0), scale (6,1,6)
- Four walls (scaled Cubes) at:
  - North (0,1.5,3) rot (0,180,0) scale (6,3,0.1)
  - South (0,1.5,-3) rot (0,0,0) scale (6,3,0.1)
  - East (3,1.5,0) rot (0,-90,0) scale (6,3,0.1)
  - West (-3,1.5,0) rot (0,90,0) scale (6,3,0.1)
- Lighting: create one Directional Light (rot (50, -30, 0)) and two Point Lights near bed and cart.

3) Bed + Patient
- Bed base (Cube) at (0,0.35,0), scale (2.0,0.3,0.8)
- Mattress (Cube) at (0,0.7,0), scale (2.0,0.12,0.8)
- Headrest (Cube) at (0.8,0.78,0), rot (0,0,10), scale (0.35,0.08,0.8)
- Instantiate `EKG Patient In Bed.fbx` at (0,0.82,0), rot (0,-90,0)
- Create `PatientAnchor` empty at sternum (0.00,0.10,0.00) relative to patient chest

4) Cart + Machine + Tray + Rack
- Cart top (Cube) at (1.2,0.92,0.45), scale (0.9,0.06,0.6)
- Cart legs (4 Cubes) to floor
- Instantiate `EKG Machine Console.fbx` at (1.2,0.98,0.45), rot (0,-30,0)
- Tray (Cube) at (1.2,0.9,0.0), scale (0.6,0.03,0.4)
- Rack bar (Cylinder) at (1.2,1.05,-0.4), rot (0,0,90), scale (0.7,0.01,0.01)

5) Electrode target zones (empties with triggers)
- Create empties under `PatientAnchor`:
  - Limb: `Mount_RA (+0.55,0.00,0.15)`, `Mount_LA (-0.55,0.00,0.15)`, `Mount_RL (+0.35,-0.55,0.25)`, `Mount_LL (-0.35,-0.55,0.25)`
  - Chest: `Mount_V1 (+0.05,0.05,-0.05)`, `Mount_V2 (-0.05,0.05,-0.05)`, `Mount_V3 (-0.10,0.06,-0.03)`, `Mount_V4 (-0.16,0.06,0.00)`, `Mount_V5 (-0.22,0.07,0.00)`, `Mount_V6 (-0.28,0.07,0.00)`
- Add SphereCollider (isTrigger=true, radius ~0.03) to each mount and a small colored Quad as a marker.

6) Pads on the Tray (grabbable)
- Instantiate `EKG Pad With Back.fbx` 10x parented to Tray in a grid:
  - Start (1.12,0.93,0.02), spacing (0.06,0,0.06)
- Add `Rigidbody`, `BoxCollider`, and `XR Grab Interactable` to each.
- Optional peel effect: On first grab, disable backing child if present and instantiate `EKG Backing Peeled.fbx` near the hand.

7) Power button (interactable)
- Create a small Cube `PowerButton` on the console front-right with `XR Simple Interactable` and a highlight material. Initially disabled; enable after all pads placed.

8) Minimal logic components (Editor-generated MonoBehaviours)
- Create two scripts alongside the scene (if not present):
  - `PadPlacement.cs`: OnTriggerEnter with a `Mount_*` snaps/locks the pad; invokes `GameState.ReportPadPlaced(string mountName)`; plays a click/attach sound (if available) or UnityEvent.
  - `GameState.cs`: tracks which mounts are filled; when 10 are placed, enables `PowerButton` highlight and a waveform panel; exposes events for UI updates.

9) World-space UI (Slides)
- Create a world-space Canvas `SB12_Panel` (size ~1.1m x 0.6m) at (-0.2,1.5,-1.0), rot (0,15,0) with `Tracked Device Graphic Raycaster`.
- Add Text elements and a `Continue` Button.
- Generate a `SlideController.cs` with the 13 slides from the document (section 2.5), advancing on button click and invoking actions at slides 4,5,6,9.

10) Teleport anchors
- Create empties with `TeleportationAnchor` components at:
  - Entry (-1.2,0,-1.2)
  - Bedside (0.2,0,0.8)
  - Cart (1.4,0,0.4)

11) Save scene
- Save as `Assets/Scenes/SB12_Auto.unity` (create `Assets/Scenes` if needed).

## Implementation Hints

- Use `AssetDatabase.LoadAssetAtPath<T>(path)` for FBX under `Assets/3DModelsElectrode`.
- Use `PrefabUtility.InstantiatePrefab` for prebuilt prefabs (XR Origin).
- Wrap creation in helpers so repeated runs update transforms without duplicating.
- Guard with `#if UNITY_EDITOR` and put script in `Editor/` folder.

### Hierarchy & naming contract (deterministic)

Create/ensure the following hierarchy exactly (names used for idempotent lookups):

```
RoomRoot
  XR
    XR Interaction Manager
    EventSystem (+ XR UI Input Module)
    XR Origin (XR Rig)
  Lighting
    Directional Light
    Point Light (Bed)
    Point Light (Cart)
  Environment
    Floor
    Wall_North
    Wall_South
    Wall_East
    Wall_West
  Bed
    Bed_Base
    Bed_Mattress
    Headrest
    Patient (EKG Patient In Bed)
    PatientAnchor
      Mounts
        Mount_RA, Mount_LA, Mount_RL, Mount_LL
        Mount_V1, Mount_V2, Mount_V3, Mount_V4, Mount_V5, Mount_V6
  Cart
    Cart_Top
    Cart_Leg_FL / FR / BL / BR
    MachineConsole (EKG Machine Console)
    PowerButton
    Tray
      Pads
        Pad_01..Pad_10 (EKG Pad With Back)
    Rack
      Rack_Bar
  UI
    SB12_Panel (world-space)
```

### Idempotent build algorithm (pseudocode)

```
Begin Undo group "SB12 Build";
Show ProgressBar;
Validate packages & assets; Log warnings; continue;
root = FindOrCreate("RoomRoot");
CreateOrUpdateXR(root/XR);
CreateOrUpdateLighting(root/Lighting);
CreateOrUpdateEnvironment(root/Environment);
CreateOrUpdateBedAndPatient(root/Bed);
CreateOrUpdateCartAndMachine(root/Cart);
CreateOrUpdateMounts(root/Bed/Patient/PatientAnchor/Mounts);
CreateOrUpdatePads(root/Cart/Tray/Pads);
CreateOrUpdateUI(root/UI);
ConnectBehaviours(GameState, PadPlacement, SlideController);
Save scene to Assets/Scenes/SB12_Auto.unity;
Clear ProgressBar; End Undo group;
```

All `CreateOrUpdate*` helpers must:
- Find by exact path; if exists, update transforms/components; if missing, create.
- Never duplicate children; use a static dictionary of expected children to reconcile.
- Use cached references to reduce `GetComponent` calls.

### Slide data model

Implement `SlideSet` ScriptableObject with an array of `Slide`:
- `id` (string), `title` (string), `body` (string), `footer` (string), `onAdvanceAction` (enum: None, ActivateEquipment, EnablePads, EnableWires, EnablePowerButton), optional `conditions` (e.g., RequirePadsPlaced=10, RequireAllConnected=true).

Seed 13 slides exactly per SB12_EKG_GameFlow_UI_Layout.md §2.5. Examples:
- Slide1: Task Intro → None
- Slide2: Skin Prep → None
- Slide3: Adhesion Tip → None
- Slide4: Equipment Appears → ActivateEquipment
- Slide5: Markers + Peel → EnablePads
- Slide6: Wire Matching → EnableWires (placeholder)
- Slide7: Order Hint → None
- Slide8: Verify Prep (Condition: PadsPlaced==10)
- Slide9: Power On (Condition: PadsPlaced==10; Action: EnablePowerButton)
- Slide10–13: Completion, Success, Rationale, Exit

### Optimization tactics

- Cache all created/transformed objects in a struct with references to avoid repeated lookups.
- Use `EditorUtility.SetDirty` only when values change.
- Batch setup: create objects inactive → configure → activate once to avoid expensive re-evaluations.
- Create minimal unlit materials for markers once under `Assets/Scenes/Generated/Materials/` and reuse.
- Use constants for all asset paths under `Assets/3DModelsElectrode` and a fallback GUID search once; cache results.
- Guard physics: temporarily set `Physics.autoSyncTransforms = false` during bulk transforms; restore afterwards.

### Error handling & logging

- Use a lightweight logger that aggregates messages and prints a final summary:
  - Created X, Updated Y, Missing Z assets, Warnings N.
- If a critical asset is missing (e.g., Patient FBX and fallback not found), place a proxy Cube with label; continue build.
- If `XR Origin (XR Rig)` prefab not found, log an explicit remediation hint (import XR samples).


## Acceptance Criteria

- Running `Tools/SB12/Build Scene` produces a playable scene with XR locomotion, patient, machine, tray, mounts, 10 grabbable pads, UI slides, and a working power button gate.
- Scene hierarchy matches the naming contract so future tools can reliably find objects.
- No references to removed folders; all meshes come from `Assets/3DModelsElectrode`.
- Re-running the builder updates in-place with no duplicates; Undo/Redo works.
- Progress bar and final summary log appear; missing assets/warnings are listed clearly.
