using UnityEngine;
using MagicEd.Core;
using Yarn.Unity;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using R3;
using UnityEngine.Events;
using System.Threading;
using System.Linq;

//A lot of this tutorial was done before we really understood how to use VMs and models
//if there's ever a good reason, refactor this to use a bunch of unityevents instead of referencing scene objects
//this loosens the coupling and makes it easier to extend functionality without code change
public class XRIntroSequenceViewModel : SequenceViewModel
{
	public SimpleLookatInteractable ArchLookAt;
	public SimpleLookatInteractable TotemLookAt;
	public Transform resetPosition;
	public UnityEvent FreePlay;
	[SerializeField]
	private GameObject RuntimeLookatTarget;

	[SerializeField]
	private LocationSequencer instructionPanelLocations;
	private SimpleLookatInteractable LControllerLookAtTarget;
	private SimpleLookatInteractable RControllerLookAtTarget;

	private int resetPanelPos;
	public Camera dummyCam;


	protected override void SetPlatform(PlatformSelector.MagicPlatform platform)
	{
		if (platform != PlatformSelector.MagicPlatform.XR)
			Debug.LogError("This sequence is only intended for XR!");
	}

	protected override void Awake()
	{
		base.Awake();
		ArchLookAt.gameObject.SetActive(false);
		TotemLookAt.gameObject.SetActive(false);
	}
	private void Start()
	{
		//without the XR rig in the scene the lazy followers get annoying
		//this needs a better solution though
		if (Runner.Instance != null)
			Destroy(dummyCam.gameObject);
	}

	public void ResetPlayer()
	{
		CacheNode();
		RunDialogue("Whoops");
	}

	[YarnCommand("LookAtArch")]
	public void LookAtArch()
	{
		ArchLookAt.gameObject.SetActive(true);
	}

	[YarnCommand("LookAtTotem")]
	public void LookAtTotem()
	{
		TotemLookAt.gameObject.SetActive(true);
	}

	[YarnCommand("LookAtControllers")]
	public void LookAtControllers()
	{
		//Tight coupling to XR platform, for obvious reasons
		//this codesmell stinks, this isn't what you do with a viewmodel
		//if this becomes a problem, refactor to use unityevents to turn these on and off
		XRInputModalityManager manager = Runner.Instance.AcquirePlatformAvatar.Invoke().GetComponent<XRInputModalityManager>();
		LControllerLookAtTarget = Instantiate(RuntimeLookatTarget, manager.leftController.transform).GetComponent<SimpleLookatInteractable>();
		LControllerLookAtTarget.aimCone = .1f;
		RControllerLookAtTarget = Instantiate(RuntimeLookatTarget, manager.rightController.transform).GetComponent<SimpleLookatInteractable>();
		RControllerLookAtTarget.aimCone = .1f;


		LControllerLookAtTarget.OnLookEnter
			.AsObservable(LControllerLookAtTarget.destroyCancellationToken)
			.CombineLatest(RControllerLookAtTarget.OnLookEnter.AsObservable(RControllerLookAtTarget.destroyCancellationToken), (x, y) => (x, y))//Tuple! would rather discard
			.Debounce(System.TimeSpan.FromSeconds(2))
			.Subscribe((_) => ControllerLookSuccess());
	}

	[YarnCommand("CrossBridge")]
	public void CrossBridge()
	{
		//do I need to do anything here?
	}

	[YarnCommand("Finished")]
	public void FinishedDialogue()
	{
		(model as XRIntroSequenceModel).finishedDialogue.OnNext(true);
	}

	protected override void CacheNode()
	{
		resetPanelPos = instructionPanelLocations.pointerIndex.Value;
		Runner.Instance.AcquirePlatformAvatar.Invoke().transform.SetPositionAndRotation(resetPosition.position, resetPosition.rotation);
		base.CacheNode();
	}

	public override void ResumeNode()
	{
		instructionPanelLocations.pointerIndex.OnNext(resetPanelPos);
		base.ResumeNode();
	}

	public void ArchSuccess()
	{
		SetPermanentInstructions(""); //clear instructions
		ArchLookAt.gameObject.SetActive(false);
		RunDialogue("ArchSuccess");
	}

	public void TotemSuccess()
	{
		SetPermanentInstructions("");
		TotemLookAt.gameObject.SetActive(false);
		RunDialogue("TotemSuccess");
	}

	public void ControllerLookSuccess()
	{
		SetPermanentInstructions(""); 
		Destroy(LControllerLookAtTarget);
		Destroy(RControllerLookAtTarget);

		RunDialogue("Controls");
	}

	public void BridgeCrossed()
	{
		SetPermanentInstructions(""); 
		RunDialogue("Manipulation");
		FreePlay.Invoke();
	}

	public void Complete()
	{
		// make this more elegant
		Runner.Instance.CompleteSequence();
	}
}
