using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SB12
{
    /// <summary>
    /// Controls gravity for electrode pads to prevent them from falling through the floor
    /// while still allowing realistic physics when grabbed and released
    /// </summary>
    public class PadGravityController : MonoBehaviour
    {
        private Rigidbody rb;
        private XRGrabInteractable grabInteractable;
        private bool wasGrabbed = false;
        
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            grabInteractable = GetComponent<XRGrabInteractable>();
            
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.AddListener(OnGrabbed);
                grabInteractable.selectExited.AddListener(OnReleased);
            }
        }
        
        void OnDestroy()
        {
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.RemoveListener(OnGrabbed);
                grabInteractable.selectExited.RemoveListener(OnReleased);
            }
        }
        
        private void OnGrabbed(SelectEnterEventArgs args)
        {
            if (rb != null)
            {
                rb.useGravity = false; // Disable gravity while being held
                wasGrabbed = true;
            }
        }
        
        private void OnReleased(SelectExitEventArgs args)
        {
            if (rb != null && wasGrabbed)
            {
                // Enable gravity only after being grabbed at least once
                rb.useGravity = true;
                
                // Add slight upward velocity to prevent immediate falling through surfaces
                rb.velocity = new Vector3(rb.velocity.x, Mathf.Max(rb.velocity.y, 0.1f), rb.velocity.z);
            }
        }
        
        void FixedUpdate()
        {
            // Safety check: if pad falls below floor level, reset position
            if (transform.position.y < -1f)
            {
                ResetToTray();
            }
        }
        
        private void ResetToTray()
        {
            // Find tray and reset pad position
            Transform tray = GameObject.Find("RoomRoot/TrayTable/Tray")?.transform;
            if (tray == null) tray = GameObject.Find("RoomRoot/Cart/Tray")?.transform;
            
            if (tray != null)
            {
                transform.position = tray.position + Vector3.up * 0.1f;
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.useGravity = false; // Reset gravity state
                }
                wasGrabbed = false;
                Debug.Log($"[SB12] Reset {gameObject.name} to tray position");
            }
        }
    }
}
