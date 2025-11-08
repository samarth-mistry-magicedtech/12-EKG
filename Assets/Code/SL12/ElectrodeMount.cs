using MagicEd.Core;
using UnityEngine;
using UnityEngine.Events;

public class ElectrodeMount : MonoBehaviour
{
	public ElectrodesViewModel.LeadIndex leadID;

	public UnityEvent<ElectrodesViewModel.LeadIndex> OnSticker;
	public UnityEvent<ElectrodesViewModel.LeadIndex> OnLead;
	public UnityEvent OnWrongLead;

	public void LinkSticker()
	{
		OnSticker.Invoke(leadID);
	}

	public void LinkLead(SimpleDetectable detected)
	{
		if (detected.GetComponent<ElectrodeMetadata>().leadID == leadID)
		{
			OnLead.Invoke(leadID);
		}
		else
		{
			OnWrongLead.Invoke();
		}
	}
}
