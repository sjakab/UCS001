using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;

public class LobbyPlayersListUIManager : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyManager;
	[SerializeField] private GameObject playerSingleRecordPrefab;
	private List<GameObject> playerRecords;
	[SerializeField] private Transform container;

	//	[SerializeField] private Button refreshButton;
	[SerializeField] private TMP_Text lobbyNameTxt;
	[SerializeField] private TMP_Text playerNameTxt;
	[SerializeField] private GameObject playersPanel;

	[SerializeField] private float lobbyUpdateTimer = 5.0f;

	private void OnEnable()
	{
		Debug.Log("ON ENABLE!!!");

		RelayManager.OnClientGameStarted += OnGameStarted;
		lobbyNameTxt.text = lobbyManager.GetCurrentLobbyName();
		playerNameTxt.text = lobbyManager.GetCurrentPlayerName();

		StartCoroutine(UpdateLobby());
	}

	private void OnDisable()
	{
		RelayManager.OnClientGameStarted -= OnGameStarted;
	}

	IEnumerator UpdateLobby()
	{
		while (gameObject.activeSelf) 
		{
			lobbyNameTxt.text = lobbyManager.GetCurrentLobbyName();
			playerNameTxt.text = lobbyManager.GetCurrentPlayerName();
			UpdatePlayerList();
			yield return new WaitForSeconds(lobbyUpdateTimer);
		}
	}

	void Start()
	{
		playerRecords = new List<GameObject>();
		lobbyNameTxt.text = lobbyManager.GetCurrentLobbyName();
		playerNameTxt.text = lobbyManager.GetCurrentPlayerName();
	}

	public void OnClickLeaveLobbyButton()
	{
		// If player is host and another player is in the lobby migrate the host to the other player.
		List<Player> playersInLobby = lobbyManager.GetPlayersInCurrentLobby();

		if (lobbyManager.IsLobbyHost() && (playersInLobby.Count > 1) )
		{
			string newHostPlayerId;
			foreach(Player plyr in playersInLobby)
			{
				if (plyr.Id != AuthenticationService.Instance.PlayerId)
				{
					newHostPlayerId = plyr.Id;
					lobbyManager.MigrateLobbyHost(newHostPlayerId);
					break;
				}
			}
		}
		lobbyManager.LeaveLobby();
	}

	[ContextMenu("UpdatePlayersList")]
	public void UpdatePlayerList()
	{
		if (lobbyManager.CreatedOrJoinedLobby == null)
		{
			return;
		}
		
		PlayerRecordConfigurator plyrc;
		for (int i = 0; i < lobbyManager.GetPlayersInCurrentLobby().Count; i++)
		{
			if (i >= playerRecords.Count)
			{
				GameObject PlayerSingleRecordGO = Instantiate(playerSingleRecordPrefab, container);
				plyrc = PlayerSingleRecordGO.GetComponent<PlayerRecordConfigurator>();
				plyrc.PlayerId = lobbyManager.GetPlayersInCurrentLobby()[i].Id;
				plyrc.PlayerName.text = lobbyManager.GetPlayersInCurrentLobby()[i].Data["PlayerName"].Value;
				plyrc.PlayerColor.text = lobbyManager.GetPlayersInCurrentLobby()[i].Data["PlayerColor"].Value;
				playerRecords.Add(PlayerSingleRecordGO);
			}
			else
			{
				plyrc = playerRecords[i].GetComponent<PlayerRecordConfigurator>();
				plyrc.PlayerId = lobbyManager.GetPlayersInCurrentLobby()[i].Id;
				plyrc.PlayerName.text = lobbyManager.GetPlayersInCurrentLobby()[i].Data["PlayerName"].Value;
				plyrc.PlayerColor.text = lobbyManager.GetPlayersInCurrentLobby()[i].Data["PlayerColor"].Value;
			}
		}
	}

	private void OnGameStarted()
	{
		Debug.Log("JOIN LOBBY CLICKED BY CLIENT!!");
		playersPanel.SetActive(false);
	}
}
