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
        public string leadName;
        public float attachDistance = 1.0f;

        private XRGrabInteractable grab;
        private Transform homeParent;
        private Vector3 homeLocalPos;
        private Quaternion homeLocalRot;
        private bool hovered;
        private bool isAttached;
        private Transform attachedMount;

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

            // Fallback: infer lead name from object name if not set (e.g., Plug_RA -> RA)
            if (string.IsNullOrEmpty(leadName))
            {
                leadName = ParseLeadFromName(gameObject.name);
            }

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
            if (isAttached) DetachFromMount();
            if (highlightMaterial != null) SetMaterial(highlightMaterial);
        }
        private void OnSelectExited(SelectExitEventArgs _)
        {
            if (baseMaterial != null) SetMaterial(baseMaterial);
            // Try to attach if released near matching mount; otherwise return home after a short delay
            if (!isAttached) StartCoroutine(TryAttachThenMaybeReturn());
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
            isAttached = false;
            attachedMount = null;
        }

        private System.Collections.IEnumerator TryAttachThenMaybeReturn()
        {
            // Give physics a frame to settle
            yield return null;
            if (TryAttachNearby()) yield break;
            // Small grace period for triggers to fire
            yield return new WaitForSeconds(0.2f);
            if (!isAttached) ResetToHome();
        }

        private bool TryAttachNearby()
        {
            // Check nearby colliders in a sphere for a matching Mount_<LEAD>
            var hits = Physics.OverlapSphere(transform.position, attachDistance);
            Transform best = null;
            float bestDist = attachDistance;
            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                if (col == null) continue;
                Transform mount; string mountLead;
                if (TryResolveMount(col.transform, out mount, out mountLead))
                {
                    if (!string.IsNullOrEmpty(leadName) && mountLead == leadName)
                    {
                        float d = Vector3.Distance(transform.position, mount.position);
                        if (d <= bestDist)
                        {
                            bestDist = d; best = mount;
                        }
                    }
                }
            }
            if (best != null)
            {
                AttachToMount(best);
                return true;
            }
            return false;
        }

        private void OnTriggerStay(Collider other)
        {
            if (grab != null && grab.isSelected) return;
            if (isAttached) return;
            if (other == null) return;
            Transform mount;
            string mountLead;
            if (TryResolveMount(other.transform, out mount, out mountLead))
            {
                if (!string.IsNullOrEmpty(leadName) && mountLead == leadName)
                {
                    var d = Vector3.Distance(transform.position, mount.position);
                    if (d <= attachDistance)
                    {
                        AttachToMount(mount);
                    }
                }
            }
        }

        private bool TryResolveMount(Transform t, out Transform mount, out string lead)
        {
            mount = null; lead = null;
            var curr = t;
            for (int i = 0; i < 3 && curr != null; i++)
            {
                var n = curr.name;
                if (!string.IsNullOrEmpty(n) && n.StartsWith("Mount_"))
                {
                    mount = curr;
                    lead = n.Substring("Mount_".Length);
                    return true;
                }
                curr = curr.parent;
            }
            return false;
        }

        private void AttachToMount(Transform mount)
        {
            isAttached = true;
            attachedMount = mount;
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; rb.isKinematic = true;
            }
            transform.SetParent(mount, true);
            transform.position = mount.position;
            transform.rotation = mount.rotation;
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

        private void DetachFromMount()
        {
            isAttached = false;
            attachedMount = null;
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            transform.SetParent(homeParent, true);
        }

        private string ParseLeadFromName(string objName)
        {
            if (string.IsNullOrEmpty(objName)) return null;
            // Expecting names like "Plug_RA" => returns "RA"
            const string prefix = "Plug_";
            if (objName.StartsWith(prefix))
            {
                return objName.Substring(prefix.Length);
            }
            return null;
        }
    }
}
