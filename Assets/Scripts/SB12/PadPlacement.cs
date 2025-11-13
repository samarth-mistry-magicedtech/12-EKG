using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace SB12
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class PadPlacement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float snapDistance = 0.1f;
        [SerializeField] private bool autoSnap = true;
        [SerializeField] private LayerMask mountLayerMask = -1;
        
        [Header("State")]
        [SerializeField] private bool isPlaced = false;
        [SerializeField] private string currentMount = "";
        
        [Header("Events")]
        public UnityEvent<string> OnPlaced = new UnityEvent<string>();
        public UnityEvent<string> OnRemoved = new UnityEvent<string>();
        
        private XRGrabInteractable grabInteractable;
        private Rigidbody rb;
        private Transform originalParent;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private GameObject currentMountObject;
        
        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();
            
            originalParent = transform.parent;
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
        
        private void OnEnable()
        {
            grabInteractable.selectExited.AddListener(OnGrabReleased);
            grabInteractable.selectEntered.AddListener(OnGrabbed);
        }
        
        private void OnDisable()
        {
            grabInteractable.selectExited.RemoveListener(OnGrabReleased);
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        }
        
        private void OnGrabbed(SelectEnterEventArgs args)
        {
            if (isPlaced)
            {
                RemoveFromMount();
            }
            
            // Optional: Spawn peeled backing effect here
            SpawnPeeledBacking();
        }
        
        private void OnGrabReleased(SelectExitEventArgs args)
        {
            if (autoSnap)
            {
                TrySnapToNearestMount();
            }
        }
        
        private void TrySnapToNearestMount()
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, snapDistance, mountLayerMask);
            
            GameObject nearestMount = null;
            float nearestDistance = snapDistance;
            
            foreach (var col in nearbyColliders)
            {
                if (col.CompareTag("Mount") || col.name.Contains("Mount_"))
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestMount = col.gameObject;
                    }
                }
            }
            
            if (nearestMount != null)
            {
                SnapToMount(nearestMount);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!grabInteractable.isSelected && !isPlaced)
            {
                if (other.CompareTag("Mount") || other.name.Contains("Mount_"))
                {
                    SnapToMount(other.gameObject);
                }
            }
        }
        
        private void SnapToMount(GameObject mount)
        {
            if (isPlaced && currentMount == mount.name)
            {
                return; // Already placed here
            }
            
            if (isPlaced)
            {
                RemoveFromMount();
            }
            
            currentMount = mount.name;
            currentMountObject = mount;
            isPlaced = true;
            
            // Snap position and rotation
            transform.position = mount.transform.position;
            transform.rotation = mount.transform.rotation;
            
            // Lock in place
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // Disable grab temporarily (optional)
            grabInteractable.enabled = false;
            Invoke(nameof(ReenableGrab), 0.5f);
            
            // Play sound effect
            PlayAttachSound();
            
            // Report to GameState
            if (GameState.Instance != null)
            {
                GameState.Instance.ReportPadPlaced(currentMount);
            }
            
            OnPlaced?.Invoke(currentMount);
            
            Debug.Log($"Electrode pad snapped to {currentMount}");
        }
        
        private void RemoveFromMount()
        {
            if (!isPlaced) return;
            
            string previousMount = currentMount;
            
            isPlaced = false;
            currentMount = "";
            currentMountObject = null;
            
            // Restore physics
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            
            // Report to GameState
            if (GameState.Instance != null)
            {
                GameState.Instance.ReportPadRemoved(previousMount);
            }
            
            OnRemoved?.Invoke(previousMount);
            
            Debug.Log($"Electrode pad removed from {previousMount}");
        }
        
        private void ReenableGrab()
        {
            grabInteractable.enabled = true;
        }
        
        private void SpawnPeeledBacking()
        {
            // Check if this is the first grab (has backing)
            Transform backing = transform.Find("Backing");
            if (backing != null)
            {
                backing.gameObject.SetActive(false);
                
                // Try to spawn peeled backing model
                GameObject peeledPrefab = Resources.Load<GameObject>("EKG Backing Peeled");
#if UNITY_EDITOR
                if (peeledPrefab == null)
                {
                    // Try from assets folder (Editor-only)
                    peeledPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/3DModelsElectrode/EKG Backing Peeled.fbx");
                }
#endif
                
                if (peeledPrefab != null)
                {
                    GameObject peeled = Instantiate(peeledPrefab, transform.position - Vector3.up * 0.05f, transform.rotation);
                    peeled.name = "Peeled_Backing";
                    
                    // Add physics and grab
                    Rigidbody peeledRb = peeled.AddComponent<Rigidbody>();
                    peeledRb.mass = 0.01f;
                    peeled.AddComponent<BoxCollider>();
                    
                    XRGrabInteractable peeledGrab = peeled.AddComponent<XRGrabInteractable>();
                    peeledGrab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                    peeledGrab.throwOnDetach = true;
                    
                    // Apply small force to separate
                    peeledRb.AddForce(Vector3.down * 2f + Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
                }
            }
        }
        
        private void PlayAttachSound()
        {
            AudioSource audio = GetComponent<AudioSource>();
            if (audio == null)
            {
                audio = gameObject.AddComponent<AudioSource>();
            }
            
            if (audio.clip != null)
            {
                audio.Play();
            }
            else
            {
                // Play a default click sound if available
                audio.PlayOneShot(audio.clip);
            }
        }
        
        public void ResetToOriginalPosition()
        {
            if (isPlaced)
            {
                RemoveFromMount();
            }
            
            transform.SetParent(originalParent);
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
            }
        }
        
        public bool IsPlaced() => isPlaced;
        public string GetCurrentMount() => currentMount;
    }
}
