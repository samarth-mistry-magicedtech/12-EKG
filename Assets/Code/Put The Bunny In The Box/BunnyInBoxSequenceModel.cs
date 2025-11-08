using R3;
using UnityEngine;
using UnityEditor;
using MagicEd.Core;

public class BunnyInBoxSequenceModel : SequenceModel
{
	// This might be foolish, but the sequence VM is responsible for pushing OnNext for these state subjects
	// this is because the VM is doing data transformation from the in-scene simulation stuff
	[ReadOnlyInspector]
	public SerializableReactiveProperty<bool> bunnyInBox = new SerializableReactiveProperty<bool>();

	public override AsyncOperation LoadScene()
	{
		return base.LoadScene();
	}
}
