using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class EditLobbyUIManager : MonoBehaviour
{
	[SerializeField] private LobbyManager lobbyMgr;
	[SerializeField] private TMP_InputField lobbyGameModeTxt;

	void OnEnable()
	{
		Lobby currentLobby = lobbyMgr.CreatedOrJoinedLobby;
		lobbyGameModeTxt.text = currentLobby.Data["GameMode"].Value;
	}

	public void UpdateLobbyGameMode()
    {
		lobbyMgr.UpdateLobbyGameMode(lobbyGameModeTxt.text);
	}

	void OnDisable()
	{
		lobbyGameModeTxt.text = "";
	}
}
