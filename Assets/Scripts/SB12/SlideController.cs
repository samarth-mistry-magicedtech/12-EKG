using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SB12
{
    public class SlideController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text footerText;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject slidePanel;
        
        [Header("Slide Data")]
        [SerializeField] private List<SlideData> slides = new List<SlideData>();
        [SerializeField] private int currentSlideIndex = 0;
        
        [Header("Object References")]
        [SerializeField] private GameObject equipment;
        [SerializeField] private GameObject[] electroPads;
        [SerializeField] private GameObject powerButton;
        
        private Dictionary<string, object> conditions = new Dictionary<string, object>();
        
        [System.Serializable]
        public class SlideData
        {
            public string id = "";
            public string title = "";
            public string body = "";
            public string footer = "";
            public SlideAction onAdvanceAction = SlideAction.None;
            public string conditionKey = "";
            public int conditionValue = 0;
        }
        
        public enum SlideAction
        {
            None,
            ActivateEquipment,
            EnablePads,
            EnableWires,
            EnablePowerButton
        }
        
        private void Awake()
        {
            // Initialize default slides if empty
            if (slides.Count == 0)
            {
                InitializeDefaultSlides();
            }
        }
        
        private void Start()
        {
            // Find UI references if not set
            if (slidePanel == null)
                slidePanel = GameObject.Find("RoomRoot/UI/SB12_Panel");
            
            if (titleText == null && slidePanel != null)
                titleText = slidePanel.transform.Find("Title")?.GetComponent<Text>();
            
            if (bodyText == null && slidePanel != null)
                bodyText = slidePanel.transform.Find("BodyText")?.GetComponent<Text>();
            
            if (continueButton == null && slidePanel != null)
                continueButton = slidePanel.transform.Find("ContinueButton")?.GetComponent<Button>();
            
            // Find game objects
            if (equipment == null)
                equipment = GameObject.Find("RoomRoot/Cart");
            
            if (powerButton == null)
                powerButton = GameObject.Find("RoomRoot/Cart/PowerButton");
            
            // Setup button listener
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(NextSlide);
            }
            
            // Show first slide
            ShowCurrentSlide();
        }
        
        private void InitializeDefaultSlides()
        {
            slides.Clear();
            
            // Slide 1: Task Introduction
            slides.Add(new SlideData {
                id = "intro",
                title = "SB12 EKG Training",
                body = "In this training, you will learn to properly place electrodes for a 10-lead EKG.\n\nYou'll be placing electrodes on specific locations on the patient's chest and limbs.",
                footer = "Press Continue to begin."
            });
            
            // Slide 2: Skin Preparation
            slides.Add(new SlideData {
                id = "skin_prep",
                title = "Skin Preparation",
                body = "Before placing electrodes, ensure the patient's skin is clean and dry.\n\nIn a real scenario, you would:\n• Clean the area with alcohol wipes\n• Allow to dry completely\n• Shave hair if necessary",
                footer = "Press Continue"
            });
            
            // Slide 3: Adhesion Tip
            slides.Add(new SlideData {
                id = "adhesion",
                title = "Proper Adhesion",
                body = "For best signal quality:\n\n• Press firmly on the center of each electrode\n• Ensure edges are sealed\n• Avoid placing over bones or tendons",
                footer = "Press Continue"
            });
            
            // Slide 4: Equipment Appears
            slides.Add(new SlideData {
                id = "equipment",
                title = "Equipment Ready",
                body = "The EKG cart with electrodes is now available.\n\nYou'll see:\n• 10 electrode pads on the tray\n• The EKG machine\n• Target markers on the patient",
                footer = "Press Continue",
                onAdvanceAction = SlideAction.ActivateEquipment
            });
            
            // Slide 5: Placement Locations
            slides.Add(new SlideData {
                id = "locations",
                title = "Electrode Placement",
                body = "Place electrodes at the marked locations:\n\nLIMBS (Yellow markers):\n• RA - Right arm\n• LA - Left arm\n• RL - Right leg\n• LL - Left leg\n\nCHEST (Green markers):\n• V1-V6 across the chest",
                footer = "Grab and place the electrodes",
                onAdvanceAction = SlideAction.EnablePads
            });
            
            // Slide 6: Peel Backing
            slides.Add(new SlideData {
                id = "peel",
                title = "Remove Backing",
                body = "When you grab an electrode pad:\n\n• The backing will peel away automatically\n• Place the sticky side on the skin marker\n• The pad will snap into place when close enough",
                footer = "Continue placing electrodes"
            });
            
            // Slide 7: Order Tip
            slides.Add(new SlideData {
                id = "order",
                title = "Placement Order",
                body = "While there's no strict order required, many practitioners prefer:\n\n1. Start with limb leads (RA, LA, RL, LL)\n2. Then place chest leads (V1-V6)\n\nThis helps establish a baseline before chest placement.",
                footer = "Continue placing electrodes"
            });
            
            // Slide 8: Verify Prep (conditional)
            slides.Add(new SlideData {
                id = "verify",
                title = "Almost Done!",
                body = "Excellent work!\n\nAll 10 electrode pads are now in place.\n\nNext, we'll verify the connections and power on the EKG machine.",
                footer = "Press Continue",
                conditionKey = "PadsPlaced",
                conditionValue = 10
            });
            
            // Slide 9: Power On (conditional)
            slides.Add(new SlideData {
                id = "power",
                title = "Power On",
                body = "The power button is now active (glowing green).\n\nPress the power button on the EKG machine to begin monitoring.",
                footer = "Press the power button",
                onAdvanceAction = SlideAction.EnablePowerButton,
                conditionKey = "PadsPlaced",
                conditionValue = 10
            });
            
            // Slide 10: Setup Complete
            slides.Add(new SlideData {
                id = "complete",
                title = "Setup Complete",
                body = "The EKG is now recording!\n\nYou can see the waveform on the display.\n\nThe machine is monitoring all 10 leads.",
                footer = "Press Continue"
            });
            
            // Slide 11: Success
            slides.Add(new SlideData {
                id = "success",
                title = "Congratulations!",
                body = "You have successfully placed all electrodes for a 10-lead EKG.\n\nProper electrode placement ensures accurate cardiac monitoring.",
                footer = "Press Continue"
            });
            
            // Slide 12: Key Points
            slides.Add(new SlideData {
                id = "review",
                title = "Key Points",
                body = "Remember:\n\n• Clean and dry skin before placement\n• Press firmly for good adhesion\n• Check all connections before starting\n• Monitor for loose electrodes during recording",
                footer = "Press Continue"
            });
            
            // Slide 13: Exit
            slides.Add(new SlideData {
                id = "exit",
                title = "Training Complete",
                body = "Thank you for completing the SB12 EKG electrode placement training.\n\nYou may now exit the simulation or practice again.",
                footer = "End Training"
            });
        }
        
        public void ShowCurrentSlide()
        {
            if (currentSlideIndex >= slides.Count)
            {
                Debug.Log("All slides completed");
                return;
            }
            
            SlideData slide = slides[currentSlideIndex];
            
            // Check conditions
            if (!string.IsNullOrEmpty(slide.conditionKey))
            {
                if (!CheckCondition(slide.conditionKey, slide.conditionValue))
                {
                    // Skip this slide if condition not met
                    currentSlideIndex++;
                    ShowCurrentSlide();
                    return;
                }
            }
            
            // Update UI
            if (titleText != null)
                titleText.text = slide.title;
            
            if (bodyText != null)
                bodyText.text = slide.body;
            
            if (footerText != null)
                footerText.text = slide.footer;
            else if (continueButton != null)
            {
                Text btnText = continueButton.GetComponentInChildren<Text>();
                if (btnText != null)
                    btnText.text = string.IsNullOrEmpty(slide.footer) ? "Continue" : 
                                   slide.footer.Contains("Continue") ? "Continue" : "Continue";
            }
            
            Debug.Log($"Showing slide {currentSlideIndex + 1}/{slides.Count}: {slide.title}");
        }
        
        public void NextSlide()
        {
            if (currentSlideIndex >= slides.Count) return;
            
            // Execute action for current slide before advancing
            SlideData currentSlide = slides[currentSlideIndex];
            ExecuteSlideAction(currentSlide.onAdvanceAction);
            
            // Move to next slide
            currentSlideIndex++;
            
            if (currentSlideIndex >= slides.Count)
            {
                Debug.Log("Training completed!");
                // Optional: Hide panel or show completion screen
            }
            else
            {
                ShowCurrentSlide();
            }
        }
        
        private void ExecuteSlideAction(SlideAction action)
        {
            switch (action)
            {
                case SlideAction.ActivateEquipment:
                    if (equipment != null)
                    {
                        equipment.SetActive(true);
                        // Show mount markers
                        GameObject mounts = GameObject.Find("RoomRoot/Bed/Patient/PatientAnchor/Mounts");
                        if (mounts != null)
                        {
                            foreach (Transform mount in mounts.transform)
                            {
                                Transform marker = mount.Find("Marker");
                                if (marker != null)
                                    marker.gameObject.SetActive(true);
                            }
                        }
                    }
                    Debug.Log("Equipment activated");
                    break;
                    
                case SlideAction.EnablePads:
                    GameObject pads = GameObject.Find("RoomRoot/Cart/Tray/Pads");
                    if (pads != null)
                    {
                        foreach (Transform pad in pads.transform)
                        {
                            var grab = pad.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
                            if (grab != null)
                                grab.enabled = true;
                            
                            // Add PadPlacement script if missing
                            if (pad.GetComponent<PadPlacement>() == null)
                                pad.gameObject.AddComponent<PadPlacement>();
                        }
                    }
                    Debug.Log("Electrode pads enabled for interaction");
                    break;
                    
                case SlideAction.EnableWires:
                    // Placeholder for wire visualization
                    Debug.Log("Wire connections would be shown here");
                    break;
                    
                case SlideAction.EnablePowerButton:
                    if (GameState.Instance != null)
                    {
                        GameState.Instance.EnablePowerButton();
                    }
                    Debug.Log("Power button enabled");
                    break;
            }
        }
        
        private bool CheckCondition(string key, object value)
        {
            if (conditions.ContainsKey(key))
            {
                if (value is int intValue)
                {
                    return conditions[key] is int condInt && condInt >= intValue;
                }
                return conditions[key].Equals(value);
            }
            
            // Check with GameState
            if (key == "PadsPlaced" && GameState.Instance != null)
            {
                return GameState.Instance.GetPlacedCount() >= (int)value;
            }
            
            return false;
        }
        
        public void OnConditionMet(string key, object value)
        {
            conditions[key] = value;
            
            // Check if we should advance to a conditional slide
            if (currentSlideIndex < slides.Count)
            {
                SlideData nextSlide = slides[currentSlideIndex];
                if (nextSlide.conditionKey == key)
                {
                    ShowCurrentSlide();
                }
            }
        }
        
        public void JumpToSlide(string slideId)
        {
            for (int i = 0; i < slides.Count; i++)
            {
                if (slides[i].id == slideId)
                {
                    currentSlideIndex = i;
                    ShowCurrentSlide();
                    break;
                }
            }
        }
        
        public int GetCurrentSlideIndex() => currentSlideIndex;
        public int GetTotalSlides() => slides.Count;
    }
}
