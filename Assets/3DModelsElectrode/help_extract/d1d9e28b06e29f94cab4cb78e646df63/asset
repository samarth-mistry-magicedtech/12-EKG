using MagicEd.Core;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;
using R3;

public class ElectrodesViewModel : SequenceViewModel
{
	[System.Serializable]
	public enum LeadIndex
	{
		LA = 0,
		RA = 1,
		LL = 2,
		RL = 3,
		V1 = 4,
		V2 = 5,
		V3 = 6,
		V4 = 7,
		V5 = 8,
		V6 = 9
	}

	public UnityEvent DoPrep;
	public UnityEvent DoStickers;
	public UnityEvent DoLeads;
	public UnityEvent DoEKG;
	public UnityEvent DoDone;

	private ElectrodesModel CastModel;

	protected override void SetPlatform(PlatformSelector.MagicPlatform platform)
	{
		CastModel = model as ElectrodesModel;
		CastModel.StickersDone.Where(d => d).Take(1).Subscribe(_ => StickersDone());
		CastModel.LeadsDone.SkipWhile(_ => !CastModel.StickersDone.Value).Subscribe(_ => BuildMissingLeadString());
		CastModel.LeadsDone.Where(d => d).Take(1).Subscribe(_ => LeadsDone());
	}

	[YarnCommand("DoPrep")]
	public void Prep()
	{
		RunDialogue("Prep");
		DoPrep.Invoke();
	}

	[YarnCommand("DoStickers")]
	public void Stickers()
	{
		SetPermanentInstructions("Peel the back off the pads and apply them to the patient");
		RunDialogue("Stickers");
		DoStickers.Invoke();
	}

	public void DroppedObject()
	{
		CacheNode();
		RunDialogue("DroppedObject");
	}

	public void AttachSticker(int lead)
	{
		SetSticker((LeadIndex)lead, true);
	}

	public void AttachSticker(LeadIndex lead)
	{
		SetSticker(lead, true);
	}

	public void SetSticker(LeadIndex lead, bool attached)
	{
		switch (lead)
		{
			case LeadIndex.LA:
				CastModel.LASticker.Value = attached;
				break;
			case LeadIndex.RA:
				CastModel.RASticker.Value = attached;
				break;
			case LeadIndex.LL:
				CastModel.LLSticker.Value = attached;
				break;
			case LeadIndex.RL:
				CastModel.RLSticker.Value = attached;
				break;
			case LeadIndex.V1:
				CastModel.V1Sticker.Value = attached;
				break;
			case LeadIndex.V2:
				CastModel.V2Sticker.Value = attached;
				break;
			case LeadIndex.V3:
				CastModel.V3Sticker.Value = attached;
				break;
			case LeadIndex.V4:
				CastModel.V4Sticker.Value = attached;
				break;
			case LeadIndex.V5:
				CastModel.V5Sticker.Value = attached;
				break;
			case LeadIndex.V6:
				CastModel.V6Sticker.Value = attached;
				break;
			default:
				break;
		}
	}

	public void PeelFirst()
	{
		RunDialogue("PeelFirst");
	}

	private void StickersDone()
	{
		SetPermanentInstructions("Attach the correct lead to each electrode pad");
		RunDialogue("Leads");
		DoLeads.Invoke();
	}

	private void BuildMissingLeadString()
	{
		//I'm so vector math-brained this is the best i could come up with for string manip
		SetPermanentInstructions(
				"Missing or Incorrect leads: " +
				(CastModel.LALead.Value ? "" : "LA ") +
				(CastModel.LLLead.Value ? "" : "LL ") +
				(CastModel.RALead.Value ? "" : "RA ") +
				(CastModel.RLLead.Value ? "" : "RL ") +
				(CastModel.V1Lead.Value ? "" : "V1 ") +
				(CastModel.V2Lead.Value ? "" : "V2 ") +
				(CastModel.V3Lead.Value ? "" : "V3 ") +
				(CastModel.V4Lead.Value ? "" : "V4 ") +
				(CastModel.V5Lead.Value ? "" : "V5 ") +
				(CastModel.V6Lead.Value ? "" : "V6 ")
			);
	}

	public void AttachLead(int lead)
	{
		SetLead((LeadIndex)lead, true);
	}

	public void AttachLead(LeadIndex lead)
	{
		SetLead(lead, true);
	}

	public void DetachLead(int lead)
	{
		SetLead((LeadIndex)lead, false);
	}

	public void DetachLead(LeadIndex lead)
	{
		SetLead(lead, false);
	}

	public void SetLead(LeadIndex lead, bool attached)
	{
		switch (lead)
		{
			case LeadIndex.LA:
				CastModel.LALead.Value = attached;
				break;
			case LeadIndex.RA:
				CastModel.RALead.Value = attached;
				break;
			case LeadIndex.LL:
				CastModel.LLLead.Value = attached;
				break;
			case LeadIndex.RL:
				CastModel.RLLead.Value = attached;
				break;
			case LeadIndex.V1:
				CastModel.V1Lead.Value = attached;
				break;
			case LeadIndex.V2:
				CastModel.V2Lead.Value = attached;
				break;
			case LeadIndex.V3:
				CastModel.V3Lead.Value = attached;
				break;
			case LeadIndex.V4:
				CastModel.V4Lead.Value = attached;
				break;
			case LeadIndex.V5:
				CastModel.V5Lead.Value = attached;
				break;
			case LeadIndex.V6:
				CastModel.V6Lead.Value = attached;
				break;
			default:
				break;
		}
	}


	public void WrongLead()
	{
		RunDialogue("WrongLead");
	}

	private void LeadsDone()
	{
		RunDialogue("AlmostDone");
		DoEKG.Invoke();
		SetPermanentInstructions("Turn on the EKG machine");
	}

	public void EKGDone()
	{
		DoDone.Invoke();
		SetPermanentInstructions("");
		RunDialogue("EKGRunning");
	}

	[YarnCommand("Complete")]
	public void Complete()
	{
		model.OnComplete.Invoke();
	}
}
