using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SB12
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class WireEndController : MonoBehaviour
    {
        [Header("References")]
        public WireRuntime wire;
        public Renderer[] renderers;

        [Header("Materials")]
        public Material baseMaterial;
        public Material highlightMaterial;
        public Material wireBaseMaterial;
        public Material wireHighlightMaterial;

        [Header("Behavior")]
        public bool returnToHomeOnActivate = true;

        private XRGrabInteractable grab;
        private Transform homeParent;
        private Vector3 homeLocalPos;
        private Quaternion homeLocalRot;
        private bool hovered;

        private void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }
            // Home pose
            homeParent = transform.parent;
            homeLocalPos = transform.localPosition;
            homeLocalRot = transform.localRotation;

            // XR events
            grab.hoverEntered.AddListener(OnHoverEntered);
            grab.hoverExited.AddListener(OnHoverExited);
            grab.selectEntered.AddListener(OnSelectEntered);
            grab.selectExited.AddListener(OnSelectExited);
            grab.activated.AddListener(OnActivated);
        }

        private void OnDestroy()
        {
            if (grab != null)
            {
                grab.hoverEntered.RemoveListener(OnHoverEntered);
                grab.hoverExited.RemoveListener(OnHoverExited);
                grab.selectEntered.RemoveListener(OnSelectEntered);
                grab.selectExited.RemoveListener(OnSelectExited);
                grab.activated.RemoveListener(OnActivated);
            }
        }

        private void SetMaterial(Material m)
        {
            if (m == null) return;
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;
                var arr = r.sharedMaterials;
                if (arr != null && arr.Length > 1)
                {
                    var mats = new Material[arr.Length];
                    for (int j = 0; j < arr.Length; j++) mats[j] = m;
                    r.sharedMaterials = mats;
                }
                else r.sharedMaterial = m;
            }
        }

        private void OnHoverEntered(HoverEnterEventArgs _)
        {
            hovered = true;
            if (highlightMaterial != null) SetMaterial(highlightMaterial);
            if (wire != null)
            {
                var lr = wire.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    if (wireHighlightMaterial != null) lr.material = wireHighlightMaterial;
                    else if (highlightMaterial != null) lr.material = highlightMaterial;
                }
            }
        }
        private void OnHoverExited(HoverExitEventArgs _)
        {
            hovered = false;
            if (!grab.isSelected && baseMaterial != null) SetMaterial(baseMaterial);
            if (!grab.isSelected && wire != null)
            {
                var lr = wire.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    if (wireBaseMaterial != null) lr.material = wireBaseMaterial;
                    else if (baseMaterial != null) lr.material = baseMaterial;
                }
            }
        }
        private void OnSelectEntered(SelectEnterEventArgs _)
        {
            if (highlightMaterial != null) SetMaterial(highlightMaterial);
        }
        private void OnSelectExited(SelectExitEventArgs _)
        {
            if (baseMaterial != null) SetMaterial(baseMaterial);
        }
        private void OnActivated(ActivateEventArgs _)
        {
            if (returnToHomeOnActivate)
            {
                ResetToHome();
            }
        }

        public void ResetToHome()
        {
            // Return to home parent and pose
            var rb = GetComponent<Rigidbody>();
            if (rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            transform.SetParent(homeParent, true);
            transform.localPosition = homeLocalPos;
            transform.localRotation = homeLocalRot;
            if (baseMaterial != null) SetMaterial(baseMaterial);
            if (wire != null)
            {
                var lr = wire.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    if (wireBaseMaterial != null) lr.material = wireBaseMaterial;
                    else if (baseMaterial != null) lr.material = baseMaterial;
                }
            }
        }
    }
}
