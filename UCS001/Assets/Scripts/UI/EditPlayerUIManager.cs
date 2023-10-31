using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class EditPlayerUIManager : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyMgr;
	[SerializeField] private TMP_InputField playerNameTxt;
	[SerializeField] private TMP_Dropdown playerColorDD;

	private int lobbyIndexOfPlayer;
	// Index for player colors.
	private string[] PlayerColors = new string[] { "Red", "Blue", "Yellow", "Green"};

void OnEnable()
	{
		Lobby currentLobby = lobbyMgr.CreatedOrJoinedLobby;
		playerNameTxt.text = lobbyMgr.GetCurrentPlayerName();
		playerColorDD.value = GetIndexOfColor(lobbyMgr.AssignedPlayerColor);
	}

	public void UpdatePlayerData()
	{
		lobbyIndexOfPlayer = lobbyMgr.GetLobbyIndexOfCurrentPlayer();
		if (lobbyIndexOfPlayer == -1)
		{
			Debug.Log("Error - Unable to update player name and color. No index found for player in the lobby list");
		}
		else
		{
			lobbyMgr.AssignPlayerData(lobbyIndexOfPlayer);
			lobbyMgr.UpdatePlayerNameAndColor(playerNameTxt.text, PlayerColors[playerColorDD.value]);
		}
	}

	private int GetIndexOfColor(string col)
	{
		for(int i=0; i < PlayerColors.Length; i++)
		{
			if (PlayerColors[i].Equals(col))
			{
				return i;
			}
		}
		Debug.LogError("ERROR: Color not found");
		return -1;
	}
	void OnDisable()
	{
		playerNameTxt.text = "";
	}
}
