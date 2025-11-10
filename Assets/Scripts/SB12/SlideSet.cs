using System;
using System.Collections.Generic;
using UnityEngine;

namespace SB12
{
    [CreateAssetMenu(fileName = "SlideSet", menuName = "SB12/Slide Set", order = 1)]
    public class SlideSet : ScriptableObject
    {
        [SerializeField] private List<Slide> slides = new List<Slide>();
        
        public List<Slide> Slides => slides;
        
        [System.Serializable]
        public class Slide
        {
            [Header("Identification")]
            public string id = "";
            public string name = "";
            
            [Header("Content")]
            [TextArea(2, 3)]
            public string title = "";
            
            [TextArea(5, 10)]
            public string body = "";
            
            [TextArea(1, 2)]
            public string footer = "Press Continue";
            
            [Header("Actions")]
            public AdvanceAction onAdvanceAction = AdvanceAction.None;
            public GameObject[] objectsToActivate;
            public GameObject[] objectsToDeactivate;
            
            [Header("Conditions")]
            public ConditionType conditionType = ConditionType.None;
            public string conditionKey = "";
            public int conditionIntValue = 0;
            public bool conditionBoolValue = false;
            
            [Header("Visual")]
            public Sprite backgroundImage;
            public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            public Color textColor = Color.white;
        }
        
        public enum AdvanceAction
        {
            None,
            ActivateEquipment,
            EnablePads,
            EnableWires,
            EnablePowerButton,
            ShowWaveform,
            CompleteTraining,
            Custom
        }
        
        public enum ConditionType
        {
            None,
            PadsPlaced,
            PowerEnabled,
            WaveformActive,
            Custom
        }
        
        public void InitializeDefaultSlides()
        {
            slides.Clear();
            
            // Slide 1: Task Introduction
            slides.Add(new Slide {
                id = "slide_01_intro",
                name = "Task Introduction",
                title = "SB12 EKG Training",
                body = "In this training, you will learn to properly place electrodes for a 10-lead EKG.\n\n" +
                       "You'll be placing electrodes on specific locations on the patient's chest and limbs.\n\n" +
                       "Follow the instructions carefully and use the visual markers as guides.",
                footer = "Press Continue to begin",
                onAdvanceAction = AdvanceAction.None
            });
            
            // Slide 2: Skin Preparation
            slides.Add(new Slide {
                id = "slide_02_prep",
                name = "Skin Preparation",
                title = "Preparing the Skin",
                body = "Before placing electrodes, ensure the patient's skin is properly prepared:\n\n" +
                       "• Clean the area with alcohol wipes\n" +
                       "• Allow the skin to dry completely\n" +
                       "• Shave excessive hair if necessary\n" +
                       "• Avoid areas with skin irritation or wounds\n\n" +
                       "Proper skin preparation ensures good electrode adhesion and signal quality.",
                footer = "Press Continue"
            });
            
            // Slide 3: Adhesion Tips
            slides.Add(new Slide {
                id = "slide_03_adhesion",
                name = "Adhesion Tips",
                title = "Ensuring Good Contact",
                body = "For optimal signal quality:\n\n" +
                       "• Remove the adhesive backing just before placement\n" +
                       "• Press firmly on the center of each electrode for 5-10 seconds\n" +
                       "• Ensure all edges are sealed to the skin\n" +
                       "• Avoid placing electrodes over bones or joints\n" +
                       "• Replace electrodes that don't adhere properly",
                footer = "Press Continue"
            });
            
            // Slide 4: Equipment Activation
            slides.Add(new Slide {
                id = "slide_04_equipment",
                name = "Equipment Ready",
                title = "Equipment Overview",
                body = "The EKG equipment is now ready for use.\n\n" +
                       "You can see:\n" +
                       "• The EKG machine on the cart\n" +
                       "• 10 electrode pads on the tray\n" +
                       "• Yellow markers for limb placement (RA, LA, RL, LL)\n" +
                       "• Green markers for chest placement (V1-V6)\n\n" +
                       "The markers indicate the correct placement positions.",
                footer = "Press Continue to start placing electrodes",
                onAdvanceAction = AdvanceAction.ActivateEquipment
            });
            
            // Slide 5: Electrode Locations
            slides.Add(new Slide {
                id = "slide_05_locations",
                name = "Placement Guide",
                title = "Electrode Placement Locations",
                body = "LIMB ELECTRODES (Yellow Markers):\n" +
                       "• RA (Right Arm): Right shoulder/upper arm\n" +
                       "• LA (Left Arm): Left shoulder/upper arm\n" +
                       "• RL (Right Leg): Right lower abdomen\n" +
                       "• LL (Left Leg): Left lower abdomen\n\n" +
                       "CHEST ELECTRODES (Green Markers):\n" +
                       "• V1: 4th intercostal space, right sternal border\n" +
                       "• V2: 4th intercostal space, left sternal border\n" +
                       "• V3: Between V2 and V4\n" +
                       "• V4: 5th intercostal space, midclavicular line\n" +
                       "• V5: Anterior axillary line, level with V4\n" +
                       "• V6: Midaxillary line, level with V4",
                footer = "Grab electrodes from the tray and place them",
                onAdvanceAction = AdvanceAction.EnablePads
            });
            
            // Slide 6: Interaction Instructions
            slides.Add(new Slide {
                id = "slide_06_interaction",
                name = "How to Place",
                title = "Placing the Electrodes",
                body = "To place an electrode:\n\n" +
                       "1. Grab an electrode pad from the tray\n" +
                       "2. The backing will automatically peel away\n" +
                       "3. Move the pad near a placement marker\n" +
                       "4. The pad will snap into place when close enough\n" +
                       "5. You'll hear a confirmation sound\n\n" +
                       "If you need to reposition, simply grab and move the pad again.",
                footer = "Continue placing all 10 electrodes"
            });
            
            // Slide 7: Placement Order
            slides.Add(new Slide {
                id = "slide_07_order",
                name = "Suggested Order",
                title = "Recommended Placement Order",
                body = "While any order works, many practitioners follow this sequence:\n\n" +
                       "1. Start with limb leads for baseline:\n" +
                       "   • RA → LA → RL → LL\n\n" +
                       "2. Then place chest leads in order:\n" +
                       "   • V1 → V2 → V3 → V4 → V5 → V6\n\n" +
                       "This systematic approach helps ensure no electrodes are missed.",
                footer = "Continue placing electrodes"
            });
            
            // Slide 8: Verification (Conditional)
            slides.Add(new Slide {
                id = "slide_08_verify",
                name = "Verify Placement",
                title = "Excellent Work!",
                body = "All 10 electrode pads are now in place!\n\n" +
                       "Before proceeding, verify that:\n" +
                       "• Each electrode is firmly attached\n" +
                       "• All markers have been covered\n" +
                       "• The pads are properly positioned\n\n" +
                       "Next, we'll power on the EKG machine.",
                footer = "Press Continue",
                conditionType = ConditionType.PadsPlaced,
                conditionIntValue = 10
            });
            
            // Slide 9: Power On (Conditional)
            slides.Add(new Slide {
                id = "slide_09_power",
                name = "Power On",
                title = "Activate the EKG Machine",
                body = "The EKG machine is ready to be powered on.\n\n" +
                       "Notice the power button is now glowing green.\n\n" +
                       "Press the power button to:\n" +
                       "• Initialize the EKG system\n" +
                       "• Begin signal acquisition\n" +
                       "• Display the cardiac waveforms",
                footer = "Press the green power button",
                onAdvanceAction = AdvanceAction.EnablePowerButton,
                conditionType = ConditionType.PadsPlaced,
                conditionIntValue = 10
            });
            
            // Slide 10: Monitoring Active
            slides.Add(new Slide {
                id = "slide_10_monitoring",
                name = "Monitoring Active",
                title = "EKG Recording Active",
                body = "The EKG is now actively monitoring!\n\n" +
                       "The display shows:\n" +
                       "• Real-time cardiac waveforms\n" +
                       "• Heart rate measurement\n" +
                       "• Signal quality indicators\n\n" +
                       "All 10 leads are providing data for comprehensive cardiac assessment.",
                footer = "Press Continue"
            });
            
            // Slide 11: Success
            slides.Add(new Slide {
                id = "slide_11_success",
                name = "Success",
                title = "Congratulations!",
                body = "You have successfully completed the 10-lead EKG setup!\n\n" +
                       "Your proper electrode placement ensures:\n" +
                       "• Accurate cardiac monitoring\n" +
                       "• Clear signal quality\n" +
                       "• Reliable diagnostic data\n\n" +
                       "This skill is essential for cardiac care and emergency medicine.",
                footer = "Press Continue"
            });
            
            // Slide 12: Key Takeaways
            slides.Add(new Slide {
                id = "slide_12_review",
                name = "Key Takeaways",
                title = "Important Reminders",
                body = "Remember these key points:\n\n" +
                       "• Always prepare the skin properly\n" +
                       "• Follow anatomical landmarks for placement\n" +
                       "• Ensure firm electrode adhesion\n" +
                       "• Check all connections before recording\n" +
                       "• Replace electrodes if signal quality is poor\n" +
                       "• Document any placement variations\n\n" +
                       "Regular practice improves speed and accuracy.",
                footer = "Press Continue"
            });
            
            // Slide 13: Training Complete
            slides.Add(new Slide {
                id = "slide_13_complete",
                name = "Training Complete",
                title = "Training Session Complete",
                body = "Thank you for completing the SB12 EKG Electrode Placement Training.\n\n" +
                       "You have demonstrated proficiency in:\n" +
                       "• Identifying correct electrode positions\n" +
                       "• Proper placement technique\n" +
                       "• EKG system operation\n\n" +
                       "Feel free to practice again or exit the simulation.",
                footer = "End Training",
                onAdvanceAction = AdvanceAction.CompleteTraining
            });
        }
        
        public Slide GetSlide(int index)
        {
            if (index >= 0 && index < slides.Count)
                return slides[index];
            return null;
        }
        
        public Slide GetSlideById(string id)
        {
            return slides.Find(s => s.id == id);
        }
        
        public int GetSlideIndex(string id)
        {
            for (int i = 0; i < slides.Count; i++)
            {
                if (slides[i].id == id)
                    return i;
            }
            return -1;
        }
    }
}
