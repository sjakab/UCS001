using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUIManager : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyManager;
	[SerializeField] private GameObject lobbySingleRecordPrefab;
	private List<GameObject> lobbyRecords;
	[SerializeField] private Transform container;
	[SerializeField] private GameObject lobbiesPanel;
	[SerializeField] private GameObject playersPanel;

	[SerializeField] private Button createLobbyButton;

	[SerializeField] private float lobbyUpdateTime = 5.0f;

	private async void OnEnable()
	{
		Debug.Log("ON ENABLE!!!");
		LobbyRecordConfigurator.onJoinLobby += OnJoinLobbyClicked;
		await lobbyManager.ListLobbies();
		StartCoroutine(UpdateLobbies());
	}

	private void Start()
	{
		lobbyRecords = new List<GameObject>();
	}

	IEnumerator UpdateLobbies()
	{
		while (gameObject.activeSelf)
		{
			UpdateLobbyList();
			yield return new WaitForSeconds(lobbyUpdateTime);
		}
	}

	[ContextMenu("UpdateLobbies")]
	public void UpdateLobbyList()
	{
		LobbyRecordConfigurator lrc;
		int loopLength = Mathf.Max(lobbyManager.QueriedLobbies.Count, lobbyRecords.Count);  
		for (int i = 0; i < loopLength; i++)
		{
			if (i >= lobbyRecords.Count)
			{
				GameObject lobbySingleRecordGO = Instantiate(lobbySingleRecordPrefab, container);
				lrc = lobbySingleRecordGO.GetComponent<LobbyRecordConfigurator>();
				lrc.LobbyId = lobbyManager.QueriedLobbies[i].Id;
				lrc.LobbyName.text = lobbyManager.QueriedLobbies[i].Name;
				lrc.LobbyGameMode.text = lobbyManager.QueriedLobbies[i].Data["GameMode"].Value;
				lrc.LobbyPlayerCount.text = lobbyManager.QueriedLobbies[i].MaxPlayers.ToString();
				lobbyRecords.Add(lobbySingleRecordGO);
			}
			else if (i >= lobbyManager.QueriedLobbies.Count)
			{
				int j;
				for (j = i; j < lobbyRecords.Count; ++j)
				{
					Destroy(lobbyRecords[j]);
				}
				lobbyRecords.RemoveRange(i, j - i);
				break;
			}
			else
			{
				lrc = lobbyRecords[i].GetComponent<LobbyRecordConfigurator>();
				lrc.LobbyId = lobbyManager.QueriedLobbies[i].Id;
				lrc.LobbyName.text = lobbyManager.QueriedLobbies[i].Name;
				lrc.LobbyGameMode.text = lobbyManager.QueriedLobbies[i].Data["GameMode"].Value;
				lrc.LobbyPlayerCount.text = lobbyManager.QueriedLobbies[i].MaxPlayers.ToString();
			}
		}
	}

	private void OnJoinLobbyClicked(string lobbyId)
	{
		Debug.Log("JOIN LOBBY CLICKED BY CLIENT!!");
		lobbiesPanel.SetActive(false);
		playersPanel.SetActive(true);
	}

	private void OnDisable()
	{
		LobbyRecordConfigurator.onJoinLobby -= OnJoinLobbyClicked;
	}
}
