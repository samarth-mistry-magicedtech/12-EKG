#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.UI;

namespace SB12.Editor
{
    public static class SB12SceneBuilder
    {
        private const string MENU_PATH = "Tools/SB12/Build Scene (One-Click Setup)";
        private const string ASSETS_FOLDER = "Assets/3DModelsElectrode";
        
        private static Dictionary<string, GameObject> cachedObjects = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();
        private static int createdCount, updatedCount, warningCount;
        private static List<string> missingAssets = new List<string>();
        
        [MenuItem(MENU_PATH)]
        public static void BuildScene()
        {
            if (EditorUtility.DisplayDialog("SB12 Scene Builder", 
                "This will create/update the SB12 EKG training scene.\n\nIt will:\n• Check/install XR packages\n• Create room and equipment\n• Set up XR interaction\n• Generate UI slides\n\nProceed?", 
                "Build Scene", "Cancel"))
            {
                ExecuteBuild();
            }
        }
        
        private static void ExecuteBuild()
        {
            cachedObjects.Clear();
            createdCount = updatedCount = warningCount = 0;
            missingAssets.Clear();
            
            Undo.RegisterCompleteObjectUndo(new GameObject(), "SB12 Build");
            
            try
            {
                EditorUtility.DisplayProgressBar("SB12 Builder", "Validating assets...", 0.1f);
                ValidateAssets();
                
                GameObject root = FindOrCreateRoot();
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Setting up XR...", 0.2f);
                SetupXR(root);
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating environment...", 0.3f);
                CreateEnvironment(root);
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating bed and patient...", 0.4f);
                CreateBedAndPatient(root);
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating cart and equipment...", 0.5f);
                CreateCart(root);
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating tray table...", 0.55f);
                CreateTrayTable(root);
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating mount points...", 0.6f);
                CreateMountPoints(root);
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating electrode pads...", 0.7f);
                CreatePads(root);
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating UI...", 0.8f);
                CreateUI(root);
                EditorUtility.DisplayProgressBar("SB12 Builder", "Applying materials...", 0.85f);
                ApplyThemeMaterials(root);
                EditorUtility.DisplayProgressBar("SB12 Builder", "Generating common materials...", 0.87f);
                GenerateRemapMaterials();
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Generating scripts...", 0.9f);
                GenerateRuntimeScripts();
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Saving scene...", 1.0f);
                SaveScene();
                
                EditorUtility.ClearProgressBar();
                ShowBuildReport();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"Build failed: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private static void ValidateAssets()
        {
            string[] assetPaths = {
                $"{ASSETS_FOLDER}/EKG Patient In Bed.fbx",
                $"{ASSETS_FOLDER}/EKG Machine Console.fbx",
                $"{ASSETS_FOLDER}/EKG Pad With Back.fbx"
            };
            
            foreach (var path in assetPaths)
            {
                if (!File.Exists(path))
                {
                    missingAssets.Add(Path.GetFileName(path));
                    warningCount++;
                }
                else
                {
                    FileInfo info = new FileInfo(path);
                    if (info.Length < 10000)
                    {
                        Debug.LogWarning($"Asset {path} seems to be LFS pointer. Run 'git lfs pull'");
                        warningCount++;
                    }
                }
            }
        }
        
        private static GameObject FindOrCreateRoot()
        {
            GameObject root = GameObject.Find("RoomRoot");
            if (root == null)
            {
                root = new GameObject("RoomRoot");
                createdCount++;
            }
            return root;
        }
        
        private static Transform FindOrCreateChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent);
                createdCount++;
            }
            return child;
        }
        
        private static void SetupXR(GameObject root)
        {
            Transform xrParent = FindOrCreateChild(root.transform, "XR");
            
            // XR Interaction Manager
            Transform manager = FindOrCreateChild(xrParent, "XR Interaction Manager");
            if (!manager.GetComponent<XRInteractionManager>())
            {
                manager.gameObject.AddComponent<XRInteractionManager>();
            }
            
            // EventSystem
            Transform eventSystem = FindOrCreateChild(xrParent, "EventSystem");
            if (!eventSystem.GetComponent<EventSystem>())
            {
                eventSystem.gameObject.AddComponent<EventSystem>();
                eventSystem.gameObject.AddComponent<XRUIInputModule>();
            }
            
            // XR Origin
            Transform xrOrigin = xrParent.Find("XR Origin (XR Rig)");
            if (xrOrigin == null)
            {
                string prefabPath = "Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (prefab != null)
                {
                    GameObject origin = (GameObject)PrefabUtility.InstantiatePrefab(prefab, xrParent);
                    origin.transform.position = new Vector3(-1.2f, 0, -1.2f);
                    origin.transform.rotation = Quaternion.Euler(0, 45, 0);
                }
                else
                {
                    Debug.LogWarning("XR Origin prefab not found. Import XR Interaction Toolkit samples.");
                    warningCount++;
                }
            }
        }
        
        private static void CreateEnvironment(GameObject root)
        {
            Transform env = FindOrCreateChild(root.transform, "Environment");
            Transform lighting = FindOrCreateChild(root.transform, "Lighting");
            
            // Floor
            Transform floor = CreatePrimitive(env, "Floor", PrimitiveType.Plane, Vector3.zero, Quaternion.identity, new Vector3(6, 1, 6));
            ApplyColor(floor, "mat_floor", new Color32(180, 180, 180, 255));
            
            // Walls
            Transform wN = CreatePrimitive(env, "Wall_North", PrimitiveType.Cube, new Vector3(0, 1.5f, 3), Quaternion.Euler(0, 180, 0), new Vector3(6, 3, 0.1f));
            Transform wS = CreatePrimitive(env, "Wall_South", PrimitiveType.Cube, new Vector3(0, 1.5f, -3), Quaternion.identity, new Vector3(6, 3, 0.1f));
            Transform wE = CreatePrimitive(env, "Wall_East", PrimitiveType.Cube, new Vector3(3, 1.5f, 0), Quaternion.Euler(0, -90, 0), new Vector3(6, 3, 0.1f));
            Transform wW = CreatePrimitive(env, "Wall_West", PrimitiveType.Cube, new Vector3(-3, 1.5f, 0), Quaternion.Euler(0, 90, 0), new Vector3(6, 3, 0.1f));
            ApplyColor(wN, "mat_wall", new Color32(220, 220, 220, 255));
            ApplyColor(wS, "mat_wall", new Color32(220, 220, 220, 255));
            ApplyColor(wE, "mat_wall", new Color32(220, 220, 220, 255));
            ApplyColor(wW, "mat_wall", new Color32(220, 220, 220, 255));
            
            // Lights
            CreateLight(lighting, "Directional Light", LightType.Directional, Vector3.zero, Quaternion.Euler(50, -30, 0));
            CreateLight(lighting, "Point Light (Bed)", LightType.Point, new Vector3(0, 2, 0), Quaternion.identity);
            CreateLight(lighting, "Point Light (Cart)", LightType.Point, new Vector3(1.2f, 2, 0.45f), Quaternion.identity);
        }
        
        private static void CreateBedAndPatient(GameObject root)
        {
            Transform bed = FindOrCreateChild(root.transform, "Bed");
            
            // Patient
            Transform patient = bed.Find("Patient");
            if (patient == null)
            {
                GameObject patientFBX = AssetDatabase.LoadAssetAtPath<GameObject>($"{ASSETS_FOLDER}/EKG Patient In Bed.fbx");
                if (patientFBX != null)
                {
                    patient = ((GameObject)PrefabUtility.InstantiatePrefab(patientFBX, bed)).transform;
                    patient.name = "Patient";
                    patient.position = new Vector3(0, 0f, 0);
                    patient.rotation = Quaternion.Euler(0, -90, 0);
                }
                else
                {
                    patient = CreatePrimitive(bed, "Patient", PrimitiveType.Capsule, new Vector3(0, 0.85f, 0), Quaternion.Euler(90, 0, 0), new Vector3(0.4f, 0.8f, 0.4f));
                }
            }
            
            Transform anchor = FindOrCreateChild(patient, "PatientAnchor");
            anchor.localPosition = new Vector3(0, 0.1f, 0);
        }
        
        private static void CreateCart(GameObject root)
        {
            Transform cart = FindOrCreateChild(root.transform, "Cart");
            
            Transform cartTop = CreatePrimitive(cart, "Cart_Top", PrimitiveType.Cube, new Vector3(1.2f, 0.92f, 0.45f), Quaternion.identity, new Vector3(0.9f, 0.06f, 0.6f));
            ApplyColor(cartTop, "mat_cart", new Color32(120, 120, 120, 255));
            CreatePrimitive(cart, "Cart_Leg_FL", PrimitiveType.Cube, new Vector3(1.55f, 0.45f, 0.15f), Quaternion.identity, new Vector3(0.05f, 0.9f, 0.05f));
            CreatePrimitive(cart, "Cart_Leg_FR", PrimitiveType.Cube, new Vector3(1.55f, 0.45f, 0.75f), Quaternion.identity, new Vector3(0.05f, 0.9f, 0.05f));
            CreatePrimitive(cart, "Cart_Leg_BL", PrimitiveType.Cube, new Vector3(0.85f, 0.45f, 0.15f), Quaternion.identity, new Vector3(0.05f, 0.9f, 0.05f));
            CreatePrimitive(cart, "Cart_Leg_BR", PrimitiveType.Cube, new Vector3(0.85f, 0.45f, 0.75f), Quaternion.identity, new Vector3(0.05f, 0.9f, 0.05f));
            
            // Machine
            Transform machine = cart.Find("MachineConsole");
            if (machine == null)
            {
                GameObject machineFBX = AssetDatabase.LoadAssetAtPath<GameObject>($"{ASSETS_FOLDER}/EKG Machine Console.fbx");
                if (machineFBX != null)
                {
                    machine = ((GameObject)PrefabUtility.InstantiatePrefab(machineFBX, cart)).transform;
                    machine.name = "MachineConsole";
                    machine.position = new Vector3(1.2f, 0.98f, 0.45f);
                    machine.rotation = Quaternion.Euler(0, -30, 0);
                }
                else
                {
                    machine = CreatePrimitive(cart, "MachineConsole", PrimitiveType.Cube, new Vector3(1.2f, 1.1f, 0.45f), Quaternion.Euler(0, -30, 0), new Vector3(0.4f, 0.3f, 0.3f));
                }
            }
            
            // Power button
            Transform powerBtn = CreatePrimitive(cart, "PowerButton", PrimitiveType.Cube, new Vector3(1.35f, 1.05f, 0.35f), Quaternion.identity, new Vector3(0.05f, 0.05f, 0.02f));
            powerBtn.gameObject.AddComponent<BoxCollider>();
            powerBtn.gameObject.AddComponent<XRSimpleInteractable>();
            ApplyColor(powerBtn, "mat_power", new Color32(200, 60, 60, 255));
            
            // Rack and rack bar intentionally omitted per request
            Transform existingRack = cart.Find("Rack");
            if (existingRack != null)
            {
                UnityEngine.Object.DestroyImmediate(existingRack.gameObject);
            }
        }
        
        private static void CreateTrayTable(GameObject root)
        {
            Transform existing = GameObject.Find("RoomRoot/TrayTable")?.transform;
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }
            Transform tableRoot = new GameObject("TrayTable").transform;
            tableRoot.SetParent(root.transform);
            createdCount++;

            Vector3 tableSize = new Vector3(0.9f, 0.06f, 0.6f);
            float legHeight = 0.8f;
            Vector3 legSize = new Vector3(0.05f, legHeight, 0.05f);
            Vector3 traySize = new Vector3(0.7f, 0.03f, 0.5f);

            Vector3[] candidates = new Vector3[]
            {
                new Vector3(-1.2f, 0f, 0.9f),
                new Vector3(-1.2f, 0f, -0.9f),
                new Vector3(0f, 0f, 1.2f),
                new Vector3(0f, 0f, -1.2f),
                new Vector3(1.2f, 0f, -1.2f),
            };

            float totalHeight = legHeight + tableSize.y + traySize.y;
            Vector3 halfExtents = new Vector3(Mathf.Max(tableSize.x, traySize.x) * 0.5f + 0.05f, totalHeight * 0.5f, Mathf.Max(tableSize.z, traySize.z) * 0.5f + 0.05f);

            Vector3 chosenXZ = candidates[0];
            foreach (var c in candidates)
            {
                Vector3 center = new Vector3(c.x, halfExtents.y, c.z);
                var hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity);
                bool clear = true;
                for (int i = 0; i < hits.Length; i++)
                {
                    var h = hits[i];
                    if (h == null) continue;
                    string n = h.name;
                    if (n == "Floor") continue;
                    clear = false;
                    break;
                }
                if (clear)
                {
                    chosenXZ = c;
                    break;
                }
            }

            float topCenterY = legHeight + tableSize.y * 0.5f;
            Vector3 topPos = new Vector3(chosenXZ.x, topCenterY, chosenXZ.z);
            Transform top = CreatePrimitive(tableRoot, "Table_Top", PrimitiveType.Cube, topPos, Quaternion.identity, tableSize);
            ApplyColor(top, "mat_traytable_top", new Color32(150, 150, 150, 255));

            float legY = legHeight * 0.5f;
            float offX = tableSize.x * 0.5f - legSize.x * 0.5f;
            float offZ = tableSize.z * 0.5f - legSize.z * 0.5f;
            CreatePrimitive(tableRoot, "Table_Leg_FL", PrimitiveType.Cube, new Vector3(topPos.x + offX, legY, topPos.z + offZ), Quaternion.identity, legSize);
            CreatePrimitive(tableRoot, "Table_Leg_FR", PrimitiveType.Cube, new Vector3(topPos.x + offX, legY, topPos.z - offZ), Quaternion.identity, legSize);
            CreatePrimitive(tableRoot, "Table_Leg_BL", PrimitiveType.Cube, new Vector3(topPos.x - offX, legY, topPos.z + offZ), Quaternion.identity, legSize);
            CreatePrimitive(tableRoot, "Table_Leg_BR", PrimitiveType.Cube, new Vector3(topPos.x - offX, legY, topPos.z - offZ), Quaternion.identity, legSize);

            float trayCenterY = topCenterY + tableSize.y * 0.5f + traySize.y * 0.5f;
            Transform tray = CreatePrimitive(tableRoot, "Tray", PrimitiveType.Cube, new Vector3(topPos.x, trayCenterY, topPos.z), Quaternion.identity, traySize);
            ApplyColor(tray, "mat_tray", new Color32(60, 60, 60, 255));
            if (tray.GetComponent<Collider>() == null) tray.gameObject.AddComponent<BoxCollider>();
        }
        
        private static void CreateMountPoints(GameObject root)
        {
            GameObject patient = GameObject.Find("RoomRoot/Bed/Patient");
            if (patient == null) return;
            
            Transform anchor = patient.transform.Find("PatientAnchor");
            if (anchor == null) return;
            
            Transform mounts = FindOrCreateChild(anchor, "Mounts");
            
            string[] mountNames = { "Mount_RA", "Mount_LA", "Mount_RL", "Mount_LL", "Mount_V1", "Mount_V2", "Mount_V3", "Mount_V4", "Mount_V5", "Mount_V6" };
            Vector3[] positions = {
                new Vector3(0.55f, 0, 0.15f), new Vector3(-0.55f, 0, 0.15f),
                new Vector3(0.35f, -0.55f, 0.25f), new Vector3(-0.35f, -0.55f, 0.25f),
                new Vector3(0.05f, 0.05f, -0.05f), new Vector3(-0.05f, 0.05f, -0.05f),
                new Vector3(-0.10f, 0.06f, -0.03f), new Vector3(-0.16f, 0.06f, 0),
                new Vector3(-0.22f, 0.07f, 0), new Vector3(-0.28f, 0.07f, 0)
            };
            
            for (int i = 0; i < mountNames.Length; i++)
            {
                Transform mount = FindOrCreateChild(mounts, mountNames[i]);
                mount.localPosition = positions[i];

                // Ensure collider exists and is configured safely
                SphereCollider col = mount.GetComponent<SphereCollider>();
                if (col == null)
                {
                    try
                    {
                        col = mount.gameObject.AddComponent<SphereCollider>();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed adding SphereCollider to {mount.name}: {ex.Message}");
                    }
                }
                if (col != null)
                {
                    col.isTrigger = true;
                    col.radius = 0.03f;
                }

                // Tagging skipped (project may not define a "Mount" tag). Matching is done by name.
                
                Transform marker = mount.Find("Marker");
                if (marker == null)
                {
                    GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.name = "Marker";
                    quad.transform.SetParent(mount);
                    quad.transform.localPosition = Vector3.zero;
                    quad.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                    var rend = quad.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        var mat = rend.sharedMaterial; // avoid instantiating materials in edit mode
                        if (mat == null)
                        {
                            mat = new Material(Shader.Find("Standard"));
                            rend.sharedMaterial = mat;
                        }
                        mat.color = mountNames[i].Contains("V") ? Color.green : Color.yellow;
                    }
                }
            }
        }
        
        private static void CreatePads(GameObject root)
        {
            Transform tray = GameObject.Find("RoomRoot/TrayTable/Tray")?.transform;
            if (tray == null) 
            {
                Debug.LogWarning("[SB12] Tray not found, trying fallback location...");
                tray = GameObject.Find("RoomRoot/Cart/Tray")?.transform;
            }
            if (tray == null) return;
            
            Transform padsParent = FindOrCreateChild(tray, "Pads");
            // Ensure pads are positioned relative to the tray, not world origin
            padsParent.localPosition = Vector3.zero;
            padsParent.localRotation = Quaternion.identity;
            padsParent.localScale = Vector3.one;
            GameObject padPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ASSETS_FOLDER}/EKG Pad With Back.fbx");
            
            for (int i = 0; i < 10; i++)
            {
                string padName = $"Pad_{(i + 1):D2}";
                Transform pad = padsParent.Find(padName);
                GameObject padObj = null;
                if (pad == null)
                {
                    if (padPrefab != null)
                    {
                        padObj = (GameObject)PrefabUtility.InstantiatePrefab(padPrefab, padsParent);
                    }
                    else
                    {
                        padObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        padObj.transform.localScale = new Vector3(0.04f, 0.008f, 0.04f);
                        // Apply white color to make pads visible on dark tray
                        ApplyColor(padObj.transform, "mat_pad", new Color32(240, 240, 240, 255));
                    }
                    padObj.name = padName;
                    pad = padObj.transform;
                    pad.SetParent(padsParent, false);

                    // Add physics components when creating
                    Rigidbody rb = padObj.AddComponent<Rigidbody>();
                    rb.mass = 0.01f; // Light weight for realistic feel
                    rb.useGravity = false; // Disable gravity initially to prevent falling
                    rb.drag = 2f; // Add drag for more controlled movement
                    rb.angularDrag = 5f; // Prevent excessive spinning

                    // Use MeshCollider for accurate collision detection matching pad shape
                    MeshCollider meshCol = padObj.GetComponent<MeshCollider>();
                    if (meshCol == null) meshCol = padObj.AddComponent<MeshCollider>();
                    meshCol.convex = true; // Required for Rigidbody interaction
                    meshCol.isTrigger = false;

                    // XR Grab setup with gravity control
                    XRGrabInteractable grab = padObj.AddComponent<XRGrabInteractable>();
                    grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                    grab.throwOnDetach = false; // Disable throwing to prevent pads flying away

                    // Add script to enable gravity only when grabbed and released
                    var padScript = padObj.AddComponent<PadGravityController>();
                }
                else
                {
                    // Ensure correct parent and transform basis even for existing pads
                    pad.SetParent(padsParent, false);
                    padObj = pad.gameObject;
                }

                // Arrange pads in 2 rows of 5 on the tray for both existing and newly created
                int row = i / 5;
                int col = i % 5;
                float startX = -0.28f, spacingX = 0.14f;
                float startZ = -0.18f, spacingZ = 0.18f;
                pad.localPosition = new Vector3(startX + col * spacingX, 0f, startZ + row * spacingZ);
                pad.localRotation = Quaternion.identity;
                // For safety, keep scale as-is from prefab but ensure uniform if primitive
                if (padPrefab == null)
                {
                    pad.localScale = new Vector3(0.04f, 0.008f, 0.04f);
                }
                PlacePadOnTraySurface(pad, tray, 0.001f);
            }
        }

        private static void PlacePadOnTraySurface(Transform pad, Transform tray, float margin)
        {
            if (pad == null || tray == null) return;
            var rend = pad.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                float trayTopWorldY = tray.position.y + (tray.localScale.y * 0.5f);
                float bottomWorldY = rend.bounds.min.y;
                float delta = (trayTopWorldY + margin) - bottomWorldY;
                if (Mathf.Abs(delta) > 1e-5f)
                {
                    var wp = pad.position;
                    wp.y += delta;
                    pad.position = wp;
                }
            }
            else
            {
                var lp = pad.localPosition;
                lp.y = (tray.localScale.y * 0.5f) + (pad.localScale.y * 0.5f) + margin;
                pad.localPosition = lp;
            }
        }
        
        private static void CreateUI(GameObject root)
        {
            // Prefer to parent under Wall_North; avoid creating a separate UI root
            Transform fallbackParent = root.transform;

            // Find or create the panel GameObject; support both under UI and under Wall_North for idempotency
            GameObject panelGO = null;
            Transform existing = GameObject.Find("SB12_Panel")?.transform;
            if (existing != null) panelGO = existing.gameObject;
            if (panelGO == null)
            {
                Transform wallN = GameObject.Find("RoomRoot/Environment/Wall_North")?.transform;
                if (wallN != null)
                {
                    Transform onWall = wallN.Find("SB12_Panel");
                    if (onWall != null) panelGO = onWall.gameObject;
                }
            }
            if (panelGO == null)
            {
                panelGO = new GameObject("SB12_Panel", typeof(RectTransform));
                panelGO.transform.SetParent(fallbackParent);
                createdCount++;
            }

            // Ensure Canvas exists
            Canvas canvasComp = panelGO.GetComponent<Canvas>();
            if (canvasComp == null)
            {
                canvasComp = panelGO.AddComponent<Canvas>();
            }
            canvasComp.renderMode = RenderMode.WorldSpace;

            // Ensure RectTransform exists and configure
            RectTransform rt = panelGO.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = panelGO.AddComponent<RectTransform>();
            }
            // Initial size; will be overridden to fit wall if found
            rt.sizeDelta = new Vector2(1000, 600);
            rt.localScale = Vector3.one * 0.001f;
            rt.pivot = new Vector2(0.5f, 0.5f); // center pivot so panel centers on wall

            if (!panelGO.GetComponent<GraphicRaycaster>()) panelGO.AddComponent<GraphicRaycaster>();
            if (!panelGO.GetComponent<TrackedDeviceGraphicRaycaster>()) panelGO.AddComponent<TrackedDeviceGraphicRaycaster>();

            Transform canvasTf = panelGO.transform;
            CreateUIElement(canvasTf, "Background", new Vector2(0, 0), new Vector2(1, 1), "").gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            var titleTf = CreateUIElement(canvasTf, "Title", new Vector2(0, 0.8f), new Vector2(1, 1), "SB12 EKG Training");
            var bodyTf  = CreateUIElement(canvasTf, "Body", new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.78f), "Welcome to the EKG electrode placement training.");

            Transform btnObj = CreateUIElement(canvasTf, "NextButton", new Vector2(0.35f, 0.06f), new Vector2(0.65f, 0.18f), "");
            btnObj.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.2f);
            btnObj.gameObject.AddComponent<Button>();
            var btnTextTf = CreateUIElement(btnObj, "Text", new Vector2(0, 0), new Vector2(1, 1), "Continue");
            var titleText = titleTf.GetComponent<Text>();
            if (titleText) { titleText.fontSize = 140; titleText.alignment = TextAnchor.UpperCenter; }
            var bodyText = bodyTf.GetComponent<Text>();
            if (bodyText) { bodyText.fontSize = 72; bodyText.alignment = TextAnchor.UpperLeft; }
            var btnText = btnTextTf.GetComponent<Text>();
            if (btnText) { btnText.fontSize = 84; btnText.alignment = TextAnchor.MiddleCenter; }

            // Fit and place the panel relative to the north wall
            Transform wallNorth = GameObject.Find("RoomRoot/Environment/Wall_North")?.transform;
            if (wallNorth == null)
            {
                // Ensure the environment exists then try again
                Debug.Log("[SB12] Wall_North not found. Creating environment and retrying.");
                CreateEnvironment(root);
                wallNorth = GameObject.Find("RoomRoot/Environment/Wall_North")?.transform;
            }
            float worldScale = 0.001f; // 1 canvas unit = 1 mm
            if (wallNorth != null)
            {
                // Match the panel size to the wall size (in meters). Convert meters -> canvas units using worldScale.
                float wallW = wallNorth.localScale.x; // meters
                float wallH = wallNorth.localScale.y; // meters
                rt.sizeDelta = new Vector2(wallW / worldScale, wallH / worldScale);

                // Parent to the wall and neutralize parent's non-uniform scale so panel keeps consistent world size
                panelGO.transform.SetParent(wallNorth, false);
                Vector3 p = wallNorth.lossyScale;
                Vector3 inv = new Vector3(
                    worldScale / Mathf.Max(0.0001f, p.x),
                    worldScale / Mathf.Max(0.0001f, p.y),
                    worldScale / Mathf.Max(0.0001f, p.z)
                );
                panelGO.transform.localScale = inv;
                // Ensure the panel's front (+Z) faces into the room (flip if needed)
                panelGO.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                // Center on the wall (local X=0,Y=0) and offset forward by 1m as requested
                panelGO.transform.localPosition = new Vector3(0f, 0f, 1f);

                // Use a VerticalLayoutGroup to layout children relative to the panel
                var vlg = panelGO.GetComponent<VerticalLayoutGroup>();
                if (vlg == null) vlg = panelGO.AddComponent<VerticalLayoutGroup>();
                // Calculate padding as 5% of panel width/height
                float pw = rt.sizeDelta.x;
                float ph = rt.sizeDelta.y;
                vlg.padding = new RectOffset(
                    Mathf.RoundToInt(pw * 0.05f),
                    Mathf.RoundToInt(pw * 0.05f),
                    Mathf.RoundToInt(ph * 0.05f),
                    Mathf.RoundToInt(ph * 0.05f)
                );
                vlg.spacing = Mathf.RoundToInt(ph * 0.02f);
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                // Background should ignore layout and fill the panel
                var bgRt = panelGO.transform.Find("Background")?.GetComponent<RectTransform>();
                if (bgRt)
                {
                    var bgLE = bgRt.GetComponent<LayoutElement>();
                    if (bgLE == null) bgLE = bgRt.gameObject.AddComponent<LayoutElement>();
                    bgLE.ignoreLayout = true;
                    bgRt.anchorMin = new Vector2(0f, 0f);
                    bgRt.anchorMax = new Vector2(1f, 1f);
                    bgRt.offsetMin = Vector2.zero; // left/bottom = 0
                    bgRt.offsetMax = Vector2.zero; // right/top = 0
                    bgRt.localScale = Vector3.one; // scale = (1,1,1)

                    // Add VerticalLayoutGroup to Background for nice child layout
                    var bgVlg = bgRt.GetComponent<VerticalLayoutGroup>();
                    if (bgVlg == null) bgVlg = bgRt.gameObject.AddComponent<VerticalLayoutGroup>();
                    float bgW = rt.sizeDelta.x;
                    float bgH = rt.sizeDelta.y;
                    bgVlg.padding = new RectOffset(
                        Mathf.RoundToInt(bgW * 0.08f), // left
                        Mathf.RoundToInt(bgW * 0.08f), // right
                        Mathf.RoundToInt(bgH * 0.06f), // top
                        Mathf.RoundToInt(bgH * 0.06f)  // bottom
                    );
                    bgVlg.spacing = Mathf.RoundToInt(bgH * 0.03f);
                    bgVlg.childAlignment = TextAnchor.UpperCenter;
                    bgVlg.childControlWidth = true;
                    bgVlg.childControlHeight = true;
                    bgVlg.childForceExpandWidth = true;
                    bgVlg.childForceExpandHeight = false;
                }

                // Title row (make child of Background)
                var titleTf2 = panelGO.transform.Find("Title");
                var titleRt = titleTf2 ? titleTf2.GetComponent<RectTransform>() : null;
                if (titleRt)
                {
                    if (bgRt) titleRt.SetParent(bgRt, false);
                    titleRt.localScale = Vector3.one; // scale = (1,1,1)
                    var le = titleRt.GetComponent<LayoutElement>();
                    if (le == null) le = titleRt.gameObject.AddComponent<LayoutElement>();
                    le.minHeight = ph * 0.12f;
                    le.preferredHeight = ph * 0.15f;
                    le.flexibleHeight = 0f;
                }

                // Body row (make child of Background)
                var bodyTf2 = panelGO.transform.Find("Body");
                if (bodyTf2 == null) bodyTf2 = panelGO.transform.Find("BodyText"); // fallback for old name
                var bodyRt = bodyTf2 ? bodyTf2.GetComponent<RectTransform>() : null;
                if (bodyRt)
                {
                    if (bgRt) bodyRt.SetParent(bgRt, false);
                    bodyRt.localScale = Vector3.one; // scale = (1,1,1)
                    var le = bodyRt.GetComponent<LayoutElement>();
                    if (le == null) le = bodyRt.gameObject.AddComponent<LayoutElement>();
                    le.minHeight = ph * 0.50f;
                    le.preferredHeight = ph * 0.60f;
                    le.flexibleHeight = 1f;
                }

                // Button row (make child of Background)
                var btnTf2 = panelGO.transform.Find("NextButton");
                if (btnTf2 == null) btnTf2 = panelGO.transform.Find("ContinueButton"); // fallback for old name
                var btnRt = btnTf2 ? btnTf2.GetComponent<RectTransform>() : null;
                if (btnRt)
                {
                    if (bgRt) btnRt.SetParent(bgRt, false);
                    btnRt.localScale = Vector3.one; // scale = (1,1,1)
                    var le = btnRt.GetComponent<LayoutElement>();
                    if (le == null) le = btnRt.gameObject.AddComponent<LayoutElement>();
                    le.minHeight = ph * 0.08f;
                    le.preferredHeight = ph * 0.10f;
                    le.flexibleHeight = 0f;
                    le.preferredWidth = ph * 0.25f; // Make button narrower than full width
                }

                // Scale fonts relative to panel height for better readability
                var titleText2 = titleTf2 ? titleTf2.GetComponent<Text>() : null;
                if (titleText2) 
                {
                    titleText2.fontSize = Mathf.RoundToInt(ph * 0.05f);
                    titleText2.alignment = TextAnchor.MiddleCenter;
                    titleText2.fontStyle = FontStyle.Bold;
                }
                var bodyText2 = bodyTf2 ? bodyTf2.GetComponent<Text>() : null;
                if (bodyText2) 
                {
                    bodyText2.fontSize = Mathf.RoundToInt(ph * 0.03f);
                    bodyText2.alignment = TextAnchor.UpperLeft;
                    bodyText2.fontStyle = FontStyle.Normal;
                }
                var btnText2 = btnTf2 ? btnTf2.GetComponent<Text>() : null;
                if (btnText2) 
                {
                    btnText2.fontSize = Mathf.RoundToInt(ph * 0.04f);
                    btnText2.alignment = TextAnchor.MiddleCenter;
                    btnText2.fontStyle = FontStyle.Bold;
                }
                Debug.Log($"[SB12] SB12_Panel parented to Wall_North. localPos={panelGO.transform.localPosition}, localScale={panelGO.transform.localScale}, sizeDelta={rt.sizeDelta}");
            }
            else
            {
                // Fallback world placement centered to room
                panelGO.transform.SetParent(fallbackParent, false);
                panelGO.transform.position = new Vector3(0f, 1.5f, 2.9f);
                panelGO.transform.rotation = Quaternion.Euler(0, 180f, 0);
                Debug.Log("[SB12] Wall_North still not found. Placed SB12_Panel under RoomRoot as fallback.");
            }
        }
        
        private static Transform CreateUIElement(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string text)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            
            if (!string.IsNullOrEmpty(text))
            {
                Text t = obj.AddComponent<Text>();
                t.text = text;
                t.fontSize = name == "Title" ? 5 : 3;
                t.alignment = name == "Title" ? TextAnchor.UpperCenter : TextAnchor.MiddleCenter;
                t.color = Color.white;
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            
            return obj.transform;
        }

        private static void ApplyColor(Transform tf, string key, Color color)
        {
            if (tf == null) return;
            var rend = tf.GetComponent<Renderer>();
            if (rend == null) return;
            var mat = EnsureMaterialAsset(key, color);
            var arr = rend.sharedMaterials;
            if (arr != null && arr.Length > 1)
            {
                var mats = new Material[arr.Length];
                for (int i = 0; i < arr.Length; i++) mats[i] = mat;
                rend.sharedMaterials = mats;
            }
            else
            {
                rend.sharedMaterial = mat;
            }
        }

        private static Material EnsureMaterialAsset(string key, Color color)
        {
            if (matCache.TryGetValue(key, out var cached) && cached != null) return cached;
            string baseFolder = "Assets/SB12";
            string matsFolder = "Assets/SB12/Materials";
            if (!AssetDatabase.IsValidFolder(baseFolder)) AssetDatabase.CreateFolder("Assets", "SB12");
            if (!AssetDatabase.IsValidFolder(matsFolder)) AssetDatabase.CreateFolder(baseFolder, "Materials");
            string path = $"{matsFolder}/{key}.mat";
            var asset = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (asset == null)
            {
                asset = new Material(Shader.Find("Standard"));
                asset.color = color;
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }
            matCache[key] = asset;
            return asset;
        }
        
        private static void SetRendererColor(Transform root, string key, Color color, bool includeChildren)
        {
            if (root == null) return;
            var mat = EnsureMaterialAsset(key, color);
            if (includeChildren)
            {
                var rends = root.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rends.Length; i++)
                {
                    var r = rends[i];
                    if (r == null) continue;
                    var arr = r.sharedMaterials;
                    if (arr != null && arr.Length > 1)
                    {
                        var mats = new Material[arr.Length];
                        for (int j = 0; j < arr.Length; j++) mats[j] = mat;
                        r.sharedMaterials = mats;
                    }
                    else
                    {
                        r.sharedMaterial = mat;
                    }
                }
            }
            else
            {
                var r = root.GetComponent<Renderer>();
                if (r != null)
                {
                    var arr = r.sharedMaterials;
                    if (arr != null && arr.Length > 1)
                    {
                        var mats = new Material[arr.Length];
                        for (int j = 0; j < arr.Length; j++) mats[j] = mat;
                        r.sharedMaterials = mats;
                    }
                    else
                    {
                        r.sharedMaterial = mat;
                    }
                }
            }
        }

        private static void ApplyThemeMaterials(GameObject root)
        {
            if (root == null) return;
            Color32 wallCol = new Color32(245, 229, 211, 255);
            Color32 floorCol = new Color32(180, 185, 192, 255);
            Color32 patientCol = new Color32(192, 150, 110, 255);
            Color32 mattressCol = new Color32(200, 228, 255, 255);
            Color32 bedFrameCol = new Color32(120, 120, 125, 255);
            Color32 trayTopCol = new Color32(160, 160, 160, 255);
            Color32 trayLegCol = new Color32(90, 90, 90, 255);
            Color32 trayCol = new Color32(60, 60, 60, 255);
            Color32 cartCol = new Color32(170, 170, 175, 255);
            Color32 machineCol = new Color32(130, 160, 190, 255);

            Transform env = GameObject.Find("RoomRoot/Environment")?.transform;
            if (env != null)
            {
                string[] walls = {"Wall_North","Wall_South","Wall_East","Wall_West"};
                for (int i = 0; i < walls.Length; i++)
                {
                    var w = env.Find(walls[i]);
                    if (w) SetRendererColor(w, "mat_wall_theme", wallCol, true);
                }
                var floor = env.Find("Floor");
                if (floor) SetRendererColor(floor, "mat_floor_theme", floorCol, true);
            }

            Transform table = GameObject.Find("RoomRoot/TrayTable")?.transform;
            if (table != null)
            {
                var top = table.Find("Table_Top");
                if (top) SetRendererColor(top, "mat_traytable_top_theme", trayTopCol, false);
                var legs = new[]{"Table_Leg_FL","Table_Leg_FR","Table_Leg_BL","Table_Leg_BR"};
                for (int i = 0; i < legs.Length; i++)
                {
                    var lg = table.Find(legs[i]);
                    if (lg) SetRendererColor(lg, "mat_traytable_leg_theme", trayLegCol, false);
                }
                var tray = table.Find("Tray");
                if (tray) SetRendererColor(tray, "mat_tray_theme", trayCol, false);
            }

            Transform cart = GameObject.Find("RoomRoot/Cart")?.transform;
            if (cart != null)
            {
                var cartTop = cart.Find("Cart_Top");
                if (cartTop) SetRendererColor(cartTop, "mat_cart_theme", cartCol, false);
                var rackBar = cart.Find("Rack/Rack_Bar");
                if (rackBar) SetRendererColor(rackBar, "mat_rack_theme", bedFrameCol, false);
                var machine = cart.Find("MachineConsole");
                if (machine) SetRendererColor(machine, "mat_machine_theme", machineCol, true);
                var pwr = cart.Find("PowerButton");
                if (pwr) SetRendererColor(pwr, "mat_power_theme", new Color32(210,70,70,255), false);
            }

            Transform bed = GameObject.Find("RoomRoot/Bed")?.transform;
            Transform patient = GameObject.Find("RoomRoot/Bed/Patient")?.transform;
            if (patient != null)
            {
                var skinMat = EnsureMaterialAsset("mat_patient_skin", patientCol);
                var rends = patient.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rends.Length; i++)
                {
                    var r = rends[i];
                    if (r == null) continue;
                    string n = r.transform.name.ToLowerInvariant();
                    if (n.Contains("genesis") || n.Contains("body") || n.Contains("torso") || n.Contains("head") || n.Contains("arm") || n.Contains("leg") || n.Contains("hand") || n.Contains("foot") || n.Contains("skin") )
                    {
                        var arr = r.sharedMaterials;
                        if (arr != null && arr.Length > 0)
                        {
                            var mats = new Material[arr.Length];
                            for (int j = 0; j < arr.Length; j++) mats[j] = skinMat;
                            r.sharedMaterials = mats;
                        }
                        else
                        {
                            r.sharedMaterial = skinMat;
                        }
                    }
                }
                var genesisTf = FindChildByNameContains(patient, "genesis9");
                if (genesisTf != null)
                {
                    var gensRends = genesisTf.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < gensRends.Length; i++)
                    {
                        var r = gensRends[i];
                        if (r == null) continue;
                        var arr = r.sharedMaterials;
                        if (arr != null && arr.Length > 0)
                        {
                            var mats = new Material[arr.Length];
                            for (int j = 0; j < arr.Length; j++) mats[j] = skinMat;
                            r.sharedMaterials = mats;
                        }
                        else
                        {
                            r.sharedMaterial = skinMat;
                        }
                    }
                }
            }
            if (bed != null)
            {
                var mattress = FindChildByNameContains(bed, "mattress");
                if (mattress) SetRendererColor(mattress, "mat_mattress_theme", mattressCol, true);
                var pillow = FindChildByNameContains(bed, "pillow");
                if (pillow) SetRendererColor(pillow, "mat_pillow_theme", Color.white, true);
                var frame = FindChildByNameContains(bed, "frame") ?? FindChildByNameContains(bed, "rail");
                if (frame) SetRendererColor(frame, "mat_bedframe_theme", bedFrameCol, true);
            }
        }

        private static Transform FindChildByNameContains(Transform root, string token)
        {
            if (root == null || string.IsNullOrEmpty(token)) return null;
            string t = token.ToLowerInvariant();
            if (root.name.ToLowerInvariant().Contains(t)) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var c = FindChildByNameContains(root.GetChild(i), token);
                if (c != null) return c;
            }
            return null;
        }
        
        private static void GenerateRemapMaterials()
        {
            // Create a single material asset per name so remap panels can pick them without duplicates
            // Colors are guessed to be sensible defaults for readability
            var pairs = new (string name, Color col)[]
            {
                ("Arms", new Color32(192,150,110,255)),
                ("Base_Middle", new Color32(180,185,192,255)),
                ("Base_Upper", new Color32(200,205,210,255)),
                ("Bed_CushionTXT", new Color32(200,228,255,255)),
                ("Bed_PaperTXT", new Color32(245,245,245,255)),
                ("Body", new Color32(192,150,110,255)),
                ("DoctorBedTXT", new Color32(170,170,175,255)),
                ("EyeMoisture_Left", new Color32(210,230,255,180)),
                ("EyeMoisture_Right", new Color32(210,230,255,180)),
                ("Eye_Left", new Color32(235,240,245,255)),
                ("Eye_Right", new Color32(235,240,245,255)),
                ("Eyebrows_Primary", new Color32(60,45,35,255)),
                ("Eyebrows_Secondary", new Color32(60,45,35,255)),
                ("Eyelashes_Lower", Color.black),
                ("Eyelashes_Upper", Color.black),
                ("Fingernails", new Color32(245,220,210,255)),
                ("Head", new Color32(192,150,110,255)),
                ("LVA_Pant", new Color32(40,70,120,255)),
                ("LVA_Pant_Lower", new Color32(40,70,120,255)),
                ("LVA_Pant_Upper", new Color32(40,70,120,255)),
                ("Legs", new Color32(192,150,110,255)),
                ("Lewis__high_top_fade", new Color32(40,35,30,255)),
                ("Mouth", new Color32(210,120,120,255)),
                ("Mouth_Cavity", new Color32(80,20,20,255)),
                ("PillowTXT", Color.white),
                ("Sole_Upper", new Color32(40,40,40,255)),
                ("Soole_001", new Color32(40,40,40,255)),
                ("Straps_001", new Color32(50,50,50,255)),
                ("Tear", new Color32(210,230,255,180)),
                ("Teeth", new Color32(245,245,240,255)),
                ("Toenails", new Color32(240,215,205,255)),
            };
            for (int i = 0; i < pairs.Length; i++)
            {
                EnsureMaterialAsset(pairs[i].name, pairs[i].col);
            }
            AssetDatabase.Refresh();
        }
        
        private static Transform CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            Transform obj = parent.Find(name);
            if (obj == null)
            {
                GameObject go = GameObject.CreatePrimitive(type);
                go.name = name;
                go.transform.SetParent(parent);
                obj = go.transform;
                createdCount++;
            }
            
            obj.position = pos;
            obj.rotation = rot;
            obj.localScale = scale;
            return obj;
        }
        
        private static void CreateLight(Transform parent, string name, LightType type, Vector3 pos, Quaternion rot)
        {
            Transform lightTf = FindOrCreateChild(parent, name);
            lightTf.position = pos;
            lightTf.rotation = rot;

            // Ensure a Light component exists. If something odd happens, recreate the GO.
            Light l = lightTf.GetComponent<Light>();
            if (l == null)
            {
                try
                {
                    l = lightTf.gameObject.AddComponent<Light>();
                }
                catch
                {
                    // Fallback: recreate node with a Light
                    var go = new GameObject(name);
                    go.transform.SetParent(parent);
                    go.transform.position = pos;
                    go.transform.rotation = rot;
                    lightTf = go.transform;
                    l = go.AddComponent<Light>();
                }
            }

            if (l != null)
            {
                l.type = type;
                if (type == LightType.Directional)
                {
                    l.intensity = 1.0f;
                }
                else
                {
                    l.intensity = 0.5f;
                    l.range = 5.0f;
                }
            }
        }
        
        private static void GenerateRuntimeScripts()
        {
            AssetDatabase.Refresh();
        }
        
        private static void SaveScene()
        {
            string scenePath = "Assets/Scenes/SB12_Auto.unity";
            string dir = Path.GetDirectoryName(scenePath);
            
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), 
                scenePath);
        }
        
        private static void ShowBuildReport()
        {
            string report = $"SB12 Scene Build Complete!\n\n";
            report += $"• Created: {createdCount} objects\n";
            report += $"• Updated: {updatedCount} objects\n";
            report += $"• Warnings: {warningCount}\n";
            
            if (missingAssets.Count > 0)
            {
                report += $"\nMissing assets:\n";
                foreach (var asset in missingAssets)
                {
                    report += $"  - {asset}\n";
                }
                report += "\nRun 'git lfs pull' if assets are missing.";
            }
            
            report += "\n\nScene saved to: Assets/Scenes/SB12_Auto.unity";
            
            EditorUtility.DisplayDialog("Build Report", report, "OK");
            Debug.Log(report);
        }
    }
}
#endif
