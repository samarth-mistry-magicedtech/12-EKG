#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SB12.Editor
{
    public static class SB12WireBuilder
    {
        [MenuItem("Tools/SB12/Build Wires")] 
        public static void BuildWires()
        {
            var cart = GameObject.Find("RoomRoot/Cart");
            if (cart == null)
            {
                EditorUtility.DisplayDialog("SB12 Wires", "Cart not found at RoomRoot/Cart. Build the scene first.", "OK");
                return;
            }

            var root = FindOrCreate(cart.transform, "Wires");

            // Per-lead colors derived from marker materials (fallback to default palette)
            var defaultPalette = GetDefaultLeadPalette();

            // Create anchors laid out along the cart top front edge
            // Fallback if Cart_Top not present: use cart origin with offsets
            var cartTop = cart.transform.Find("Cart_Top");
            Vector3 basePos = cartTop ? cartTop.position : (cart.transform.position + new Vector3(1.2f, 0.92f, 0.45f));
            Vector3 right = cart.transform.right;
            Vector3 forward = cart.transform.forward;

            // Wire names
            string[] names = {"RA","LA","RL","LL","V1","V2","V3","V4","V5","V6"};

            // Place 10 anchors in a row with spacing
            float spacing = 0.07f; // 7cm
            Vector3 start = basePos - forward * 0.25f - right * (spacing * 4.5f);

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                // Resolve color for this lead from marker mat or fallback palette
                Color leadColor;
                if (!TryLoadMarkerColor(name, out leadColor))
                {
                    if (!defaultPalette.TryGetValue(name, out leadColor)) leadColor = new Color(0.5f,0.5f,0.5f);
                }

                // Build or load per-lead materials
                var wireBase = EnsureMat($"Assets/SB12/Materials/Wires/wire_{name}_base.mat", leadColor);
                var wireHL   = EnsureMat($"Assets/SB12/Materials/Wires/wire_{name}_highlight.mat", Brighten(leadColor, 1.5f));
                var plugBase = EnsureMat($"Assets/SB12/Materials/Wires/plug_{name}_base.mat", leadColor);
                var plugHL   = EnsureMat($"Assets/SB12/Materials/Wires/plug_{name}_highlight.mat", Brighten(leadColor, 1.5f));

                var anchor = FindOrCreate(root, $"Anchor_{name}");
                anchor.position = start + right * (i * spacing);
                anchor.rotation = Quaternion.identity;
                var anchorVis = anchor.GetComponentInChildren<MeshRenderer>();
                if (anchorVis == null)
                {
                    var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    s.name = "Gizmo";
                    s.transform.SetParent(anchor, false);
                    s.transform.localScale = Vector3.one * 0.01f;
                    Object.DestroyImmediate(s.GetComponent<Collider>());
                    s.GetComponent<Renderer>().sharedMaterial = wireBase;
                }
                else
                {
                    // Update gizmo color if it already exists
                    var r = anchorVis.GetComponent<Renderer>();
                    if (r != null) r.sharedMaterial = wireBase;
                }

                // Build or update plug
                var plug = FindOrCreate(root, $"Plug_{name}");
                BuildOrUpdatePlug(plug, plugBase, plugHL);

                // Home pose next to anchor, offset forward a bit
                plug.position = anchor.position - forward * 0.05f;
                plug.rotation = Quaternion.LookRotation(-forward, Vector3.up);

                // Wire holder
                var wireGo = FindOrCreate(root, $"Wire_{name}").gameObject;
                var lr = wireGo.GetComponent<LineRenderer>();
                if (lr == null) lr = wireGo.AddComponent<LineRenderer>();
                lr.material = wireBase;

                var wr = wireGo.GetComponent<global::SB12.WireRuntime>();
                if (wr == null) wr = wireGo.AddComponent<global::SB12.WireRuntime>();
                wr.anchor = anchor;
                wr.plug = plug;
                wr.width = 0.008f;
                wr.segments = 48;
                wr.slack = 0.3f;

                // End controller on plug
                var end = plug.GetComponent<global::SB12.WireEndController>();
                if (end == null) end = plug.gameObject.AddComponent<global::SB12.WireEndController>();
                end.wire = wr;
                end.baseMaterial = plugBase;
                end.highlightMaterial = plugHL;
                end.wireBaseMaterial = wireBase;
                end.wireHighlightMaterial = wireHL;
                end.leadName = name;

                // Ensure plug renderers use base mat now
                foreach (var r in plug.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial = plugBase;
            }

            EditorUtility.DisplayDialog("SB12 Wires", "Wires built under Cart/Wires. Use XR to grab plugs. Press Activate to reset.", "OK");
        }

        private static Transform FindOrCreate(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent);
                t = go.transform;
            }
            return t;
        }

        private static Material EnsureMat(string path, Color c)
        {
            string folder = System.IO.Path.GetDirectoryName(path).Replace('\\','/');
            string[] parts = folder.Split('/');
            string acc = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(acc + "/" + parts[i]))
                {
                    AssetDatabase.CreateFolder(acc, parts[i]);
                }
                acc += "/" + parts[i];
            }
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                mat.color = c;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", c * 0.25f);
                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
            }
            else
            {
                mat.color = c; mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", c * 0.25f);
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        private static bool TryLoadMarkerColor(string lead, out Color color)
        {
            // Markers are created as: Assets/SB12/Materials/Markers/mat_marker_Mount_<LEAD>.mat
            string path = $"Assets/SB12/Materials/Markers/mat_marker_Mount_{lead}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                color = mat.color;
                return true;
            }
            color = default(Color);
            return false;
        }

        private static Dictionary<string, Color> GetDefaultLeadPalette()
        {
            // Mirrors SB12AnchorAndMounts palette
            return new Dictionary<string, Color>
            {
                {"RA", new Color(1f, 0.15f, 0.15f)},
                {"LA", new Color(0.2f, 0.5f, 1f)},
                {"RL", new Color(1f, 0f, 1f)},
                {"LL", new Color(0f, 1f, 1f)},
                {"V1", new Color(0.2f, 1f, 0.2f)},
                {"V2", new Color(1f, 1f, 0.2f)},
                {"V3", new Color(1f, 0.6f, 0.2f)},
                {"V4", new Color(0.6f, 0.2f, 1f)},
                {"V5", new Color(1f, 0.3f, 0.7f)},
                {"V6", new Color(0.1f, 0.9f, 0.8f)},
            };
        }

        private static Color Brighten(Color c, float factor)
        {
            float r = Mathf.Clamp01(c.r * factor);
            float g = Mathf.Clamp01(c.g * factor);
            float b = Mathf.Clamp01(c.b * factor);
            return new Color(r,g,b,1f);
        }

        private static void BuildOrUpdatePlug(Transform plug, Material baseMat, Material highlightMat)
        {
            // Simple primitive plug: capsule + small ring
            if (plug.childCount == 0)
            {
                var cap = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                cap.name = "Body";
                cap.transform.SetParent(plug, false);
                cap.transform.localScale = new Vector3(0.025f, 0.05f, 0.025f);
                cap.GetComponent<Renderer>().sharedMaterial = baseMat;

                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "Ring";
                ring.transform.SetParent(plug, false);
                ring.transform.localScale = new Vector3(0.03f, 0.005f, 0.03f);
                ring.transform.localPosition = new Vector3(0, -0.045f, 0);
                ring.GetComponent<Renderer>().sharedMaterial = baseMat;

                var col = plug.gameObject.GetComponent<CapsuleCollider>();
                if (col == null) col = plug.gameObject.AddComponent<CapsuleCollider>();
                col.radius = 0.03f; col.height = 0.12f; col.center = Vector3.zero; col.direction = 1;
            }

            // Ensure proximity trigger for auto-attach exists
            var trigEnsure = plug.Find("AttachTrigger");
            if (trigEnsure == null)
            {
                var trig = new GameObject("AttachTrigger").transform;
                trig.SetParent(plug, false);
                var sc = trig.gameObject.AddComponent<SphereCollider>();
                sc.isTrigger = true; sc.radius = 0.06f; // match default attachDistance
            }

            var rb = plug.GetComponent<Rigidbody>();
            if (rb == null) rb = plug.gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.05f; rb.drag = 1f; rb.angularDrag = 1f;

            var grab = plug.GetComponent<XRGrabInteractable>();
            if (grab == null) grab = plug.gameObject.AddComponent<XRGrabInteractable>();
            grab.throwOnDetach = false;
            grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        }
    }
}
#endif
