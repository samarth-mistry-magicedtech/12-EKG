using MagicEd.Core;
using R3;
using UnityEngine;

public class ElectrodesModel : SequenceModel
{
	public SerializableReactiveProperty<bool> LASticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> LALead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> RASticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> RALead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> LLSticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> LLLead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> RLSticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> RLLead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V1Sticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V1Lead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V2Sticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V2Lead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V3Sticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V3Lead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V4Sticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V4Lead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V5Sticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V5Lead = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V6Sticker = new SerializableReactiveProperty<bool>(false);
	public SerializableReactiveProperty<bool> V6Lead = new SerializableReactiveProperty<bool>(false);

	public SerializableReactiveProperty<bool> EKGRunning = new SerializableReactiveProperty<bool>(false);

	public BehaviorSubject<bool> StickersDone = new BehaviorSubject<bool>(false);
	[SerializeField, ReadOnlyInspector]
	private bool m_StickersDone;//just for monitoring in inspector

	public BehaviorSubject<bool> LeadsDone = new BehaviorSubject<bool>(false);
	[SerializeField, ReadOnlyInspector]
	private bool m_LeadsDone;//just for monitoring in inspector

	private void Start()
	{
		//inspector visualization
		StickersDone.Subscribe(x => m_StickersDone = x).AddTo(this);
		LeadsDone.Subscribe(x => m_LeadsDone = x).AddTo(this);
	}

	public override AsyncOperation LoadScene()
	{
		//horrific
		disposables.Add(Observable.CombineLatest(
				LASticker,
				LLSticker,
				RASticker,
				RLSticker,
				V1Sticker,
				V2Sticker,
				V3Sticker,
				V4Sticker,
				V5Sticker,
				V6Sticker,
				(LA, LL, RA, RL, v1, v2, v3, v4, v5, v6) => LA && LL && RA && RL && v1 && v2 && v3 && v4 && v5 && v6)
			.Subscribe(StickersDone.OnNext));

		disposables.Add(Observable.CombineLatest(
				LALead,
				LLLead,
				RALead,
				RLLead,
				V1Lead,
				V2Lead,
				V3Lead,
				V4Lead,
				V5Lead,
				V6Lead,
				(LA, LL, RA, RL, v1, v2, v3, v4, v5, v6) => LA && LL && RA && RL && v1 && v2 && v3 && v4 && v5 && v6)
			.Subscribe(LeadsDone.OnNext));

		return base.LoadScene();
	}
}
