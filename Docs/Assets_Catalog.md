# 12-EKG Project Assets Catalog and Usage Notes

This document catalogs key assets under `Assets/Art`, `Assets/Code`, and `Assets/YAML` and provides concise notes to help you use them correctly.

## Assets/Art

- **[Geometry]**
  - `Assets/Art/Geometry/Chair With Shirt.obj`
    - Simple prop mesh (chair with shirt) for set dressing.
  - `Assets/Art/Geometry/EKG Machine/`
    - `EKG Backing Peeled.fbx`
      - Mesh for the peeled adhesive backing from an electrode pad.
      - Spawned by the prefab “Peeled Pad Backing”.
    - `EKG Cable Splitter.fbx`
      - Mesh for the cable splitter hardware.
    - `EKG Electrode.fbx`
      - Mesh for electrode head (used by “Lead root” prefab as child named “EKG Electrode”).
    - `EKG Machine Console.fbx`
      - Mesh for the main console housing + screen/buttons geometry.
    - `EKG Pad With Back.fbx`
      - Mesh for electrode pad with backing attached.
    - `VGA Plug.fbx`
      - Mesh for VGA plug component.
  - `Assets/Art/Geometry/EKG Patient/`
    - `EKG Patient.fbx`
      - Base human patient mesh.
    - `EKG Patient In Bed.fbx`
      - Patient mesh posed in hospital bed.
  - `Assets/Art/Geometry/Pillow.fbx`
    - Pillow prop mesh.

- **[Materials]**
  - `Assets/Art/Materials/`
    - `AMIS No Computer TXT.mat`
    - `Pillow TXT.mat`
    - `EKG Machine/` … materials for machine sub-meshes:
      - `EKG Buttons TXT.mat`, `EKG Console TXT.mat`, `EKG Screen TXT.mat`, `EKG Splitter TXT.mat`, `EKG VGA Plug TXT.mat`, `EKG VGA PortTXT.mat`, …
      - `EKG Electrodes/` … per-electrode label/color materials: `LA/LL/RA/RL/V1..V6`.
    - `EKG Patient/` … materials for body parts:
      - Examples: `EKG Body TXT.mat`, `EKG Head TXT.mat`, `EKG Arms TXT.mat`, `EKG Legs TXT.mat`, `Eyes`, `Eyelashes`, `Hair`, `Teeth`, `FingerNails`, `ToeNails`, clothing, soles, markers variants.
    - Usage: assign to corresponding patient or machine sub-meshes; most pair with texture sets in `Textures`.

- **[Textures]**
  - `Assets/Art/Textures/`
    - Top-level examples:
      - `Ambient Occlusion Map from Mesh AMiS_TXT1.png`
    - `EKG Machine/`
      - PBR texture sets for each sub-part, following naming like:
        - `*_AO.png` (ambient occlusion)
        - `*_AlbedoTransparency.png`
        - `*_Emission.png`
        - `*_MetallicSmoothness.png`
        - `*_Normal.png`
      - Coverage includes: Buttons, Cable Splitter, Machine, Pad, Screen, VGA Plug/Port, Electrode, etc.
      - `EKG Electrodes/`
        - Color-coded electrode labels: `Blue LA EKG.png`, `Red RA EKG.png`, `Purple RL EKG.png`, `Dark Green LL EKG.png`, and chest leads `V1..V6` (Red/Yellow/Green/Blue/Magenta/Orange).
        - Additional electrode PBR maps: `*_AO`, `*_MetallicSmoothness`, `*_Normal`, `*_Emission`.
    - `EKG Patient/`
      - Large set of body textures (albedo/normal/etc.) for patient materials listed above.
    - `Pillow/` … pillow texture maps.
    - `Waveforms/` … images used for UI/monitor visuals (EKG waveforms etc.).
    - Usage: Materials reference these; when creating new materials, match the suffix conventions for correct channels.

- **[Audio/Narration]**
  - `Assets/Art/Audio/Narration/` (voice prompts used by sequences/tutorial)
    - 1–19 indexed prompts and sub-steps:
      - `1Welcometoadvance.wav`, `1welcome12.wav`
      - `2letsfirst.wav`, `2yourtask.wav`
      - `3before.wav`, `3pleaselook.wav`
      - `4Goodnext.wav`, `4electrodes.wav`
      - `5Goodletsmove.wav`, `5youmaybegin.wav`
      - `6dontdrop.wav`, `6takealook.wav`
      - `7ifwemention.wav`, `7mustbeplaced.wav`
      - `8Notethatyour.wav`, `8wehavemarked.wav`
      - `9peel.wav`, `9useyourjoysticks.wav`
      - `10grip.wav`, `10pleasedontwalk.wav`
      - `11attachthelead.wav`, `11whenyoureready.wav`
      - `12limbbeforechest.wav`, `12thefun.wav`
      - `13wehavetoys.wav`, `13wrong.wav`
      - `14almostdone.wav`, `14objectscanrespond.wav`
      - `15dropthebean.wav`, `15switchon.wav`
      - `16ekgsetup.wav`, `16whoops.wav`
      - `17youhavesuccessfully.wav`
      - `18properplacement.wav`
      - `19exiting12.wav`
    - Usage: Trigger via Yarn dialogue or UnityEvents in ViewModels to guide user actions.

- **[Skybox]**
  - `Skybox1.jpg`, `Skybox2.jpeg` … optional environment skybox imagery.

- **[Animation]**
  - `Assets/Art/Animation/EKG Controller.controller`
    - Animator Controller for EKG-related presentation; bind to relevant objects as needed.

## Assets/Code

- **Put The Bunny In The Box/**
  - `BunnyInBoxIntroViewModel.cs`
    - Toggles overlay vs world-space canvases by platform (`FixedPancake/FPPancake` vs `XR`).
    - For FixedPancake, acquires `FixedCameraController` and sets camera range; Yarn command `Enter()` completes sequence.
  - `BunnyInBoxSequenceModel.cs`
    - `SerializableReactiveProperty<bool> bunnyInBox` to track when the bunny is placed; inherits `SequenceModel`.
  - `BunnyInBoxSequenceViewModel.cs`
    - Same platform toggles as intro; subscribes to `bunnyInBox` and invokes `OnBunnyInBox` once true.
    - Camera pose helpers; `Finish()` completes sequence.
  - `IntroSequenceModel.cs`
    - Empty `SequenceModel` placeholder for intro scene.

- **SL12/** (Electrode placement simulation)
  - `ElectrodeMetadata.cs`
    - MonoBehaviour holding `ElectrodesViewModel.LeadIndex leadID` on lead objects for identification.
  - `ElectrodeMount.cs`
    - MonoBehaviour with `leadID` and UnityEvents: `OnSticker`, `OnLead`, `OnWrongLead`.
    - `LinkSticker()` invokes `OnSticker(leadID)`.
    - `LinkLead(SimpleDetectable)` checks detected object’s `ElectrodeMetadata.leadID`; invokes `OnLead` if matching, else `OnWrongLead`.
  - `ElectrodesModel.cs`
    - Reactive booleans for each sticker and lead (LA, RA, LL, RL, V1–V6).
    - `BehaviorSubject<bool> StickersDone/LeadsDone`; computed by CombineLatest across all required flags in `LoadScene()`.
  - `ElectrodesViewModel.cs`
    - `LeadIndex` enum LA/RA/LL/RL/V1–V6.
    - Yarn hooks: `DoPrep`, `DoStickers`, `DoLeads`, `DoEKG`, `DoDone` via UnityEvents.
    - `SetSticker/SetLead` APIs set the model flags. `BuildMissingLeadString()` updates on-screen instructions.
    - Flow: Prep → Stickers → Leads → EKG → Done → `Complete()` (invokes model completion).

- **Splash Screen/**
  - `SplashScreenManager.cs`
    - In-Editor fields to pick scene assets; auto-caches scene names on `OnValidate`.
    - `EnterTutorial()` / `EnterSimulation()` load the selected scenes.

- **Template (Rename Me)/**
  - `TemplateSequenceModel.cs`
  - `TemplateSequenceViewModel.cs`
    - Scaffold for creating a new sequence; `SetPlatform` not implemented yet.

- **XR Tutorial/**
  - `XRIntroSequenceModel.cs`
    - Reactive `finishedDialogue` flag; base `LoadScene` override.
  - `XRIntroSequenceViewModel.cs`
    - Manages tutorial flow and look-at targets for arch/totem/controllers; Yarn commands to progress.
    - Uses `XRInputModalityManager` (from Runner’s platform avatar) to parent visual look targets to controllers.
    - Provides `ResetPlayer`, `ControllerLookSuccess`, `BridgeCrossed`, `Complete`, and instruction panel sequencing.

## Assets/YAML

- **Prefabs/**
  - `Electrode Sticker.prefab`
    - Composition:
      - Root `Electrode Sticker` (Rigidbody + XR Grab Interactable). Children include `EKG_Pad` mesh, `Adhesive backing` mesh, collisions, and detectable helpers.
      - `SpawnGrabber` + `SpawnOnGrab` pattern spawns `Peeled Pad Backing` on interaction and toggles visibility of backing/attachable objects.
      - XR Grab events enable/disable colliders for pick-up/placement.
    - Usage: Grabbable pad; place on `ElectrodeMount` to register sticker attachment events.
  - `ElectrodeMount.prefab`
    - Composition:
      - `ElectrodeMount` (Rigidbody), `ElectrodeMount` script with `leadID` and events.
      - `Sticker Attacher` and `Lead Attacher` helper objects (with detection/attacher scripts) and “Entry Hitbox” triggers.
      - XR Simple detectables for warning/entry; UnityEvents wired to `ElectrodesViewModel.AttachSticker/AttachLead/WrongLead` and MagicEd attach helpers.
    - Usage: Place on patient at target positions; passing correct lead/pad triggers model updates.
  - `Lead root.prefab`
    - Composition:
      - Root with `SplineContainer` + `SplineAnimate/SplineExtrude` for rendering lead cable; material `Tube`-style mesh via SplineExtrude.
      - Child `Electrode Lead` (Rigidbody + XR Grab Interactable) with `Grab Pivot` attach transform and `Lead head` detectable collider.
      - `LeadIdentifier`-style script on head stores `leadID`.
    - Usage: Grabbable lead end; when placed into `ElectrodeMount` lead socket, registers appropriate lead attachment and extrudes cable along spline.
  - `Peeled Pad Backing.prefab`
    - XR Grab Interactable + BoxCollider + Rigidbody; spawned when grabbing pad to simulate peeling backing; throwable.

- **Etc/**
  - `Slippery.physicMaterial`
    - Physics material for low friction surfaces.

- **Scenes/**
  - `Put The Bunny In The Box/`
    - `Start Scene (Bunny In Box).unity`, `PutTheBunnyInTheBox.unity`, `PutTheBunnyInTheBoxIntro.unity`
    - Yarn: `Put The Bunny In The Box.yarn`, `*.yarnproject`.
  - `SL12/`
    - `Electrode Placement.unity` (core EKG placement sim).
    - `Start Scene (SL12).unity`.
    - `Electrode Placement/` baked lighting (`LightingData.asset`, `Lightmap-*.png`, `ReflectionProbe-*.exr`) and Yarn (`SL12Script.yarn`, `SL12YarnProject.yarnproject`).
  - `XR Tutorial/`
    - `Start Scene (XR tutorial).unity`, `XR Intro.unity`.
    - GI settings (`IntroLighting.lighting`, `*.giparams`) and Yarn (`XR Tutorial.yarn`, `*.yarnproject`, CSV metadata).
  - `Template/`
    - `Template Sequence.unity`, `Start Scene (template).unity` and `*.scenetemplate` seeds.
  - `Splash.unity`
    - Entry scene for routing to tutorial/simulation via `SplashScreenManager`.

## Notes and Conventions

- **Lead naming**: `LA/RA/LL/RL/V1..V6` are used consistently across textures, materials, scripts, and prefabs.
- **PBR texture suffixes**: `_AO`, `_AlbedoTransparency`, `_Emission`, `_MetallicSmoothness`, `_Normal`.
- **XR interaction**: Grabbable electrodes/pads/leads use XRIT interactables and SimpleDetectables; mounts listen and forward to `ElectrodesViewModel`.
- **Dialogue**: Yarn files drive prompts; ViewModels contain `YarnCommand` methods to progress.

If you want this catalog split into multiple focused docs (e.g., Art-only, Code-only), say the word and I’ll generate them.
