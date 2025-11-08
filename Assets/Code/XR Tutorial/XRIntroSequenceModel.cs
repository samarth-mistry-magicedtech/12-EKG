using MagicEd.Core;
using R3;
using UnityEngine;

public class XRIntroSequenceModel : SequenceModel
{
	[ReadOnlyInspector]
	public SerializableReactiveProperty<bool> finishedDialogue = new SerializableReactiveProperty<bool>(false);

	public override AsyncOperation LoadScene()
	{
		return base.LoadScene();
	}
}
