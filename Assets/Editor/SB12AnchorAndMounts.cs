#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace SB12.Editor
{
    public static class SB12AnchorAndMounts
    {
        [MenuItem("Tools/SB12/Reposition Patient Anchor & Mounts")] 
        public static void Reposition()
        {
            var patient = GameObject.Find("RoomRoot/Bed/Patient");
            if (patient == null)
            {
                EditorUtility.DisplayDialog("SB12", "Patient not found at RoomRoot/Bed/Patient.", "OK");
                return;
            }

            // Ensure/Find anchor
            var anchorTf = patient.transform.Find("PatientAnchor");
            if (anchorTf == null)
            {
                anchorTf = new GameObject("PatientAnchor").transform;
                anchorTf.SetParent(patient.transform, false);
            }

            // Place anchor using LOCAL transform so inspector shows exact values regardless of parent
            Undo.RecordObject(anchorTf, "Reposition PatientAnchor");
            anchorTf.SetParent(patient.transform, false);
            anchorTf.localPosition = new Vector3(0.25f, 1.1f, 0f);
            anchorTf.localRotation = Quaternion.Euler(90f, 90f, 0f);
            anchorTf.localScale = Vector3.one;

            // Prepare/position mounts relative to anchor using patient extents
            var mounts = anchorTf.Find("Mounts");
            if (mounts == null)
            {
                mounts = new GameObject("Mounts").transform;
                mounts.SetParent(anchorTf, false);
            }

            // Exact local positions relative to anchor
            Vector3 ra = new Vector3( 0.36f,   -0.194f,   0.183f);
            Vector3 la = new Vector3(-0.355f, -0.2f,   0.195f);
            Vector3 rl = new Vector3( 0.173f,  -1.035f, 0.145f);
            Vector3 ll = new Vector3(-0.2f,   -1.035f, 0.145f);
            Vector3 v1 = new Vector3( 0.0962f, 0.0577f, -0.01f);
            Vector3 v2 = new Vector3( 0.051f,  0.0605f, -0.01f);
            Vector3 v3 = new Vector3( 0.0055f, 0.0608f, -0.0136f);
            Vector3 v4 = new Vector3(-0.0376f, 0.0608f, -0.014f);
            Vector3 v5 = new Vector3(-0.0821f, 0.0636f, -0.0145f);
            Vector3 v6 = new Vector3(-0.125f,  0.0709f, -0.01f);

            SetMount(mounts, "Mount_RA", ra, new Color(1f, 0.15f, 0.15f));
            SetMount(mounts, "Mount_LA", la, new Color(0.2f, 0.5f, 1f));
            SetMount(mounts, "Mount_RL", rl, new Color(1f, 0f, 1f));
            SetMount(mounts, "Mount_LL", ll, new Color(0f, 1f, 1f));

            SetMount(mounts, "Mount_V1", v1, new Color(0.2f, 1f, 0.2f));
            SetMount(mounts, "Mount_V2", v2, new Color(1f, 1f, 0.2f));
            SetMount(mounts, "Mount_V3", v3, new Color(1f, 0.6f, 0.2f));
            SetMount(mounts, "Mount_V4", v4, new Color(0.6f, 0.2f, 1f));
            SetMount(mounts, "Mount_V5", v5, new Color(1f, 0.3f, 0.7f));
            SetMount(mounts, "Mount_V6", v6, new Color(0.1f, 0.9f, 0.8f));

            EditorUtility.DisplayDialog("SB12", "PatientAnchor and mount positions updated.", "OK");
        }

        private static bool TryGetCombinedBounds(Transform root, out Bounds worldBounds)
        {
            worldBounds = new Bounds();
            var rends = root.GetComponentsInChildren<Renderer>(true);
            bool inited = false;
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (r == null) continue;
                if (!inited)
                {
                    worldBounds = r.bounds;
                    inited = true;
                }
                else
                {
                    worldBounds.Encapsulate(r.bounds);
                }
            }
            return inited;
        }

        private static void SetMount(Transform mounts, string name, Vector3 localPos, Color color)
        {
            var t = mounts.Find(name);
            if (t == null)
            {
                t = new GameObject(name).transform;
                t.SetParent(mounts, false);
            }
            Undo.RecordObject(t, $"Move {name}");
            t.localPosition = localPos;

            var col = t.GetComponent<SphereCollider>();
            if (col == null) col = t.gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true; col.radius = 0.03f;

            var marker = t.Find("Marker");
            if (marker == null)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "Marker";
                quad.transform.SetParent(t, false);
                quad.transform.localPosition = Vector3.zero;
                quad.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                var r = quad.GetComponent<Renderer>();
                if (r != null)
                {
                    var m = new Material(Shader.Find("Standard"));
                    m.color = color;
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", color);
                    r.sharedMaterial = m;
                }
            }
            else
            {
                var r = marker.GetComponent<Renderer>();
                if (r != null)
                {
                    var m = r.sharedMaterial ?? new Material(Shader.Find("Standard"));
                    m.color = color;
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", color);
                    r.sharedMaterial = m;
                }
            }
        }

        private static float SafeDiv(float a, float b) => b > 1e-5f ? a / b : a;
    }
}
#endif
