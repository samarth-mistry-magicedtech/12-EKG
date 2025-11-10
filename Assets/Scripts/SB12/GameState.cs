using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SB12
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }
        
        [Header("State")]
        [SerializeField] private HashSet<string> placedMounts = new HashSet<string>();
        [SerializeField] private bool allPadsPlaced = false;
        [SerializeField] private bool powerEnabled = false;
        
        [Header("Events")]
        public UnityEvent<string> OnPadPlaced = new UnityEvent<string>();
        public UnityEvent OnAllPadsPlaced = new UnityEvent();
        public UnityEvent OnPowerEnabled = new UnityEvent();
        
        [Header("References")]
        [SerializeField] private GameObject powerButton;
        [SerializeField] private GameObject waveformPanel;
        [SerializeField] private SlideController slideController;
        
        private const int TOTAL_PADS = 10;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Find references if not set
            if (powerButton == null)
                powerButton = GameObject.Find("RoomRoot/Cart/PowerButton");
            
            if (slideController == null)
                slideController = FindObjectOfType<SlideController>();
            
            // Initially disable power button
            if (powerButton != null)
            {
                var renderer = powerButton.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetColor("_EmissionColor", Color.black);
                }
                
                var interactable = powerButton.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable>();
                if (interactable != null)
                {
                    interactable.enabled = false;
                }
            }
        }
        
        public void ReportPadPlaced(string mountName)
        {
            if (placedMounts.Contains(mountName))
            {
                Debug.Log($"Mount {mountName} already has a pad.");
                return;
            }
            
            placedMounts.Add(mountName);
            OnPadPlaced?.Invoke(mountName);
            
            Debug.Log($"Pad placed on {mountName}. Total: {placedMounts.Count}/{TOTAL_PADS}");
            
            if (placedMounts.Count >= TOTAL_PADS && !allPadsPlaced)
            {
                allPadsPlaced = true;
                OnAllPadsComplete();
            }
        }
        
        public void ReportPadRemoved(string mountName)
        {
            if (placedMounts.Contains(mountName))
            {
                placedMounts.Remove(mountName);
                Debug.Log($"Pad removed from {mountName}. Total: {placedMounts.Count}/{TOTAL_PADS}");
                
                if (allPadsPlaced && placedMounts.Count < TOTAL_PADS)
                {
                    allPadsPlaced = false;
                    DisablePowerButton();
                }
            }
        }
        
        private void OnAllPadsComplete()
        {
            Debug.Log("All electrode pads placed!");
            OnAllPadsPlaced?.Invoke();
            
            // Notify slide controller
            if (slideController != null)
            {
                slideController.OnConditionMet("PadsPlaced", TOTAL_PADS);
            }
        }
        
        public void EnablePowerButton()
        {
            if (powerButton != null && allPadsPlaced)
            {
                powerEnabled = true;
                
                // Enable glow effect
                var renderer = powerButton.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetColor("_EmissionColor", Color.green * 2f);
                    renderer.material.EnableKeyword("_EMISSION");
                }
                
                // Enable interaction
                var interactable = powerButton.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable>();
                if (interactable != null)
                {
                    interactable.enabled = true;
                    interactable.selectEntered.AddListener((args) => OnPowerButtonPressed());
                }
                
                OnPowerEnabled?.Invoke();
            }
        }
        
        private void DisablePowerButton()
        {
            if (powerButton != null)
            {
                powerEnabled = false;
                
                var renderer = powerButton.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetColor("_EmissionColor", Color.black);
                }
                
                var interactable = powerButton.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable>();
                if (interactable != null)
                {
                    interactable.enabled = false;
                }
            }
        }
        
        private void OnPowerButtonPressed()
        {
            Debug.Log("Power button pressed! Showing waveforms.");
            
            if (waveformPanel == null)
            {
                // Create a simple waveform display panel
                GameObject canvas = GameObject.Find("RoomRoot/UI/SB12_Panel");
                if (canvas != null)
                {
                    waveformPanel = new GameObject("WaveformPanel");
                    waveformPanel.transform.SetParent(canvas.transform);
                    
                    var rect = waveformPanel.AddComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.1f, 0.3f);
                    rect.anchorMax = new Vector2(0.9f, 0.7f);
                    rect.sizeDelta = Vector2.zero;
                    
                    var image = waveformPanel.AddComponent<UnityEngine.UI.Image>();
                    image.color = new Color(0, 0.2f, 0, 0.9f);
                    
                    var text = new GameObject("WaveText");
                    text.transform.SetParent(waveformPanel.transform);
                    var textComp = text.AddComponent<UnityEngine.UI.Text>();
                    textComp.text = "♥ EKG WAVEFORM ACTIVE ♥\n━━━━━━━━━━━━━━━━━━";
                    textComp.fontSize = 4;
                    textComp.alignment = TextAnchor.MiddleCenter;
                    textComp.color = Color.green;
                    
                    var textRect = text.GetComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.sizeDelta = Vector2.zero;
                }
            }
            else
            {
                waveformPanel.SetActive(true);
            }
        }
        
        public bool ArePadsPlaced() => allPadsPlaced;
        public int GetPlacedCount() => placedMounts.Count;
        public bool IsPowerEnabled() => powerEnabled;
    }
}
