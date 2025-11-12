#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SB12.Editor
{
    public static class SB12UIAutoWire
    {
        [MenuItem("Tools/SB12/Wire Slide Controller")] 
        public static void WireSlideController()
        {
            var panel = GameObject.Find("SB12_Panel");
            if (panel == null)
            {
                var wallN = GameObject.Find("RoomRoot/Environment/Wall_North");
                if (wallN != null) panel = wallN.transform.Find("SB12_Panel")?.gameObject;
            }
            if (panel == null)
            {
                var uiRoot = GameObject.Find("RoomRoot/UI");
                if (uiRoot != null) panel = uiRoot.transform.Find("SB12_Panel")?.gameObject;
            }
            if (panel == null)
            {
                EditorUtility.DisplayDialog("SB12", "SB12_Panel not found in the scene.", "OK");
                return;
            }

            var sc = panel.GetComponent<global::SB12.SlideController>();
            if (sc == null) sc = panel.AddComponent<global::SB12.SlideController>();

            Transform FindDeep(Transform parent, string name)
            {
                if (parent == null) return null;
                var d = parent.Find(name); if (d != null) return d;
                for (int i = 0; i < parent.childCount; i++)
                {
                    var c = FindDeep(parent.GetChild(i), name); if (c != null) return c;
                }
                return null;
            }

            var titleT = FindDeep(panel.transform, "Title")?.GetComponent<Text>();
            var bodyTf = FindDeep(panel.transform, "Body") ?? FindDeep(panel.transform, "BodyText");
            var bodyT = bodyTf ? bodyTf.GetComponent<Text>() : null;
            var btnTf = FindDeep(panel.transform, "NextButton") ?? FindDeep(panel.transform, "ContinueButton");
            var btn = btnTf ? btnTf.GetComponent<Button>() : null;
            var footerT = btn ? btn.GetComponentInChildren<Text>() : null;

            var so = new SerializedObject(sc);
            so.FindProperty("slidePanel").objectReferenceValue = panel;
            so.FindProperty("titleText").objectReferenceValue = titleT;
            so.FindProperty("bodyText").objectReferenceValue = bodyT;
            so.FindProperty("continueButton").objectReferenceValue = btn;
            so.FindProperty("footerText").objectReferenceValue = footerT;

            // Optional scene refs
            var equip = GameObject.Find("RoomRoot/Cart");
            var power = GameObject.Find("RoomRoot/Cart/PowerButton");
            so.FindProperty("equipment").objectReferenceValue = equip;
            so.FindProperty("powerButton").objectReferenceValue = power;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sc);

            EditorUtility.DisplayDialog("SB12", "SlideController wired to SB12_Panel.", "OK");
        }
    }
}
#endif
