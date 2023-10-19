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

	[SerializeField] private float lobbyUpdateTime = 1.0f;

	private void OnEnable()
	{
		Debug.Log("ON ENABLE!!!");

		RelayManager.OnClientGameStarted += OnGameStarted;
		lobbyNameTxt.text = lobbyManager.GetCurrentLobbyName();
		playerNameTxt.text = lobbyManager.GetCurrentPlayerName();

		StartCoroutine(UpdateLobbyPlayerList());
	}

	private void OnDisable()
	{
		RelayManager.OnClientGameStarted -= OnGameStarted;
	}

	IEnumerator UpdateLobbyPlayerList()
	{
		while (gameObject.activeSelf) 
		{
			lobbyNameTxt.text = lobbyManager.GetCurrentLobbyName();
			playerNameTxt.text = lobbyManager.GetCurrentPlayerName();
			if (UpdatePlayerList())
			{
				yield return new WaitForSeconds(lobbyUpdateTime);
			}
			else
			{
				yield return null;
			}
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
	public bool UpdatePlayerList()
	{
		if (lobbyManager.CreatedOrJoinedLobby == null)
		{
			return false;
		}
		
		PlayerRecordConfigurator plyrc;
		var players = lobbyManager.GetPlayersInCurrentLobby();
		int loopLength = Mathf.Max(players.Count, playerRecords.Count);
		for (int i = 0; i < loopLength; i++)
		{
			if (i >= playerRecords.Count)
			{
				GameObject PlayerSingleRecordGO = Instantiate(playerSingleRecordPrefab, container);
				plyrc = PlayerSingleRecordGO.GetComponent<PlayerRecordConfigurator>();
				plyrc.PlayerId = players[i].Id;
				plyrc.PlayerName.text = players[i].Data["PlayerName"].Value;
				plyrc.PlayerColor.text = players[i].Data["PlayerColor"].Value;
				playerRecords.Add(PlayerSingleRecordGO);
			}
			else if (i >= players.Count)
			{
				int j;
				for (j = i; j < playerRecords.Count; ++j)
				{
					if (playerRecords[j] != null)
					{
						Destroy(playerRecords[j]);
					}
				}
				playerRecords.RemoveRange(i, j - i);
			}
			else
			{
				plyrc = playerRecords[i].GetComponent<PlayerRecordConfigurator>();
				plyrc.PlayerId = players[i].Id;
				plyrc.PlayerName.text = players[i].Data["PlayerName"].Value;
				plyrc.PlayerColor.text = players[i].Data["PlayerColor"].Value;
			}
		}
		return true;
	}

	private void OnGameStarted()
	{
		Debug.Log("JOIN LOBBY CLICKED BY CLIENT!!");
		playersPanel.SetActive(false);
	}
}
