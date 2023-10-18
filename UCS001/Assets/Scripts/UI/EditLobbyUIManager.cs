using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class EditLobbyUIManager : MonoBehaviour
{
	[SerializeField] private LobbyManager _lobbyManager;
	[SerializeField] private TMP_InputField _lobbyGameModeTxt;

	void OnEnable()
	{
		Lobby currentLobby = _lobbyManager.CreatedOrJoinedLobby;
		_lobbyGameModeTxt.text = currentLobby.Data["GameMode"].Value;
	}

	public void UpdateLobbyGameMode()
    {
	    _lobbyManager.UpdateLobbyGameMode(_lobbyGameModeTxt.text);
	}

	void OnDisable()
	{
		_lobbyGameModeTxt.text = "";
	}
}
