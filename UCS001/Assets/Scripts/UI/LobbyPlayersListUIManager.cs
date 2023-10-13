using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;

public class LobbyPlayersListUIManager : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyMgr;
	[SerializeField] private GameObject playerSingleRecordPrefab;
	private List<GameObject> playerRecords;
	[SerializeField] private Transform container;

	//	[SerializeField] private Button refreshButton;
	[SerializeField] private TMP_Text lobbyNameTxt;
	[SerializeField] private TMP_Text playerNameTxt;
	[SerializeField] private GameObject playersPanel;

	private float lobbyUpdateTimer;
	[SerializeField] private float pollForPlayerUIUpdatesIntervalInSeconds = 30f;


	private void OnEnable()
	{
		Debug.Log("ON ENABLE!!!");

		RelayManager.OnClientGameStarted += OnGameStarted;
		lobbyUpdateTimer = 2f;
		lobbyNameTxt.text = lobbyMgr.GetCurrentLobbyName();
		playerNameTxt.text = lobbyMgr.GetCurrentPlayerName();
	}

	void Start()
	{
		playerRecords = new List<GameObject>();
		lobbyNameTxt.text = lobbyMgr.GetCurrentLobbyName();
		playerNameTxt.text = lobbyMgr.GetCurrentPlayerName();
	}

	void Update()
	{
		lobbyUpdateTimer -= Time.deltaTime;
		if (lobbyUpdateTimer < 0f)
		{
			lobbyUpdateTimer = pollForPlayerUIUpdatesIntervalInSeconds;
			lobbyNameTxt.text = lobbyMgr.GetCurrentLobbyName();
			playerNameTxt.text = lobbyMgr.GetCurrentPlayerName();
			UpdatePlayerList();
		}
	}

	public void OnClickLeaveLobbyButton()
	{
		// If player is host and another player is in the lobby migrate the host to the other player.
		List<Player> playersInLobby = lobbyMgr.GetPlayersInCurrentLobby();

		if (lobbyMgr.IsLobbyHost() && (playersInLobby.Count > 1) )
		{
			string newHostPlayerId;
			foreach(Player plyr in playersInLobby)
			{
				if (plyr.Id != AuthenticationService.Instance.PlayerId)
				{
					newHostPlayerId = plyr.Id;
					lobbyMgr.MigrateLobbyHost(newHostPlayerId);
					break;
				}
			}
		}
		lobbyMgr.LeaveLobby();
	}

	[ContextMenu("UpdatePlayersList")]
	public void UpdatePlayerList()
	{
		PlayerRecordConfigurator plyrc;
		for (int i = 0; i < lobbyMgr.GetPlayersInCurrentLobby().Count; i++)
		{
			if (i >= playerRecords.Count)
			{
				GameObject PlayerSingleRecordGO = Instantiate(playerSingleRecordPrefab, container);
				plyrc = PlayerSingleRecordGO.GetComponent<PlayerRecordConfigurator>();
				plyrc.PlayerId = lobbyMgr.GetPlayersInCurrentLobby()[i].Id;
				plyrc.PlayerName.text = lobbyMgr.GetPlayersInCurrentLobby()[i].Data["PlayerName"].Value;
				plyrc.PlayerColor.text = lobbyMgr.GetPlayersInCurrentLobby()[i].Data["PlayerColor"].Value;
				playerRecords.Add(PlayerSingleRecordGO);
			}
			else
			{
				plyrc = playerRecords[i].GetComponent<PlayerRecordConfigurator>();
				plyrc.PlayerId = lobbyMgr.GetPlayersInCurrentLobby()[i].Id;
				plyrc.PlayerName.text = lobbyMgr.GetPlayersInCurrentLobby()[i].Data["PlayerName"].Value;
				plyrc.PlayerColor.text = lobbyMgr.GetPlayersInCurrentLobby()[i].Data["PlayerColor"].Value;
			}
		}
	}

	private void OnGameStarted()
	{
		Debug.Log("JOIN LOBBY CLICKED BY CLIENT!!");
		playersPanel.SetActive(false);
	}

	private void OnDisable()
	{
		RelayManager.OnClientGameStarted -= OnGameStarted;
	}

}
