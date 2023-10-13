using System.Collections.Generic;
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

	private float lobbyUpdateTimer;
	[SerializeField] private float pollForLobbyUIUpdatesIntervalInSeconds = 30f;


	private async void OnEnable()
	{
		Debug.Log("ON ENABLE!!!");
		LobbyRecordConfigurator.onJoinLobby += OnJoinLobbyClicked;
		await lobbyManager.ListLobbies();
		UpdateLobbyList();
	}

	private void Start()
	{
		lobbyRecords = new List<GameObject>();
	}

	// Update is called once per frame
	void Update()
	{
		lobbyUpdateTimer -= Time.deltaTime;
		if (lobbyUpdateTimer < 0f)
		{
			lobbyUpdateTimer = pollForLobbyUIUpdatesIntervalInSeconds;
			UpdateLobbyList();
		}
	}

	[ContextMenu("UpdateLobbies")]
	public void UpdateLobbyList()
	{
		LobbyRecordConfigurator lrc;
		for (int i = 0; i < lobbyManager.QueriedLobbies.Count; i++)
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
