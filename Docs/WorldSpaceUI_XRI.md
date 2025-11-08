# World‑Space UI with XR Interaction Toolkit (from DemoScene)

This guide explains how the sample `DemoScene.unity` sets up world‑space UI, and how to reuse the same prefabs/components in your own scene.

## What the sample scene contains

- **XR Origin (XR Rig) prefab**
  - Path: `Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab`
  - Includes: `Main Camera`, `Left Controller`, `Right Controller`, locomotion components, and near/far interactors.
  - Has an `Input Action Manager` referencing the asset:
    - `Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/XRI Default Input Actions.inputactions`
- **XR Interaction Manager**
  - Scene object named `XR Interaction Manager` with `XRInteractionManager` component.
- **EventSystem + XR UI Input Module**
  - Scene object named `EventSystem` with components:
    - `EventSystem`
    - `XR UI Input Module` (wired to actions from the XRI Default Input Actions asset)
  - Mouse fallback is enabled alongside XR input in the sample.
- **World‑space UI Canvas**
  - A Canvas such as `Far Grab Interactable Info` configured as:
    - `Canvas` with `Render Mode: World Space`
    - `Event Camera`: the XR Origin’s `Main Camera`
    - `Canvas Scaler`: World preset (world‑space)
    - `Tracked Device Graphic Raycaster` (for XR pointers)
    - `Graphic Raycaster` (optional, mouse fallback)

## Asset and preset locations

- **XR Rig prefab**
  - `Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab`
- **Input Actions asset**
  - `Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/XRI Default Input Actions.inputactions`
- **XR UI Input Module preset (optional for quick wiring)**
  - `Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/Presets/XRI Default XR UI Input Module.preset`

## How to set this up in your scene

1. **Add XR Interaction Manager**
   - Create an empty GameObject named `XR Interaction Manager`.
   - Add the `XRInteractionManager` component.

2. **Add XR Origin (XR Rig)**
   - Drag the prefab into your scene: `.../Starter Assets/Prefabs/XR Origin (XR Rig).prefab`.
   - This already contains controllers, near/far interactors, locomotion, and an `Input Action Manager` that references the XRI default input actions asset.

3. **Add EventSystem with XR UI Input Module**
   - Create a GameObject: `EventSystem`.
   - Add components:
     - `EventSystem`
     - `XR UI Input Module`
   - Wire input actions:
     - Fastest: apply the preset `XRI Default XR UI Input Module.preset`, or
     - Manually assign the actions (Point, Left/Middle/Right Click, Scroll, Navigate, Submit, Cancel) from `XRI Default Input Actions.inputactions`.
   - Ensure both `Enable XR Input` and (optionally) `Enable Mouse Input` are on, matching the sample.

4. **Create a World‑Space UI Canvas**
   - Create a `Canvas` and set:
     - `Render Mode`: World Space
     - `Event Camera`: XR Origin → `Main Camera`
     - Add `Canvas Scaler` (World preset)
     - Add `Tracked Device Graphic Raycaster`
     - (Optional) Add `Graphic Raycaster` for mouse fallback
   - Place and scale in world (typical scale ~0.001–0.005). Position it in front of the player.
   - Add your UI (Panels, Text, Buttons, Toggles, Sliders, etc.).

## Notes for interaction

- **Pointers**
  - The XR rig’s near/far interactors can point and click UI via the `Tracked Device Graphic Raycaster`.
- **Distance and occlusion**
  - Keep the UI within the camera’s clipping range and not occluded. The raycaster’s blocking mask/occlusion settings can be adjusted if needed.
- **Input System**
  - The rig and EventSystem use the `XRI Default Input Actions`. Ensure the Input System package is enabled in Project Settings and the asset is present.

## Troubleshooting

- **No pointer/raycast on UI**
  - Canvas is not `World Space` or `Event Camera` not set to XR `Main Camera`.
  - Missing `Tracked Device Graphic Raycaster` on the Canvas.
  - `XR UI Input Module` not present or actions not assigned.
- **Clicks don’t register**
  - UI elements disabled or outside the Canvas bounds.
  - Interactor rays not enabled/active.
  - Physics/UI layers are blocking; check raycaster blocking masks.
- **Mouse works but XR doesn’t (or vice‑versa)**
  - Toggle `Enable XR Input` / `Enable Mouse Input` on the `XR UI Input Module`.
  - Confirm the actions on the module reference the XRI input actions asset.
