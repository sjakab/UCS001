using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyRecordConfigurator : MonoBehaviour
{
	public delegate void OnJoinLobbyUI(string lobbyId);
	public static OnJoinLobbyUI onJoinLobby;
	[SerializeField] private Button joinLobbyButton;
	[SerializeField] private TMPro.TMP_Text lobbyName;
	[SerializeField] private TMPro.TMP_Text lobbyGameMode;
	[SerializeField] private TMPro.TMP_Text lobbyPlayerCount;
	private string lobbyId;

	public TMP_Text LobbyName { get => lobbyName; set => lobbyName = value; }
	public TMP_Text LobbyGameMode { get => lobbyGameMode; set => lobbyGameMode = value; }
	public TMP_Text LobbyPlayerCount { get => lobbyPlayerCount; set => lobbyPlayerCount = value; }
	public string LobbyId { get => lobbyId; set => lobbyId = value; }

	private void Start()
	{
		joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
	}

	private void OnJoinLobbyClicked()
	{
		Debug.Log("ON JOIN LOBBY CLICKED");
		onJoinLobby.Invoke(lobbyId);
	}
}
