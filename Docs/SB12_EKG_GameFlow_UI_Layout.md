# SB12 EKG Testing – Game Flow, UI Flow, and Prefab Placement (Oculus Quest 2)

This blueprint translates the storyboard into an actionable Unity/XR implementation using existing project assets and world-space UI. It defines flow, UI behavior, and a concrete scene layout with recommended transforms. Numbers are starting values to be refined during in-editor alignment.

- Target: Oculus Quest 2 (XR Interaction Toolkit)
- Project version: 6000.0.32f1
- XR Rig: Samples/XR Interaction Toolkit/Starter Assets/Prefabs/XR Origin (XR Rig).prefab
- Core Prefabs/Assets:
  - Electrode Sticker: `Assets/YAML/Prefabs/Electrode Sticker.prefab`
  - Electrode Mount: `Assets/YAML/Prefabs/ElectrodeMount.prefab`
  - Lead Root (wire): `Assets/YAML/Prefabs/Lead root.prefab`
  - Peeled Pad Backing: `Assets/YAML/Prefabs/Peeled Pad Backing.prefab` (spawned on grab by Sticker)
  - Patient: `Assets/Art/Geometry/EKG Patient/EKG Patient In Bed.fbx`
  - EKG Machine Console: `Assets/Art/Geometry/EKG Machine/EKG Machine Console.fbx`
  - Cable Splitter / VGA Plug: `Assets/Art/Geometry/EKG Machine/*`


## 1) Scene and Room Setup (Global)

Create an empty root `RoomRoot` at world origin and build the room with primitives for performance.

- **Floor**
  - Object: `Floor (Plane)`
  - Transform: pos (0, 0, 0), rot (0, 0, 0), scale (6, 1, 6)
  - Material: neutral, non-reflective; friction normal

- **Walls** (four quads or scaled cubes)
  - `Wall_North`: pos (0, 1.5, 3), rot (0, 180, 0), scale (6, 3, 0.1)
  - `Wall_South`: pos (0, 1.5, -3), rot (0, 0, 0), scale (6, 3, 0.1)
  - `Wall_East`: pos (3, 1.5, 0), rot (0, -90, 0), scale (6, 3, 0.1)
  - `Wall_West`: pos (-3, 1.5, 0), rot (0, 90, 0), scale (6, 3, 0.1)
  - Lighting: soft warm; bake later

- **Hospital Bed (primitive mockup)**
  - `Bed_Base (Cube)`: pos (0, 0.35, 0), scale (2.0, 0.3, 0.8)
  - `Bed_Mattress (Cube)`: pos (0, 0.7, 0), scale (2.0, 0.12, 0.8)
  - `Headrest (Cube)`: pos (0.8, 0.78, 0), rot (0, 0, 10), scale (0.35, 0.08, 0.8)

- **Patient (FBX)**
  - Parent under `Bed_Mattress` to keep relative alignment
  - pos (0.0, 0.82, 0.0), rot (0, -90, 0) so head faces +X, torso up
  - Scale 1.0
  - Add empty `PatientAnchor` (at sternum): pos relative to patient chest (0.00, 0.10, 0.00)

- **EKG Machine Cart (primitive + mesh)**
  - `Cart_Top (Cube)`: pos (1.2, 0.92, 0.45), scale (0.9, 0.06, 0.6)
  - `Cart_Legs (Cubes)`: four slim cubes to ground; optional wheels
  - `EKG Machine Console (FBX)` under Cart_Top: pos (1.2, 0.98, 0.45), rot (0, -30, 0)
  - Screen initially off (material emission disabled)

- **Electrode Tray (primitive)**
  - `Tray (Cube)`: pos (1.2, 0.9, 0.0), scale (0.6, 0.03, 0.4)
  - Holds 10 `Electrode Sticker` prefabs

- **Lead Wire Rack (primitive)**
  - `Rack_Bar (Cylinder)`: pos (1.2, 1.05, -0.4), rot (0, 0, 90), scale (0.7, 0.01, 0.01)
  - Holds 10 `Lead root` prefabs (coiled near bar)

- **XR Rig**
  - `XR Origin (XR Rig)`: pos (-1.2, 0, -1.2), facing towards bed
  - EventSystem + XR UI Input Module present


## 2) UI Flow (World-Space)

All panels are world-space canvases with `Tracked Device Graphic Raycaster` and `XR UI Input Module`.

- **Intro Panel** (screenshot 2 reference)
  - Parent: `RoomRoot`
  - pos (-0.2, 1.5, -1.0), rot (0, 15, 0), size ~ (1.1 m x 0.6 m)
  - Text: title “EKG Testing: Electrode Placement” and task description
  - Button: `Begin Exam` (triggers teleport and starts Segment 2)

- **Step Panel (floating)**
  - Always face user; small progress indicator (Intro → Prep → Limb → Chest → Connect → Verify → Complete)
  - pos (0.2, 1.55, 0.2) relative to `PatientAnchor`, rot billboard to camera

- **Context Tooltips**
  - Machine, Tray, Rack, and marked body zones get gaze-based tooltips with soft glow

- **Alerts and Cues**
  - Non-blocking toasts near hands or target: Peel reminder, Order reminder, Connection reminder
  - Success cue: subtle chime + snap animation

## 2.5) UI Slide Sequence (in-order, one-by-one)

Use a single world-space panel that advances per-press and updates its copy. Each slide lists the on-screen text and what happens when the user presses Continue.

- **[Slide 1 — Task Intro]**
  - Text: "Your task is to correctly place electrodes for a 10‑lead EKG on your patient. When you are ready, press 'Continue' to proceed."
  - Advance: Continue.
  - OnAdvance: none.

- **[Slide 2 — Skin Prep]**
  - Text: "Before starting, ensure the patient's skin is clean and dry. Here, the skin has already been prepared."
  - Advance: Continue.
  - OnAdvance: none.

- **[Slide 3 — Adhesion Tip]**
  - Text: "Electrodes adhere better on a prepared surface."
  - Advance: Continue.
  - OnAdvance: none.

- **[Slide 4 — Equipment Appears]**
  - Text: "When you press Continue, the EKG machine and electrodes will appear on the bedside and you may begin."
  - Advance: Continue.
  - OnAdvance: Activate EKG cart + tray + wire rack; show Step Panel; enable limb mounts and show chest markers.

- **[Slide 5 — Markers + Peel]**
  - Text A: "We have marked these spots on the patient's body with labeled, color‑coded markers."
  - Text B (footer/subtext): "Peel the back off the pads and apply them to the patient."
  - Advance: Continue.
  - OnAdvance: Enable grabbing of Electrode Stickers; `Peeled Pad Backing` spawns on first grab.

- **[Slide 6 — Wire Matching]**
  - Text: "Attach the lead wire that matches the spot's color and label."
  - Advance: Continue.
  - OnAdvance: Enable `Lead root` wires on rack.

- **[Slide 7 — Order Hint]**
  - Text A: "Begin with the limb leads before moving to the chest leads."
  - Text B (footer): "Attach the correct lead to each electrode pad."
  - Advance: Continue.
  - OnAdvance: none (order suggested, not enforced).

- **[Slide 8 — Verify Prep]**
  - Condition to show: All 10 electrode pads placed (stickers done).
  - Text: "You're almost done! Now that all electrodes are placed, let's verify the connections."
  - Advance: Continue.
  - OnAdvance: none.

- **[Slide 9 — Power On]**
  - Condition to enable: StickersDone && LeadsDone true.
  - Text: "Switch on the EKG machine to check the readings."
  - Advance: Continue.
  - OnAdvance: Glow/enable the machine power button; pressing it shows waveforms.

- **[Slide 10 — Setup Complete]**
  - Text: "EKG setup complete!"
  - Advance: Continue.
  - OnAdvance: none.

- **[Slide 11 — Success Statement]**
  - Text: "You have successfully placed the electrodes for a 10‑lead EKG."
  - Advance: Continue.
  - OnAdvance: none.

- **[Slide 12 — Rationale]**
  - Text: "Proper placement ensures accurate readings, which are crucial for patient diagnosis."
  - Advance: Continue.
  - OnAdvance: none.

- **[Slide 13 — Exit]**
  - Text: "Exiting Simulation."
  - Advance: Continue.
  - OnAdvance: Load exit scene or return to menu.

Implementation note: the same panel can update a progress indicator (• between separators) to match the screenshots; use `XR UI Input Module` for controller presses.


## 3) Game Flow (Segments and Gating)

- **Segment 1 – Introduction**
  - Show Intro Panel. Input allowed: Look, UI Press, Teleport disabled
  - On `Begin Exam`: fade/teleport to bedside: XR Origin → pos (0.2, 0, 0.8), rot facing patient and cart
  - Narrative: skin prepped, proceed

- **Segment 2 – Limb Leads (4)**
  - Enable tray stickers and limb mounts
  - Order hint: place limb leads before chest leads; warn but don’t block
  - Peel → Place: grabbing `Electrode Sticker` spawns `Peeled Pad Backing`; if placed without peeling, show “Peel before placing” toast
  - On sticker placed on correct limb mount: `ElectrodesViewModel.SetSticker(LeadIndex)`

- **Segment 3 – Chest Leads (6)**
  - Enable chest mounts V1–V6 (see placement below)
  - Guide outline when close to correct mount; snap on drop
  - On sticker placed: update model via `SetSticker`

- **Segment 4 – Connect Wires (10)**
  - Enable `Lead root` wires on rack
  - Bring wire to matching electrode; on correct head proximity to mount, connection point lights; require firm push
  - On connect: `ElectrodesViewModel.SetLead(LeadIndex)`
  - Warnings: connection-before-placement and loose connection toasts

- **Segment 5 – Verification**
  - Power button remains inactive until all 10 stickers + wires true (model `StickersDone && LeadsDone`)
  - On complete, power button glows; pressing shows EKG waveforms
  - If any mismatch, distort waveforms and blink incorrect elements; allow 2 correction attempts, then auto-correct and continue

- **Segment 6 – Completion**
  - Dim environment, show summary panel, optional exit button


## 4) Prefab Placement Plan (aligned to screenshots)

Use `PatientAnchor` (sternum-level empty) as reference for chest mounting coordinates. Axis convention: +X patient’s right, +Y up, +Z towards patient’s feet.

- **Electrode Mounts – Limb Leads (relative to PatientAnchor)**
  - RA (Right Arm) – White: pos (+0.55, +0.00, +0.15), rot (0, 0, 0)
  - LA (Left Arm) – Black: pos (-0.55, +0.00, +0.15), rot (0, 0, 0)
  - RL (Right Leg) – Green: pos (+0.35, -0.55, +0.25), rot (0, 0, 0)
  - LL (Left Leg) – Red: pos (-0.35, -0.55, +0.25), rot (0, 0, 0)
  - Place four instances of `ElectrodeMount.prefab` with `leadID` set accordingly

- **Electrode Mounts – Chest Leads V1–V6 (relative to PatientAnchor)**
  - V1 (Red): 4th ICS, right sternal border → pos (+0.05, +0.05, -0.05)
  - V2 (Yellow): 4th ICS, left sternal border → pos (-0.05, +0.05, -0.05)
  - V3 (Green): midway V2–V4 → pos (-0.10, +0.06, -0.03)
  - V4 (Blue): 5th ICS, midclavicular line → pos (-0.16, +0.06, 0.00)
  - V5 (Orange): anterior axillary line, level with V4 → pos (-0.22, +0.07, 0.00)
  - V6 (Purple): midaxillary line, level with V4/V5 → pos (-0.28, +0.07, 0.00)
  - Six instances of `ElectrodeMount.prefab` with `leadID` V1–V6
  - Visual markers: apply colored decal/marker sprites at same positions (screenshot 1)

- **Electrode Stickers on Tray** (10 instances)
  - Parent: `Tray`
  - Grid layout starting pos (1.12, 0.93, 0.02), spacing (0.06, 0, 0.06)
  - Random small rotation jitter for realism

- **Lead Wires on Rack** (10 instances of `Lead root.prefab`)
  - Parent: `Rack_Bar`
  - Staggered positions along bar from z ∈ [-0.60 .. -0.20], y ~ 1.05, x ~ 1.2; alternating small rotations
  - Set `leadID` on `Lead head` to RA/LA/RL/LL/V1..V6; material colors match electrode labels

- **EKG Machine Console**
  - Parent: `Cart_Top`
  - pos (1.2, 0.98, 0.45), rot (0, -30, 0)
  - Power button mesh/collider at console front-right; add glow when ready

- **Teleport Anchors**
  - `TA_Entry`: (-1.2, 0, -1.2)
  - `TA_Bedside`: (0.2, 0, 0.8)
  - `TA_Cart`: (1.4, 0, 0.4)


## 5) Interaction and Wiring (existing scripts)

- `Electrode Sticker.prefab`
  - On Grab: spawns `Peeled Pad Backing`, hides adhesive backing, enables attachable collider
  - On First Select Entered: enable main collider for placement

- `ElectrodeMount.prefab`
  - Set `leadID` per mount
  - Events:
    - `OnSticker(leadID)` → `ElectrodesViewModel.AttachSticker(leadID)`
    - `OnLead(leadID)` → `ElectrodesViewModel.AttachLead(leadID)`
    - `OnWrongLead(leadID)` → show warning toast

- `Lead root.prefab`
  - Uses Spline Extrude to render cable; `Lead head` has collider + `ElectrodeMetadata.leadID`
  - On attach to matching mount: confirm click + light

- `ElectrodesViewModel`
  - `SetSticker/SetLead` update `ElectrodesModel`
  - When `StickersDone && LeadsDone` → enable machine power button glow

- Yarn/Audio
  - Use narration wavs to step users through Intro → Limb → Chest → Connect → Verify → Complete


## 6) UI Copy (concise)

- Intro: “Your task is to correctly place electrodes for a 10-lead EKG. Press ‘Begin Exam’ to proceed.”
- Peel Reminder: “Peel off the backing before placing the electrode.”
- Order Reminder: “Place the limb electrodes before the chest electrodes.”
- Connection Reminder: “Place the electrode before connecting the lead wire.”
- Firm Press: “Press the wire firmly until you feel and hear a click.”
- Verify Prompt: “Switch on the EKG machine once all electrodes and wires are connected.”
- Correction 1/2: placement or connection-specific hints
- Success: “EKG setup complete! Proper placement ensures accurate readings.”


## 7) Screenshot Alignment Notes

- Screenshot 1 (patient markers): ensure V1–V6 markers appear in a vertical sweep across the left chest as positioned above; add subtle emissive glow
- Screenshot 2 (UI): Intro Panel framed with blue outline, centered within user’s view at ~1.2–1.5 m distance
- Screenshot 3 (machine + tray): place cart to patient’s right side; tray with stickers in front, wires hanging off rack to the left of console


## 8) Implementation Checklist

- **Room + Primitives**: floor, 4 walls, bed, cart, tray, rack
- **Place Patient FBX** and add `PatientAnchor`
- **Mounts**: 4 limb + 6 chest mounts with `leadID`
- **Stickers**: 10 on tray; `Peeled Pad Backing` auto-spawn on grab
- **Wires**: 10 on rack with matching `leadID`
- **UI**: Intro Panel, Step Panel, toasts/alerts; XR UI Input Module configured
- **Verification**: power button activation only when model complete; waveforms on
- **Audio**: narration wavs mapped to segment transitions


## 9) Appendix: Exact Asset Paths

- Prefabs: `Assets/YAML/Prefabs/`
  - Electrode Sticker.prefab
  - ElectrodeMount.prefab
  - Lead root.prefab
  - Peeled Pad Backing.prefab
- Meshes: `Assets/Art/Geometry/EKG Machine/*`, `Assets/Art/Geometry/EKG Patient/*`
- Audio: `Assets/Art/Audio/Narration/*`
- Textures/Materials: `Assets/Art/Textures/*`, `Assets/Art/Materials/*`


Notes: Positions are recommended starting values based on the storyboard and screenshots and should be fine-tuned to the actual patient mesh proportions and bed/cart dimensions in-scene.
