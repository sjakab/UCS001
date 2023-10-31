using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class EditLobbyUIManager : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyManager;
	[SerializeField] private TMP_InputField lobbyGameModeTxt;

	void OnEnable()
	{
		Lobby currentLobby = lobbyManager.CreatedOrJoinedLobby;
		lobbyGameModeTxt.text = currentLobby.Data["GameMode"].Value;
	}

	public void UpdateLobbyGameMode()
    {
	    lobbyManager.UpdateLobbyGameMode(lobbyGameModeTxt.text);
	}

	void OnDisable()
	{
		lobbyGameModeTxt.text = "";
	}
}
