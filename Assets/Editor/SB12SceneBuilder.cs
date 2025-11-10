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
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating mount points...", 0.6f);
                CreateMountPoints(root);
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating electrode pads...", 0.7f);
                CreatePads(root);
                
                EditorUtility.DisplayProgressBar("SB12 Builder", "Creating UI...", 0.8f);
                CreateUI(root);
                
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
            
            Transform tray = FindOrCreateChild(cart, "Tray");
            // Place tray centered on the cart top surface
            tray.position = new Vector3(1.2f, 0.95f, 0.45f);
            Transform traySurf = CreatePrimitive(tray, "Tray_Surface", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, new Vector3(0.6f, 0.03f, 0.4f));
            ApplyColor(traySurf, "mat_tray", new Color32(170, 170, 170, 255));
            
            Transform rack = FindOrCreateChild(cart, "Rack");
            rack.position = new Vector3(1.2f, 1.05f, -0.4f);
            Transform rackBar = CreatePrimitive(rack, "Rack_Bar", PrimitiveType.Cylinder, Vector3.zero, Quaternion.Euler(0, 0, 90), new Vector3(0.01f, 0.7f, 0.01f));
            ApplyColor(rackBar, "mat_rack", new Color32(100, 100, 100, 255));
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
            Transform tray = GameObject.Find("RoomRoot/Cart/Tray")?.transform;
            if (tray == null) return;
            
            Transform padsParent = FindOrCreateChild(tray, "Pads");
            GameObject padPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ASSETS_FOLDER}/EKG Pad With Back.fbx");
            
            for (int i = 0; i < 10; i++)
            {
                string padName = $"Pad_{(i + 1):D2}";
                Transform pad = padsParent.Find(padName);
                
                if (pad == null)
                {
                    GameObject padObj;
                    if (padPrefab != null)
                    {
                        padObj = (GameObject)PrefabUtility.InstantiatePrefab(padPrefab, padsParent);
                    }
                    else
                    {
                        padObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        padObj.transform.localScale = new Vector3(0.03f, 0.01f, 0.03f);
                    }
                    
                    padObj.name = padName;
                    pad = padObj.transform;
                    pad.SetParent(padsParent);
                    
                    int row = i / 5;
                    int col = i % 5;
                    float startX = -0.22f, spacingX = 0.11f;
                    float startZ = -0.12f, spacingZ = 0.12f;
                    pad.localPosition = new Vector3(startX + col * spacingX, 0.03f, startZ + row * spacingZ);
                    pad.localRotation = Quaternion.identity;
                    
                    padObj.AddComponent<Rigidbody>();
                    padObj.AddComponent<BoxCollider>();
                    XRGrabInteractable grab = padObj.AddComponent<XRGrabInteractable>();
                    grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                }
            }
        }
        
        private static void CreateUI(GameObject root)
        {
            // Ensure UI root exists
            Transform ui = FindOrCreateChild(root.transform, "UI");

            // Find or create the panel GameObject explicitly to avoid using a destroyed Transform
            GameObject panelGO = ui.Find("SB12_Panel") ? ui.Find("SB12_Panel").gameObject : null;
            if (panelGO == null)
            {
                panelGO = new GameObject("SB12_Panel", typeof(RectTransform));
                panelGO.transform.SetParent(ui);
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
            rt.sizeDelta = new Vector2(160, 90);
            rt.localScale = Vector3.one * 0.0015f;

            if (!panelGO.GetComponent<GraphicRaycaster>()) panelGO.AddComponent<GraphicRaycaster>();
            if (!panelGO.GetComponent<TrackedDeviceGraphicRaycaster>()) panelGO.AddComponent<TrackedDeviceGraphicRaycaster>();

            Transform canvasTf = panelGO.transform;
            CreateUIElement(canvasTf, "Background", new Vector2(0, 0), new Vector2(1, 1), "").gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            CreateUIElement(canvasTf, "Title", new Vector2(0, 0.8f), new Vector2(1, 1), "SB12 EKG Training");
            CreateUIElement(canvasTf, "BodyText", new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.8f), "Welcome to the EKG electrode placement training.");

            Transform btnObj = CreateUIElement(canvasTf, "ContinueButton", new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.25f), "");
            btnObj.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.2f);
            btnObj.gameObject.AddComponent<Button>();
            CreateUIElement(btnObj, "Text", new Vector2(0, 0), new Vector2(1, 1), "Continue");

            // Reparent UI to the north wall so it's visible on the wall
            Transform wallNorth = GameObject.Find("RoomRoot/Environment/Wall_North")?.transform;
            if (wallNorth != null)
            {
                panelGO.transform.SetParent(wallNorth);
                // Mount at comfortable eye height and just in front of wall face
                panelGO.transform.localPosition = new Vector3(0f, 1.4f, 0.055f);
                panelGO.transform.localRotation = Quaternion.identity;
                panelGO.transform.localScale = Vector3.one * 0.0015f;
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
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            
            return obj.transform;
        }

        private static void ApplyColor(Transform tf, string key, Color color)
        {
            if (tf == null) return;
            var rend = tf.GetComponent<Renderer>();
            if (rend == null) return;
            if (!matCache.TryGetValue(key, out var mat))
            {
                mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                matCache[key] = mat;
            }
            rend.sharedMaterial = mat;
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
