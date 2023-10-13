using System.Collections;
using TMPro;
using UnityEngine;

public class UIPopulateJoinCode : MonoBehaviour
{
	[SerializeField] private RelayManager ugsRelayMgr;
	[SerializeField] private TextMeshProUGUI joinCodeDisplayText;
	[SerializeField] private TMP_InputField joinCodeInputText;
	[SerializeField] private SOEvent closePanelAfterDelay;
	[SerializeField] private float relayPanelCloseDelay = 3f;
	[SerializeField] private TextMeshProUGUI joinCodeToDisplayText;

	public void PopulateJoinCode()
	{
		joinCodeDisplayText.text = ugsRelayMgr.JoinCode;
		joinCodeToDisplayText.text = ugsRelayMgr.JoinCode;
	}

	public void FetchAndAssignEnteredJoinCode()
	{
		ugsRelayMgr.JoinCode = joinCodeInputText.text;
		joinCodeToDisplayText.text = ugsRelayMgr.JoinCode;
		//	joinCodeTopLabelText.text = joinCodeInputText.text;
	}

	public void ClientPopulateDisplayJoinCode()
	{
		joinCodeToDisplayText.text = ugsRelayMgr.JoinCode;
	}

	public void CallCloseOnReplayPanel()
	{
		StartCoroutine(closeRelayPaneAfterTime());
	}

	private IEnumerator closeRelayPaneAfterTime()
	{
		yield return new WaitForSeconds(relayPanelCloseDelay);
		closePanelAfterDelay?.TriggerEvent();
	}
}
