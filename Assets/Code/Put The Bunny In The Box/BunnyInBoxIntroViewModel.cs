using UnityEngine;
using MagicEd.Core;
using UnityEngine.UI;
using UnityEngine.Events;
using Yarn.Unity;

public class BunnyInBoxIntroViewModel : SequenceViewModel
{
	[SerializeField]
	private Canvas overlayCanvas;
	[SerializeField]
	private Canvas worldCanvas;

	private FixedCameraController fixedController;

	protected override void SetPlatform(PlatformSelector.MagicPlatform platform)
	{
		overlayCanvas.gameObject.SetActive(platform == PlatformSelector.MagicPlatform.FixedPancake
			|| platform == PlatformSelector.MagicPlatform.FPPancake);
		worldCanvas.gameObject.SetActive(platform == PlatformSelector.MagicPlatform.XR);

		//For things like camera behaviors that are consistent between sequences but not between sims,
		//consider inheriting from an interstitial type between the base view model and the specific sequence VM
		//Otherwise dry violations like this will become common
		if (platform == PlatformSelector.MagicPlatform.FixedPancake)
		{
			fixedController = Runner.Instance.AcquirePlatformAvatar().GetComponentInChildren<FixedCameraController>();
			fixedController.RangeStart = "Start";
			fixedController.RangeEnd = "Other";
		}
	}

	public void SetCameraPose(string key)
	{
		fixedController.GoToLocation(key);
	}

	public void SetCameraPoseNext()
	{
		fixedController.Next();
	}

	[YarnCommand("Enter")]
	public void Enter()
	{
		Runner.Instance.CompleteSequence();
	}
}
