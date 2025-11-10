# 12-EKG Project Assets Catalog and Usage Notes

This document catalogs key assets under `Assets/Art`, `Assets/Code`, and `Assets/YAML` and provides concise notes to help you use them correctly.

## Current Assets (Assets/3DModelsElectrode)

The previous `Assets/Art`, `Assets/Code`, and `Assets/YAML` folders have been removed. The active assets now live under `Assets/3DModelsElectrode`. The sections further below are legacy and kept for reference only.

- **[Models | FBX/OBJ]**
  - `Assets/3DModelsElectrode/EKG Patient.fbx`
  - `Assets/3DModelsElectrode/EKG Patient In Bed.fbx`
  - `Assets/3DModelsElectrode/EKG Machine Console.fbx`
  - `Assets/3DModelsElectrode/EKG Cable Splitter.fbx`
  - `Assets/3DModelsElectrode/EKG Pad With Back.fbx`
  - `Assets/3DModelsElectrode/EKG Backing Peeled.fbx`
  - `Assets/3DModelsElectrode/EKG Electrode.fbx`
  - `Assets/3DModelsElectrode/VGA Plug.fbx`
  - `Assets/3DModelsElectrode/Pillow.fbx`
  - `Assets/3DModelsElectrode/UniversalController.fbx` (XR sample controller model)
  - `Assets/3DModelsElectrode/BlinkVisual.fbx` (XR sample visual)
  - `Assets/3DModelsElectrode/PushButton.fbx` (XR sample push button)
  - `Assets/3DModelsElectrode/LeftHand.fbx`, `Assets/3DModelsElectrode/RightHand.fbx`
  - `Assets/3DModelsElectrode/Chair With Shirt.obj`

- **[Materials | MAT]**
  - `Assets/3DModelsElectrode/EKG Buttons TXT.mat`
  - `Assets/3DModelsElectrode/EKG Console TXT.mat`
  - `Assets/3DModelsElectrode/EKG Electrode Pad TXT.mat`
  - `Assets/3DModelsElectrode/EKG Electrode Backing TXT.mat`

- **[Reference Images | PNG]**
  - `Assets/3DModelsElectrode/EKG Normal.png`
  - `Assets/3DModelsElectrode/EKG Atrial Fibrillation.png`
  - `Assets/3DModelsElectrode/EKG Ventricular Tachycardia.png`
  - `Assets/3DModelsElectrode/EKG Premature Ventricular Contraction.png`
  - `Assets/3DModelsElectrode/EKG incorrect Placement.png`

Notes:
- Use these meshes for the patient, pads, leads hardware, machine console, and props. Apply the included materials; PNGs can be used for UI/waveform/reference visuals.
- If you would like the legacy sections below removed entirely, say the word and I will prune them.

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

- **[Skybox]**
  - `Skybox1.jpg`, `Skybox2.jpeg` … optional environment skybox imagery.

- **[Animation]**
  - `Assets/Art/Animation/EKG Controller.controller`
    - Animator Controller for EKG-related presentation; bind to relevant objects as needed.

