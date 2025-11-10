using R3;
using UnityEngine;
using MagicEd.Core;
using UnityEngine.Events;

public class BunnyInBoxSequenceViewModel : SequenceViewModel
{
	[SerializeField]
	private Canvas overlayCanvas;
	[SerializeField]
	private Canvas worldCanvas;

	public UnityEvent OnBunnyInBox;

	private FixedCameraController fixedController;

	protected override void SetPlatform(PlatformSelector.MagicPlatform platform)
	{
		overlayCanvas.gameObject.SetActive(platform == PlatformSelector.MagicPlatform.FixedPancake 
			|| platform == PlatformSelector.MagicPlatform.FPPancake);
		worldCanvas.gameObject.SetActive(platform == PlatformSelector.MagicPlatform.XR);

		//see intro view model
		if (platform == PlatformSelector.MagicPlatform.FixedPancake)
		{
			fixedController = Runner.Instance.AcquirePlatformAvatar().GetComponentInChildren<FixedCameraController>();
			fixedController.RangeStart = "Start";
			fixedController.RangeEnd = "Other";
		}

		(model as BunnyInBoxSequenceModel).bunnyInBox.SkipWhile(b => !b).Take(1).Subscribe(_ => OnBunnyInBox.Invoke());
	}

	public void SetCameraPose(string key)
	{
		fixedController.GoToLocation(key);
	}

	public void SetCameraPoseNext()
	{
		fixedController.Next();
	}

	public void UpdateBunnyBoxState(bool isInTheBox)
	{
		(model as BunnyInBoxSequenceModel).bunnyInBox.OnNext(isInTheBox);
	}

	public void Finish()
	{
		Runner.Instance.CompleteSequence();
	}
}
